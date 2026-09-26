namespace CleanArchitecture.Infrastructure.Rag;

public sealed class RagOptions
{
    public const string SectionName = "Rag";
    public const string DefaultElasticsearchUrl = "http://localhost:9200";

    public string ElasticsearchUrl { get; set; } = DefaultElasticsearchUrl;
    public string OllamaUrl { get; set; } = "http://localhost:11434";
    public string EmbeddingModel { get; set; }
    public string ChatModel { get; set; }
    public string IndexName { get; set; } = "doc-chunks";
}

public static class RagHttpClientNames
{
    public const string Ollama = "Ollama";
}
