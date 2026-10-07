using CleanArchitecture.Domain.Rag;

namespace CleanArchitecture.Application.Interfaces;

public interface ITextChunker
{
    IReadOnlyList<DocumentChunk> Split(string sourceFile, string text, int targetCharacters, bool overlap);
}

public interface IDocumentFileReader
{
    Task<IReadOnlyList<SourceDocument>> ReadSupportedDocumentsAsync(string folderPath, CancellationToken cancellationToken = default);
}

public sealed record SourceDocument(string RelativePath, string Content);

public interface IEmbeddingGenerator
{
    Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken = default);
    Task<int> GetDimensionsAsync(CancellationToken cancellationToken = default);
}

public interface IChunkIndex
{
    Task EnsureCreatedAsync(CancellationToken cancellationToken = default);
    Task IndexAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SearchResult>> SearchAsync(string query, SearchMode mode, int top, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentChunk>> ListAllAsync(CancellationToken cancellationToken = default);
}

public interface IAnswerGenerator
{
    Task<GeneratedAnswer> GenerateAsync(
        string question,
        SearchMode mode,
        int top,
        CancellationToken cancellationToken = default
    );
}

public sealed record GeneratedAnswer(string Answer, IReadOnlyList<SearchResult> Sources);
