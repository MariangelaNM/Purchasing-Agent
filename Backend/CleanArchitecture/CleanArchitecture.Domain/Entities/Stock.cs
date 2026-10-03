namespace CleanArchitecture.Domain.Entities;

public class Stock
{
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Product Product { get; private set; } = null!;

    private Stock() { }

    public Stock(int quantity) => SetQuantity(quantity);

    public void SetQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(quantity));

        Quantity = quantity;
    }
}
