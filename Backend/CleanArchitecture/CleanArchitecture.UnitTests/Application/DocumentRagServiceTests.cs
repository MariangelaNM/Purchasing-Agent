using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Rag;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Rag;

namespace CleanArchitecture.UnitTests.Application;

public sealed class DocumentRagServiceTests
{
    [Fact]
    public async Task IngestAsync_EmbedsAndIndexesChunksFromSupportedDocuments()
    {
        var index = new FakeChunkIndex();
        var service = new DocumentRagService(
            new FakeDocumentReader([new SourceDocument("guide.md", "content")]),
            new FakeChunker(),
            new FakeEmbeddingGenerator(),
            index,
            new FakeAnswerGenerator());

        var result = await service.IngestAsync(new IngestDocumentsRequest("C:\\documents"));

        Assert.Equal(1, result.DocumentsProcessed);
        Assert.Equal(1, result.ChunksIndexed);
        Assert.True(index.WasEnsuredCreated);
        var chunk = Assert.Single(index.IndexedChunks);
        Assert.Equal("guide.md#0", chunk.Id);
        Assert.Equal([0.1f, 0.2f], chunk.Embedding);
    }

    [Fact]
    public async Task AskAsync_ReturnsAnswerAndRetrievedSources()
    {
        var chunk = CreateChunk();
        var index = new FakeChunkIndex { SearchResults = [new SearchResult(chunk, 0.5)] };
        var answerGenerator = new FakeAnswerGenerator("Grounded answer [1].");
        var service = new DocumentRagService(
            new FakeDocumentReader([]), new FakeChunker(), new FakeEmbeddingGenerator(), index,
            answerGenerator);

        var result = await service.AskAsync(new AskQuestionRequest("What is documented?"));

        Assert.Equal("Grounded answer [1].", result.Answer);
        Assert.Equal(2, result.Sources.Count);
        var source = result.Sources[0];
        Assert.Equal(chunk.SourceFile, source.SourceFile);
        Assert.Equal(0.5, source.Score);
        Assert.Equal("Inventario SQL", result.Sources[1].SourceFile);
        Assert.Equal(SearchMode.Hybrid, answerGenerator.Mode);
        Assert.Equal(8, answerGenerator.Top);
    }

    private static DocumentChunk CreateChunk() => new()
    {
        Id = "guide.md#0", SourceFile = "guide.md", Ordinal = 0, Content = "content", Embedding = [0.1f, 0.2f]
    };

    private sealed class FakeDocumentReader(IReadOnlyList<SourceDocument> documents) : IDocumentFileReader
    {
        public Task<IReadOnlyList<SourceDocument>> ReadSupportedDocumentsAsync(string folderPath, CancellationToken cancellationToken = default) => Task.FromResult(documents);
    }

    private sealed class FakeChunker : ITextChunker
    {
        public IReadOnlyList<DocumentChunk> Split(string sourceFile, string text, int targetCharacters, bool overlap) =>
            [new DocumentChunk { Id = $"{sourceFile}#0", SourceFile = sourceFile, Ordinal = 0, Content = text, Embedding = [] }];
    }

    private sealed class FakeEmbeddingGenerator : IEmbeddingGenerator
    {
        public Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken = default) => Task.FromResult(new[] { 0.1f, 0.2f });
        public Task<int> GetDimensionsAsync(CancellationToken cancellationToken = default) => Task.FromResult(2);
    }

    private sealed class FakeChunkIndex : IChunkIndex
    {
        public bool WasEnsuredCreated { get; private set; }
        public IReadOnlyList<DocumentChunk> IndexedChunks { get; private set; } = [];
        public IReadOnlyList<SearchResult> SearchResults { get; init; } = [];

        public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            WasEnsuredCreated = true;
            return Task.CompletedTask;
        }

        public Task IndexAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken = default)
        {
            IndexedChunks = chunks;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SearchResult>> SearchAsync(string query, SearchMode mode, int top, CancellationToken cancellationToken = default) => Task.FromResult(SearchResults);
        public Task<IReadOnlyList<DocumentChunk>> ListAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(IndexedChunks);
    }

    private sealed class FakeAnswerGenerator(string answer = "answer") : IAnswerGenerator
    {
        public SearchMode Mode { get; private set; }
        public int Top { get; private set; }

        public Task<GeneratedAnswer> GenerateAsync(
            string question,
            SearchMode mode,
            int top,
            CancellationToken cancellationToken = default)
        {
            Mode = mode;
            Top = top;
            return Task.FromResult(new GeneratedAnswer(
                answer,
                [
                    new SearchResult(CreateChunk(), 0.5),
                    new SearchResult(
                        new DocumentChunk
                        {
                            Id = "inventory-sql#0",
                            SourceFile = "Inventario SQL",
                            Ordinal = 0,
                            Content = "Stock disponible: 6",
                            Embedding = [],
                        },
                        1)
                ]));
        }
    }
}
