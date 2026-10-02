#nullable enable

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CleanArchitecture.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Infrastructure.Rag;

public sealed class OllamaEmbeddingGenerator(
    IHttpClientFactory httpClientFactory,
    IOptions<RagOptions> options) : IEmbeddingGenerator
{
    private readonly RagOptions _options = options.Value;
    private int? _dimensions;

    public async Task<int> GetDimensionsAsync(CancellationToken cancellationToken = default)
    {
        if (_dimensions is not null)
            return _dimensions.Value;

        _dimensions = (await GenerateAsync("dimension probe", cancellationToken)).Length;
        return _dimensions.Value;
    }

    public async Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(RagHttpClientNames.Ollama);
        var response = await client.PostAsJsonAsync(
            $"{_options.OllamaUrl.TrimEnd('/')}/api/embeddings",
            new { model = _options.EmbeddingModel, prompt = text },
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Ollama failed to generate an embedding with model '{_options.EmbeddingModel}': {error}",
                null,
                response.StatusCode);
        }

        var body = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken);
        return body?.Embedding ?? throw new InvalidOperationException("Ollama returned an empty embedding response.");
    }

    private sealed record OllamaEmbeddingResponse([property: JsonPropertyName("embedding")] float[]? Embedding);
}
