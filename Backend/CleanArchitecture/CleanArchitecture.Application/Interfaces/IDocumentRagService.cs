using CleanArchitecture.Application.Rag;

namespace CleanArchitecture.Application.Interfaces;

public interface IDocumentRagService
{
    Task<IngestionResult> IngestAsync(IngestDocumentsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RetrievedChunkResponse>> SearchAsync(SearchDocumentsRequest request, CancellationToken cancellationToken = default);
    Task<AnswerQuestionResponse> AskAsync(AskQuestionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RetrievedChunkResponse>> ListChunksAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IndexedDocumentResponse>> ListDocumentsAsync(CancellationToken cancellationToken = default);
}
