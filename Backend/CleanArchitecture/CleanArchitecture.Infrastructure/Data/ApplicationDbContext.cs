using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Stock> Stocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Product>().Property(p => p.Description).HasMaxLength(1000);
            modelBuilder.Entity<Stock>().HasKey(s => s.ProductId);
            modelBuilder.Entity<Stock>().Property(s => s.Quantity).IsRequired();
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Stock)
                .WithOne(s => s.Product)
                .HasForeignKey<Stock>(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            modelBuilder
                .Entity<Product>()
                .HasData(
                    new { Id = 1, Name = "Test Product 1", Description = "Description 1", Price = 19.99m },
                    new { Id = 2, Name = "Test Product 2", Description = "Description 2", Price = 29.99m }
                );

            modelBuilder.Entity<Stock>().HasData(
                new { ProductId = 1, Quantity = 12 },
                new { ProductId = 2, Quantity = 7 }
            );
        }
    }
}
