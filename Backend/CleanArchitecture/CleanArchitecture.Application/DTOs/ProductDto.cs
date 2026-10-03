
namespace CleanArchitecture.Application.DTOs
{
    public record ProductDto(int Id, string Name, string Description, decimal Price, int Stock);

    public record CreateProductDto(string Name, string Description, decimal Price, int Stock = 0);

    public record UpdateProductDto(string Name, string Description, decimal Price, int Stock = 0);
}
