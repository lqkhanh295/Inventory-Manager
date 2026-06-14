using Dapper;
using Eims.Application.Abstractions;
using Eims.Application.Dtos;
using Npgsql;

namespace Eims.Infrastructure.Services;

public sealed class ReportingService(NpgsqlDataSource dataSource) : IReportingService
{
    public async Task<IReadOnlyList<MovementHistoryRow>> GetMovementHistoryAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                m."Id" as "MovementId",
                m."ReferenceNo",
                m."Type",
                m."CreatedAt" as "PostedAt",
                fw."Code" as "FromWarehouse",
                tw."Code" as "ToWarehouse",
                p."Sku",
                p."Name" as "ProductName",
                l."Quantity",
                l."UnitCost"
            from eims."StockMovements" m
            join eims."StockMovementLines" l on l."StockMovementId" = m."Id"
            join eims."Products" p on p."Id" = l."ProductId"
            left join eims."Warehouses" fw on fw."Id" = m."FromWarehouseId"
            left join eims."Warehouses" tw on tw."Id" = m."ToWarehouseId"
            where (@from is null or m."CreatedAt" >= @from)
              and (@to is null or m."CreatedAt" <= @to)
            order by m."CreatedAt" desc
            limit 500
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<MovementHistoryRow>(new CommandDefinition(sql, new { from, to }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<StockValuationRow>> GetStockValuationAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            select
                p."Id" as "ProductId",
                p."Sku",
                p."Name" as "ProductName",
                w."Id" as "WarehouseId",
                w."Code" as "WarehouseCode",
                s."Quantity",
                p."UnitCost",
                s."Quantity" * p."UnitCost" as "TotalValue"
            from eims."StockItems" s
            join eims."Products" p on p."Id" = s."ProductId"
            join eims."Warehouses" w on w."Id" = s."WarehouseId"
            order by w."Code", p."Sku"
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<StockValuationRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}
