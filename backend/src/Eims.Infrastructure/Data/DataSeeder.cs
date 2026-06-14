using Eims.Domain.Entities;
using Eims.Domain.Enums;
using Eims.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Eims.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(EimsDbContext db, string? adminPassword, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("SeedAdmin:Password must be configured before the first run.");
        }

        var rootCategory = new Category { Name = "General" };
        var mainWarehouse = new Warehouse { Code = "MAIN", Name = "Main Warehouse" };
        var product = new Product
        {
            Sku = "SKU-DEMO-001",
            Name = "Demo Item",
            Category = rootCategory,
            UnitCost = 100,
            MinStockLevel = 10
        };

        db.Users.Add(new User
        {
            UserName = "admin",
            PasswordHash = PasswordHasher.Hash(adminPassword),
            Role = UserRole.Admin,
            IsActive = true
        });
        db.Categories.Add(rootCategory);
        db.Warehouses.Add(mainWarehouse);
        db.Products.Add(product);
        db.StockItems.Add(new StockItem { Product = product, Warehouse = mainWarehouse, Quantity = 50 });
        await db.SaveChangesAsync(cancellationToken);
    }
}
