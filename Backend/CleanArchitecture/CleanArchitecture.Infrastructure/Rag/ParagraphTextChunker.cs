using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Rag;

namespace CleanArchitecture.Infrastructure.Rag;

public sealed class ParagraphTextChunker : ITextChunker
{
    public IReadOnlyList<DocumentChunk> Split(string sourceFile, string text, int targetCharacters, bool overlap)
    {
        var paragraphs = text.Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(paragraph => paragraph.Length > 0)
            .SelectMany(paragraph => SplitLongParagraph(paragraph, targetCharacters))
            .ToList();

        var chunks = new List<DocumentChunk>();
        var buffer = new List<string>();
        var bufferLength = 0;
        var ordinal = 0;

        foreach (var paragraph in paragraphs)
        {
            if (bufferLength > 0 && bufferLength + 2 + paragraph.Length > targetCharacters)
            {
                chunks.Add(CreateChunk(sourceFile, ordinal++, buffer));
                var carry = buffer[^1];
                buffer.Clear();
                bufferLength = 0;
                if (overlap)
                {
                    buffer.Add(carry);
                    bufferLength = carry.Length;
                }
            }

            buffer.Add(paragraph);
            bufferLength += (buffer.Count > 1 ? 2 : 0) + paragraph.Length;
        }

        if (buffer.Count > 0)
            chunks.Add(CreateChunk(sourceFile, ordinal, buffer));

        return chunks;
    }

    private static DocumentChunk CreateChunk(string sourceFile, int ordinal, List<string> paragraphs) => new()
    {
        Id = $"{sourceFile}#{ordinal}",
        SourceFile = sourceFile,
        Ordinal = ordinal,
        Content = string.Join("\n\n", paragraphs),
        Embedding = []
    };

    private static IEnumerable<string> SplitLongParagraph(string paragraph, int targetCharacters)
    {
        while (paragraph.Length > targetCharacters)
        {
            var splitAt = paragraph.LastIndexOfAny([' ', '\t', '\n'], targetCharacters - 1);
            if (splitAt <= 0)
                splitAt = targetCharacters;

            yield return paragraph[..splitAt].Trim();
            paragraph = paragraph[splitAt..].TrimStart();
        }

        if (paragraph.Length > 0)
            yield return paragraph;
    }
}
