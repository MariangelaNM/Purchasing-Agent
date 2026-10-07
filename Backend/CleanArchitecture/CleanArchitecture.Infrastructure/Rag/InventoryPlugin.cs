#nullable enable

using System.ComponentModel;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Rag;
using Microsoft.SemanticKernel;
using System.Text.RegularExpressions;

namespace CleanArchitecture.Infrastructure.Rag;

public sealed class InventoryPlugin(IProductRepository productRepository)
{
    public SearchResult? Source { get; private set; }

    [KernelFunction("get_available_products")]
    [Description("Obtiene precios y existencias de los productos asociados a las fichas técnicas clasificadas.")]
    public async Task<string> GetAvailableProductsAsync(
        [Description("Nombres de las fichas técnicas clasificadas, separados por saltos de línea.")] string specificationFiles
    )
    {
        var sourceFiles = specificationFiles
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(fileName => Path.GetFileName(fileName) ?? string.Empty)
            .Where(fileName => fileName.Length > 0)
            .Select(fileName => Regex.Replace(fileName, @"^\d+_", ""))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var products = await productRepository.GetBySpecificationFilesAsync(sourceFiles);
        var content = string.Join(
            "\n",
            products.Select(product =>
                $"Producto: {product.Name} | Precio unitario: {product.Price:0.00} | Stock disponible: {product.Stock?.Quantity ?? 0}"
            )
        );
        if (string.IsNullOrWhiteSpace(content))
            content = "No hay productos de las fichas técnicas clasificadas registrados en el inventario.";

        Source = new SearchResult(
            new DocumentChunk
            {
                Id = "inventory-sql#0",
                SourceFile = "Inventario SQL",
                Ordinal = 0,
                Content = content,
                Embedding = [],
            },
            1
        );
        return content;
    }
}
