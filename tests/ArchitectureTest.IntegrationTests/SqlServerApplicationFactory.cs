using System;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Infrastructure.SqlEFCore;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;
using Xunit;
using MySqlDatabase = ArchitectureTest.Databases.MySql;

namespace ArchitectureTest.IntegrationTests;
public class SqlServerApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly IFutureDockerImage _msSqlImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "sql_server_setup")
        .WithDockerfile("Dockerfile")
        .Build();

    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage(_msSqlImage)
        .WithName("sqlserver_container_it")
        .WithPassword("P455w0rd")
        .WithEnvironment("SA_PASSWORD", "P455w0rd") // this is a custom env, should be equal to MSSQL_SA_PASSWORD
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // var sqlServerPort = _msSqlContainer.GetMappedPublicPort(1433);
        Environment.SetEnvironmentVariable(
            "ConnectionStrings:SqlServer",
            _msSqlContainer.GetConnectionString()
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
                var mySqlContext = services.SingleOrDefault(d => d.ServiceType == typeof(MySqlDatabase.DatabaseContext))!;
                if (mySqlContext != null)
                {
                    var automapperAdnEfCoreServices = services.Where(s =>
                        s.ServiceType.ToString().Contains("AutoMapper") || 
                        s.ServiceType.ToString().Contains("EntityFrameworkCore")
                    ).ToList();
                    var unitOfWorkService = services.SingleOrDefault(d => d.ServiceType == typeof(IUnitOfWork))!;
                    services.Remove(mySqlContext);
                    services.Remove(unitOfWorkService);
                    automapperAdnEfCoreServices.ForEach(s => {
                        services.Remove(s);
                    });
                    services.AddSqlServerConfiguration(configuration);
                }
            });
    }

    public async Task InitializeAsync()
    {
        await _msSqlImage.CreateAsync();
        await _msSqlContainer.StartAsync();
        await Task.Delay(5000); // TODO: Check why this is ONLY SOMETIMES necessary
    }

    public new async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
        await _msSqlImage.DeleteAsync();
        await _msSqlImage.DisposeAsync();
    }
}
