namespace CleanArchitecture.Domain.Rag;

public sealed record SearchResult(DocumentChunk Chunk, double Score);
