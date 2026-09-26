namespace CleanArchitecture.Application.Rag;

public sealed record IngestionResult(int DocumentsProcessed, int ChunksIndexed);

public sealed record RetrievedChunkResponse(string Id, string SourceFile, int Ordinal, string Content, double Score);

public sealed record IndexedDocumentResponse(string SourceFile, int ChunksIndexed);

public sealed record AnswerQuestionResponse(string Answer, IReadOnlyList<RetrievedChunkResponse> Sources);
