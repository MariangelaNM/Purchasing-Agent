using CleanArchitecture.Infrastructure.Rag;

namespace CleanArchitecture.UnitTests.Infrastructure;

public sealed class ParagraphTextChunkerTests
{
    [Fact]
    public void Split_WithOverlap_CarriesFinalParagraphIntoNextChunk()
    {
        var chunker = new ParagraphTextChunker();

        var chunks = chunker.Split("guide.md", "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.", 35, true);

        Assert.Equal(2, chunks.Count);
        Assert.Equal("First paragraph.\n\nSecond paragraph.", chunks[0].Content);
        Assert.Equal("Second paragraph.\n\nThird paragraph.", chunks[1].Content);
        Assert.Equal("guide.md#0", chunks[0].Id);
        Assert.Equal("guide.md#1", chunks[1].Id);
    }

    [Fact]
    public void Split_WithLongParagraph_RespectsTargetSize()
    {
        var chunks = new ParagraphTextChunker().Split("document.pdf", "one two three four five", 8, false);

        Assert.Equal(["one two", "three", "four", "five"], chunks.Select(chunk => chunk.Content));
        Assert.All(chunks, chunk => Assert.True(chunk.Content.Length <= 8));
    }
}
