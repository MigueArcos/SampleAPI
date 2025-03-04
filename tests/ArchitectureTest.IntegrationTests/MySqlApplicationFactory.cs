using System;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Infrastructure.SqlEFCore;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MySql;
using Xunit;
using SqlServerDatabase = ArchitectureTest.Databases.SqlServer;

namespace ArchitectureTest.IntegrationTests;
public class MySqlApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MySqlContainer _mySqlContainer = new MySqlBuilder()
        .WithImage("mysql:latest")
        .WithBindMount($"{CommonDirectoryPath.GetSolutionDirectory().DirectoryPath}/mysql/", "/docker-entrypoint-initdb.d/")
        .WithName("mysql_container_it")
        .WithDatabase("crud")
        .WithUsername("self")
        .WithPassword("P455w0rd")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings:MySql",
            _mySqlContainer.GetConnectionString()
        );
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        builder
            .ConfigureAppConfiguration(builder => {
                builder.Sources.Clear();
                builder.AddConfiguration(configuration);
            })
            .ConfigureServices(services => {
                var msSqlContext = services.SingleOrDefault(d => d.ServiceType == typeof(SqlServerDatabase.DatabaseContext))!;
                if (msSqlContext != null)
                {
                    var automapperAdnEfCoreServices = services.Where(s =>
                        s.ServiceType.ToString().Contains("AutoMapper") || 
                        s.ServiceType.ToString().Contains("EntityFrameworkCore")
                    ).ToList();
                    var unitOfWorkService = services.SingleOrDefault(d => d.ServiceType == typeof(IUnitOfWork))!;
                    services.Remove(msSqlContext);
                    services.Remove(unitOfWorkService);
                    automapperAdnEfCoreServices.ForEach(s => {
                        services.Remove(s);
                    });
                    services.AddMySqlConfiguration(configuration);
                }
            });
    }

    public async Task InitializeAsync()
    {
        await _mySqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _mySqlContainer.DisposeAsync();
    }
}
