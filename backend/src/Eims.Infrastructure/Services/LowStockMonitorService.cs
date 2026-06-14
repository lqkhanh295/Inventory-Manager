using Eims.Domain.Entities;
using Eims.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eims.Infrastructure.Services;

public sealed class LowStockMonitorService(IServiceScopeFactory scopeFactory, ILogger<LowStockMonitorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ScanAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Low-stock monitor failed.");
            }
        }
    }

    private async Task ScanAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EimsDbContext>();
        var lowStocks = await db.StockItems
            .Include(x => x.Product)
            .Where(x => x.Product != null && x.Quantity <= x.Product.MinStockLevel)
            .ToListAsync(cancellationToken);

        foreach (var stock in lowStocks)
        {
            var exists = await db.LowStockNotifications.AnyAsync(x =>
                x.ProductId == stock.ProductId &&
                x.WarehouseId == stock.WarehouseId &&
                !x.IsResolved,
                cancellationToken);

            if (!exists)
            {
                db.LowStockNotifications.Add(new LowStockNotification
                {
                    ProductId = stock.ProductId,
                    WarehouseId = stock.WarehouseId,
                    CurrentQuantity = stock.Quantity,
                    MinStockLevel = stock.Product!.MinStockLevel
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
