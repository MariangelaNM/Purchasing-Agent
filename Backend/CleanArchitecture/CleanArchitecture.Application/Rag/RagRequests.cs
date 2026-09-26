using CleanArchitecture.Domain.Rag;

namespace CleanArchitecture.Application.Rag;

public sealed record IngestDocumentsRequest(
    string FolderPath,
    int ChunkSize = 1000,
    bool Overlap = true
);

public sealed record SearchDocumentsRequest(
    string Query,
    SearchMode Mode = SearchMode.Hybrid,
    int Top = 5
);

public sealed record AskQuestionRequest(
    string Question,
    SearchMode Mode = SearchMode.Hybrid,
    int Top = 5
);
