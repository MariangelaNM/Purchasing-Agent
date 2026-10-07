using System.ComponentModel;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Rag;
using Microsoft.SemanticKernel;

namespace CleanArchitecture.Infrastructure.Rag;

public sealed class KnowledgePlugin(IChunkIndex chunkIndex)
{
    public IReadOnlyList<SearchResult> Sources { get; private set; } = [];

    [KernelFunction("search_specifications")]
    [Description("Busca especificaciones técnicas relevantes en los documentos oficiales cargados.")]
    public async Task<string> SearchSpecificationsAsync(
        [Description("Pregunta o requisitos técnicos de la cotización.")] string query,
        [Description("Cantidad máxima de fragmentos relevantes.")] int top,
        SearchMode mode,
        CancellationToken cancellationToken = default
    )
    {
        Sources = await chunkIndex.SearchAsync(query, mode, top, cancellationToken);
        return Sources.Count == 0
            ? "No se encontraron especificaciones técnicas relevantes."
            : string.Join("\n\n", Sources.Select(result => result.Chunk.Content));
    }
}
