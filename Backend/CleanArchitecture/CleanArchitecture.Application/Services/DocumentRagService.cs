using System.Diagnostics;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Rag;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Rag;

namespace CleanArchitecture.Application.Services;

public sealed class DocumentRagService(
    IDocumentFileReader documentReader,
    ITextChunker chunker,
    IEmbeddingGenerator embeddingGenerator,
    IChunkIndex chunkIndex,
    IAnswerGenerator answerGenerator,
    IProductRepository productRepository
) : IDocumentRagService
{
    public async Task<IngestionResult> IngestAsync(
        IngestDocumentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        Stopwatch swEmbeddingGeneratorTotal = new Stopwatch();
        swEmbeddingGeneratorTotal.Start();
        ValidateFolderPath(request.FolderPath);
        ValidateChunkSize(request.ChunkSize);

        var documents = await documentReader.ReadSupportedDocumentsAsync(
            request.FolderPath,
            cancellationToken
        );
        var chunks = new List<DocumentChunk>();
        Stopwatch swEmbeddingGenerator = new Stopwatch();
        foreach (var document in documents)
        {
            foreach (
                var chunk in chunker.Split(
                    document.RelativePath,
                    document.Content,
                    request.ChunkSize,
                    request.Overlap
                )
            )
            {
                swEmbeddingGenerator.Start();
                var embedding = await embeddingGenerator.GenerateAsync(
                    chunk.Content,
                    cancellationToken
                );
                swEmbeddingGenerator.Stop();
                cancellationToken.ThrowIfCancellationRequested();
                chunks.Add(
                    new DocumentChunk
                    {
                        Id = $"{chunk.SourceFile}#{chunk.Ordinal}",
                        SourceFile = chunk.SourceFile,
                        Ordinal = chunk.Ordinal,
                        Content = chunk.Content,
                        Embedding = embedding,
                    }
                );
            }
        }
        Debug.WriteLine(
            "EmbeddingGeneratorTimeSeconds " + (swEmbeddingGenerator.ElapsedMilliseconds / 1000)
        );
        await chunkIndex.EnsureCreatedAsync(cancellationToken);
        if (chunks.Count > 0)
            await chunkIndex.IndexAsync(chunks, cancellationToken);
        swEmbeddingGeneratorTotal.Stop();
        Debug.WriteLine(
            "EmbeddingGeneratorTotalTimeSeconds "
                + (swEmbeddingGeneratorTotal.ElapsedMilliseconds / 1000)
        );
        return new IngestionResult(documents.Count, chunks.Count);
    }

    public async Task<IReadOnlyList<RetrievedChunkResponse>> SearchAsync(
        SearchDocumentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ValidateSearch(request.Query, request.Top);
        var results = await chunkIndex.SearchAsync(
            request.Query,
            request.Mode,
            request.Top,
            cancellationToken
        );
        return results.Select(ToResponse).ToList();
    }

    public async Task<AnswerQuestionResponse> AskAsync(
        AskQuestionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ValidateSearch(request.Question, request.Top);
        var results = await chunkIndex.SearchAsync(
            request.Question,
            request.Mode,
            request.Top,
            cancellationToken
        );
        var inventory = await productRepository.GetAllAsync();
        var answerContext = results.Concat([CreateInventoryContext(inventory)]).ToList();
        var answer = await answerGenerator.GenerateAsync(
            request.Question,
            answerContext,
            cancellationToken
        );
        return new AnswerQuestionResponse(answer, answerContext.Select(ToResponse).ToList());
    }

    public async Task<IReadOnlyList<RetrievedChunkResponse>> ListChunksAsync(
        CancellationToken cancellationToken = default
    ) =>
        (await chunkIndex.ListAllAsync(cancellationToken))
            .Select(chunk => new RetrievedChunkResponse(
                chunk.Id,
                chunk.SourceFile,
                chunk.Ordinal,
                chunk.Content,
                0
            ))
            .ToList();

    public async Task<IReadOnlyList<IndexedDocumentResponse>> ListDocumentsAsync(
        CancellationToken cancellationToken = default
    ) => (await chunkIndex.ListAllAsync(cancellationToken))
        .GroupBy(chunk => chunk.SourceFile)
        .OrderBy(group => group.Key)
        .Select(group => new IndexedDocumentResponse(group.Key, group.Count()))
        .ToList();

    private static RetrievedChunkResponse ToResponse(SearchResult result) =>
        new(
            result.Chunk.Id,
            result.Chunk.SourceFile,
            result.Chunk.Ordinal,
            result.Chunk.Content,
            result.Score
        );

    private static SearchResult CreateInventoryContext(IEnumerable<Product> products)
    {
        var rows = products.Select(product =>
            $"Producto: {product.Name} | Precio unitario: {product.Price:0.00} | Stock disponible: {product.Stock?.Quantity ?? 0}"
        );
        var content = string.Join("\n", rows);
        if (string.IsNullOrWhiteSpace(content))
            content = "No hay productos registrados en el inventario.";

        return new SearchResult(
            new DocumentChunk
            {
                Id = "inventory-sql#0",
                SourceFile = "Inventario SQL",
                Ordinal = 0,
                Content = content,
                Embedding = [],
            },
            1
        );
    }

    private static void ValidateFolderPath(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("A document folder path is required.", nameof(folderPath));
    }

    private static void ValidateChunkSize(int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(chunkSize),
                "Chunk size must be greater than zero."
            );
    }

    private static void ValidateSearch(string query, int top)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("A query is required.", nameof(query));
        if (top <= 0)
            throw new ArgumentOutOfRangeException(nameof(top), "Top must be greater than zero.");
    }
}
