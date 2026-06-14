using Eims.Application.Abstractions;
using Eims.Infrastructure.Data;
using Eims.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Eims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing DefaultConnection.");

        services.AddDbContext<EimsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddHostedService<LowStockMonitorService>();
        return services;
    }
}
