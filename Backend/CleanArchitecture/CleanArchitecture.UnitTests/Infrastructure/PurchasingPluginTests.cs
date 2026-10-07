using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Rag;
using CleanArchitecture.Infrastructure.Rag;
using Microsoft.SemanticKernel;

namespace CleanArchitecture.UnitTests.Infrastructure;

public sealed class PurchasingPluginTests
{
    [Fact]
    public async Task KnowledgePlugin_SearchesSpecificationsAndKeepsSources()
    {
        var result = new SearchResult(CreateChunk(), 0.8);
        var plugin = new KnowledgePlugin(new FakeChunkIndex([result]));
        var kernel = Kernel.CreateBuilder().Build();
        kernel.Plugins.AddFromObject(plugin, "Knowledge");

        var response = await kernel.InvokeAsync(
            "Knowledge",
            "search_specifications",
            new KernelArguments
            {
                ["query"] = "16 GB RAM",
                ["top"] = 8,
                ["mode"] = SearchMode.Hybrid,
            }
        );

        Assert.Equal("Technical specifications", response.ToString());
        Assert.Same(result, Assert.Single(plugin.Sources));
    }

    [Fact]
    public async Task InventoryPlugin_ReturnsPricesAndStockAsSource()
    {
        var plugin = new InventoryPlugin(new FakeProductRepository());

        var content = await plugin.GetAvailableProductsAsync("0_thinkpad-spec.pdf");

        Assert.Contains("ThinkPad", content);
        Assert.Contains("Precio unitario: 1000.00", content);
        Assert.Contains("Stock disponible: 6", content);
        Assert.Equal("Inventario SQL", plugin.Source?.Chunk.SourceFile);
    }

    private static DocumentChunk CreateChunk() => new()
    {
        Id = "spec.pdf#0",
        SourceFile = "spec.pdf",
        Ordinal = 0,
        Content = "Technical specifications",
        Embedding = [0.1f],
    };

    private sealed class FakeChunkIndex(IReadOnlyList<SearchResult> results) : IChunkIndex
    {
        public Task EnsureCreatedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task IndexAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<SearchResult>> SearchAsync(string query, SearchMode mode, int top, CancellationToken cancellationToken = default) => Task.FromResult(results);
        public Task<IReadOnlyList<DocumentChunk>> ListAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<DocumentChunk>>([]);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly List<Product> _products = [new Product("ThinkPad", "Business laptop", 1000m, 6)];

        public Task<Product> GetByIdAsync(int id) => Task.FromResult(_products[0]);
        public Task<IEnumerable<Product>> GetAllAsync() => Task.FromResult<IEnumerable<Product>>(_products);
        public Task<IEnumerable<Product>> GetBySpecificationFilesAsync(IEnumerable<string> sourceFiles)
        {
            Assert.Equal(["thinkpad-spec.pdf"], sourceFiles);
            return Task.FromResult<IEnumerable<Product>>(_products);
        }
        public Task<Product> AddAsync(Product product) => Task.FromResult(product);
        public Task UpdateAsync(Product product) => Task.CompletedTask;
        public Task DeleteAsync(int id) => Task.CompletedTask;
    }
}
