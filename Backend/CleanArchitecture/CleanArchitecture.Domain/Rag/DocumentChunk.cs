namespace CleanArchitecture.Domain.Rag;

public sealed class DocumentChunk
{
    public required string Id { get; init; }
    public required string SourceFile { get; init; }
    public required int Ordinal { get; init; }
    public required string Content { get; init; }
    public required float[] Embedding { get; init; }
}
