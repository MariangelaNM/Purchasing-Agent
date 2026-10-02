using System.Text.Json;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Rag;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Infrastructure.Rag;

public sealed class ElasticsearchChunkIndex(
    ElasticsearchClient client,
    IEmbeddingGenerator embeddingGenerator,
    IOptions<RagOptions> options) : IChunkIndex
{
    private const int RrfK = 60;
    private readonly string _indexName = options.Value.IndexName;

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        var exists = await client.Indices.ExistsAsync(_indexName, cancellationToken);
        if (exists.Exists)
            return;

        var dimensions = await embeddingGenerator.GetDimensionsAsync(cancellationToken);
        var create = await client.Indices.CreateAsync(_indexName, c => c
            .Mappings(m => m.Properties<DocumentChunk>(p => p
                .Keyword(chunk => chunk.SourceFile)
                .IntegerNumber(chunk => chunk.Ordinal)
                .Text(chunk => chunk.Content)
                .DenseVector(chunk => chunk.Embedding, vector => vector
                    .Dims(dimensions)
                    .Similarity(DenseVectorSimilarity.Cosine)))), cancellationToken);
        if (!create.IsValidResponse)
            throw new InvalidOperationException($"Failed to create Elasticsearch index: {create.DebugInformation}");
    }

    public async Task IndexAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        if (chunks.Count == 0)
            return;

        var bulk = await client.BulkAsync(b => b.Index(_indexName).IndexMany(chunks, (operation, chunk) => operation.Id(chunk.Id)), cancellationToken);
        if (bulk.Errors)
            throw new InvalidOperationException($"Elasticsearch bulk indexing had errors: {bulk.DebugInformation}");

        await client.Indices.RefreshAsync(_indexName, cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunk>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var index = await client.Indices.ExistsAsync(_indexName, cancellationToken);
        if (!index.IsValidResponse)
            throw new InvalidOperationException($"Failed to query Elasticsearch index: {index.DebugInformation}");
        if (!index.Exists)
            return [];

        var response = await client.SearchAsync<Dictionary<string, JsonElement>>(search => search
            .Indices(_indexName)
            .Size(1000)
            .Query(query => query.MatchAll()), cancellationToken);
        if (!response.IsValidResponse)
            throw new InvalidOperationException($"Failed to list Elasticsearch documents: {response.DebugInformation}");

        return response.Hits.Where(hit => hit.Source is not null).Select(hit => ToChunk(hit.Source!, hit.Id))
            .OrderBy(chunk => chunk.SourceFile).ThenBy(chunk => chunk.Ordinal).ToList();
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string query, SearchMode mode, int top, CancellationToken cancellationToken = default)
    {
        if (mode == SearchMode.Hybrid)
            return await HybridSearchAsync(query, top, cancellationToken);

        var hits = mode == SearchMode.Bm25
            ? await Bm25Async(query, top, cancellationToken)
            : await KnnAsync(query, top, cancellationToken);
        return ToResults(hits);
    }

    private async Task<IReadOnlyList<SearchResult>> HybridSearchAsync(string query, int top, CancellationToken cancellationToken)
    {
        const int perBranch = 20;
        var bm25Task = Bm25Async(query, perBranch, cancellationToken);
        var knnTask = KnnAsync(query, perBranch, cancellationToken);
        await Task.WhenAll(bm25Task, knnTask);

        var fused = new Dictionary<string, (DocumentChunk Chunk, double Score)>();
        foreach (var ranking in new[] { await bm25Task, await knnTask })
        {
            var rank = 0;
            foreach (var hit in ranking)
            {
                if (hit.Source is not { } chunk)
                    continue;
                var documentChunk = ToChunk(chunk, hit.Id);
                rank++;
                var contribution = 1.0 / (RrfK + rank);
                fused[documentChunk.Id] = fused.TryGetValue(documentChunk.Id, out var current)
                    ? (current.Chunk, current.Score + contribution)
                    : (documentChunk, contribution);
            }
        }

        return fused.Values.OrderByDescending(entry => entry.Score).Take(top)
            .Select(entry => new SearchResult(entry.Chunk, entry.Score)).ToList();
    }

    private async Task<IReadOnlyList<Hit<Dictionary<string, JsonElement>>>> Bm25Async(string query, int size, CancellationToken cancellationToken)
    {
        var response = await client.SearchAsync<Dictionary<string, JsonElement>>(search => search
            .Indices(_indexName)
            .Size(size)
            .Query(filter => filter.Match(match => match.Field("content").Query(query))), cancellationToken);
        return response.Hits.ToList();
    }

    private async Task<IReadOnlyList<Hit<Dictionary<string, JsonElement>>>> KnnAsync(string query, int size, CancellationToken cancellationToken)
    {
        var queryVector = await embeddingGenerator.GenerateAsync(query, cancellationToken);
        var response = await client.SearchAsync<Dictionary<string, JsonElement>>(search => search
            .Indices(_indexName)
            .Size(size)
            .Knn(knn => knn.Field("embedding").QueryVector(queryVector).K(size).NumCandidates(100)), cancellationToken);
        return response.Hits.ToList();
    }

    private static IReadOnlyList<SearchResult> ToResults(IReadOnlyList<Hit<Dictionary<string, JsonElement>>> hits) => hits
        .Where(hit => hit.Source is not null)
        .Select(hit => new SearchResult(ToChunk(hit.Source!, hit.Id), hit.Score ?? 0))
        .ToList();

    private static DocumentChunk ToChunk(IReadOnlyDictionary<string, JsonElement> source, string id) => new()
    {
        Id = id,
        SourceFile = GetRequiredString(source, "sourceFile"),
        Ordinal = GetOrdinal(source),
        Content = GetRequiredString(source, "content"),
        Embedding = [],
    };

    private static string GetRequiredString(IReadOnlyDictionary<string, JsonElement> source, string field) =>
        source.TryGetValue(field, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? throw new InvalidOperationException($"Document chunk field '{field}' is null.")
            : throw new InvalidOperationException($"Document chunk field '{field}' is missing.");

    private static int GetOrdinal(IReadOnlyDictionary<string, JsonElement> source)
    {
        if (!source.TryGetValue("ordinal", out var value))
            return 0;

        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var ordinal)
            ? ordinal
            : value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out ordinal)
                ? ordinal
                : 0;
    }
}
