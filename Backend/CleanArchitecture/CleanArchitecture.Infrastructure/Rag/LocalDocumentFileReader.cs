using CleanArchitecture.Application.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;

namespace CleanArchitecture.Infrastructure.Rag;

public sealed class LocalDocumentFileReader : IDocumentFileReader
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".txt", ".pdf", ".docx"
    };

    public async Task<IReadOnlyList<SourceDocument>> ReadSupportedDocumentsAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var isFile = File.Exists(folderPath);
        if (!isFile && !Directory.Exists(folderPath))
            throw new FileNotFoundException($"Document path '{folderPath}' does not exist.", folderPath);

        IEnumerable<string> files = isFile
            ? [folderPath]
            : Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                .Where(file => SupportedExtensions.Contains(Path.GetExtension(file)))
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase);
        var documents = new List<SourceDocument>();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!SupportedExtensions.Contains(Path.GetExtension(file)))
                throw new ArgumentException($"Unsupported document type '{Path.GetExtension(file)}'.", nameof(folderPath));

            var content = await ReadDocumentAsync(file, cancellationToken);
            var sourceFile = isFile ? Path.GetFileName(file) : Path.GetRelativePath(folderPath, file);
            documents.Add(new SourceDocument(sourceFile, content));
        }

        return documents;
    }

    private static async Task<string> ReadDocumentAsync(string file, CancellationToken cancellationToken)
    {
        return Path.GetExtension(file).ToLowerInvariant() switch
        {
            ".txt" or ".md" => await File.ReadAllTextAsync(file, cancellationToken),
            ".pdf" => ReadPdf(file, cancellationToken),
            ".docx" => ReadWordDocument(file, cancellationToken),
            _ => throw new InvalidOperationException()
        };
    }

    private static string ReadPdf(string file, CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(file);
        return string.Join(Environment.NewLine, document.GetPages().Select(page =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return page.Text;
        }));
    }

    private static string ReadWordDocument(string file, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var document = WordprocessingDocument.Open(file, false);
        return document.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
    }
}
