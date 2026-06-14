using System.Text.Json;
using Eims.Application.Abstractions;
using Eims.Domain.Common;
using Eims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Eims.Infrastructure.Data;

public sealed class EimsDbContext(DbContextOptions<EimsDbContext> options, ICurrentUser currentUser) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockMovementLine> StockMovementLines => Set<StockMovementLine>();
    public DbSet<InventoryAdjustment> InventoryAdjustments => Set<InventoryAdjustment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LowStockNotification> LowStockNotifications => Set<LowStockNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("eims");
        modelBuilder.Entity<User>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<Category>().HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Product>().HasIndex(x => x.Sku).IsUnique();
        modelBuilder.Entity<Product>().Property(x => x.UnitCost).HasPrecision(18, 2);
        modelBuilder.Entity<Product>().Property(x => x.MinStockLevel).HasPrecision(18, 2);
        modelBuilder.Entity<Warehouse>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Warehouse>().HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<StockItem>().HasIndex(x => new { x.ProductId, x.WarehouseId }).IsUnique();
        modelBuilder.Entity<StockItem>().Property(x => x.Quantity).HasPrecision(18, 3);
        modelBuilder.Entity<StockMovement>().HasIndex(x => x.ReferenceNo).IsUnique();
        modelBuilder.Entity<StockMovementLine>().Property(x => x.Quantity).HasPrecision(18, 3);
        modelBuilder.Entity<StockMovementLine>().Property(x => x.UnitCost).HasPrecision(18, 2);
        modelBuilder.Entity<InventoryAdjustment>().Property(x => x.SystemQuantity).HasPrecision(18, 3);
        modelBuilder.Entity<InventoryAdjustment>().Property(x => x.ActualQuantity).HasPrecision(18, 3);
        modelBuilder.Entity<LowStockNotification>().Property(x => x.CurrentQuantity).HasPrecision(18, 3);
        modelBuilder.Entity<LowStockNotification>().Property(x => x.MinStockLevel).HasPrecision(18, 3);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.RowVersion)).IsConcurrencyToken();
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAudit()
    {
        var now = DateTimeOffset.UtcNow;
        var userName = currentUser.UserName ?? "system";
        var auditLogs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userName;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userName;
            }

            entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
            auditLogs.Add(CreateAuditLog(entry, userName, now));
        }

        if (auditLogs.Count > 0)
        {
            AuditLogs.AddRange(auditLogs);
        }
    }

    private static AuditLog CreateAuditLog(EntityEntry<AuditableEntity> entry, string userName, DateTimeOffset now)
    {
        var oldValues = entry.State == EntityState.Added
            ? null
            : entry.Properties.Where(p => p.IsModified).ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
        var newValues = entry.State == EntityState.Deleted
            ? null
            : entry.Properties.Where(p => entry.State == EntityState.Added || p.IsModified).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

        return new AuditLog
        {
            TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            Action = entry.State.ToString(),
            ChangedBy = userName,
            ChangedAt = now,
            OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues),
            NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues)
        };
    }
}
