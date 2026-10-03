
namespace CleanArchitecture.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public Stock Stock { get; private set; } = null!;

        private Product() { }

        public Product(string name, string description, decimal price, int stock = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero", nameof(price));
            if (stock < 0)
                throw new ArgumentException("Stock cannot be negative", nameof(stock));

            Name = name;
            Description = description ?? "";
            Price = price;
            Stock = new Stock(stock);
        }

        public void UpdateDetails(string name, string description, decimal price, int stock)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero", nameof(price));
            if (stock < 0)
                throw new ArgumentException("Stock cannot be negative", nameof(stock));

            Name = name;
            Description = description ?? "";
            Price = price;
            if (Stock is null)
                Stock = new Stock(stock);
            else
                Stock.SetQuantity(stock);
        }
    }
}
