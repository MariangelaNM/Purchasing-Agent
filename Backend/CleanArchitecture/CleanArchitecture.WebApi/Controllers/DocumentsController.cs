using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Rag;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController(IDocumentRagService documentRagService) : ControllerBase
{
    [HttpPost("ingest-file")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IngestionResult>> IngestFile(
        [FromForm] IngestFileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { error = "A non-empty document file is required." });

        var extension = Path.GetExtension(request.File.FileName);
        if (
            !new[] { ".txt", ".md", ".pdf", ".docx" }.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase
            )
        )
            return BadRequest(
                new { error = "Supported file types are .txt, .md, .pdf, and .docx." }
            );

        var uploadDirectory = Path.Combine(
            Path.GetTempPath(),
            "CleanArchitecture",
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(uploadDirectory);
        var uploadPath = Path.Combine(uploadDirectory, Path.GetFileName(request.File.FileName));

        try
        {
            await using (var stream = System.IO.File.Create(uploadPath))
                await request.File.CopyToAsync(stream, cancellationToken);

            return Ok(
                await documentRagService.IngestAsync(
                    new IngestDocumentsRequest(uploadPath, request.ChunkSize, request.Overlap),
                    cancellationToken
                )
            );
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (HttpRequestException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = exception.Message });
        }
        finally
        {
            Directory.Delete(uploadDirectory, recursive: true);
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<RetrievedChunkResponse>>> Search(
        SearchDocumentsRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return Ok(await documentRagService.SearchAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("ask")]
    public async Task<ActionResult<AnswerQuestionResponse>> Ask(
        AskQuestionRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return Ok(await documentRagService.AskAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("chunks")]
    public async Task<ActionResult<IReadOnlyList<RetrievedChunkResponse>>> ListChunks(
        CancellationToken cancellationToken
    ) => Ok(await documentRagService.ListChunksAsync(cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IndexedDocumentResponse>>> ListDocuments(
        CancellationToken cancellationToken
    )
    {
        try
        {
            return Ok(await documentRagService.ListDocumentsAsync(cancellationToken));
        }
        catch (HttpRequestException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = exception.Message });
        }
    }
}

public sealed class IngestFileRequest
{
    public IFormFile? File { get; init; }
    public int ChunkSize { get; init; } = 1000;
    public bool Overlap { get; init; } = true;
}
