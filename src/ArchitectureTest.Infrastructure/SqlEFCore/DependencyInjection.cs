using System.Diagnostics.CodeAnalysis;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Infrastructure.SqlEFCore.MySql;
using ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MySqlDatabse = ArchitectureTest.Databases.MySql;
using SqlServerDatabase = ArchitectureTest.Databases.SqlServer;

namespace ArchitectureTest.Infrastructure.SqlEFCore;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddSqlServerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");

        services.AddAutoMapper(typeof(SqlServerMappingProfile));
        services.AddDbContext<SqlServerDatabase.DatabaseContext>(
            options => options.UseSqlServer(connectionString)
        );
        services.AddScoped<IUnitOfWork, SqlSeverUnitOfWork>();
    }

    public static void AddMySqlConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySql");

        services.AddAutoMapper(typeof(MySqlMappingProfile));
        services.AddDbContext<MySqlDatabse.DatabaseContext>(
            options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        );
        services.AddScoped<IUnitOfWork, MySqlUnitOfWork>();
    }
}
