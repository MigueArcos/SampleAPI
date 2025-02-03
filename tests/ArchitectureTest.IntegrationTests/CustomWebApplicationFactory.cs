using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.Infrastructure.Services;
using ArchitectureTest.Web.Authentication;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.MySql;
using Xunit;

namespace ArchitectureTest.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly IFutureDockerImage _msSqlImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "sql_server_setup")
        .WithDockerfile("Dockerfile")
        .Build();

    private readonly MySqlContainer _mySqlContainer = new MySqlBuilder()
        .WithImage("mysql:latest")
        .WithBindMount($"{CommonDirectoryPath.GetSolutionDirectory().DirectoryPath}/mysql/", "/docker-entrypoint-initdb.d/")
        .WithName("mysql_container")
        .WithDatabase("crud")
        .WithUsername("self")
        .WithPassword("P455w0rd")
        .Build();

    private readonly IContainer _msSqlContainer = new ContainerBuilder()
        .WithImage(_msSqlImage)
        .WithName("sqlserver_container")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithEnvironment("MSSQL_SA_PASSWORD", "P455w0rd")
        .WithEnvironment("SA_PASSWORD", "P455w0rd")
        .WithEnvironment("SSQL_PID", "Express")
        .WithPortBinding(1434, 1433)
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")
            .Build();
        Environment.SetEnvironmentVariable(
            "ConnectionStrings:MySql",
            _mySqlContainer.GetConnectionString()
        );
        Environment.SetEnvironmentVariable(
            "ConnectionStrings:SqlServer",
            "Server=localhost,1434;Database=master;User Id=sa;Password=P455w0rd;TrustServerCertificate=True"
        );

        builder
            .ConfigureAppConfiguration(builder => {
                builder.Sources.Clear();
                builder.AddConfiguration(configuration);
            })
            .ConfigureServices(services => {
                TokenValidationParameters tokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // IMPORTANT: do not validate token lifetime
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.GetValue<string>("ConfigData:Jwt:Issuer"),
                    ValidAudience = configuration.GetValue<string>("ConfigData:Jwt:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration.GetValue<string>("ConfigData:Jwt:Secret")!)
                    ),
                    ClockSkew = Debugger.IsAttached ? TimeSpan.Zero : TimeSpan.FromMinutes(10)
                };
                var jwtManagerService = services.SingleOrDefault(d => d.ServiceType == typeof(IJwtManager))!;
                // var bearerEventsService = services.SingleOrDefault(d => d.ServiceType == typeof(CustomJwtBearerEvents))!;

                services.Remove(jwtManagerService);
                // services.Remove(bearerEventsService);

                var authServices = services.Where(s => 
                    s.ServiceType.ToString().Contains("Microsoft.AspNetCore.Authentication") // remove all authentication stuff
                ).ToList();

                authServices.ForEach(s => {
                    services.Remove(s);
                });

                services.AddAuthentication().AddJwtBearer(options => {
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.EventsType = typeof(CustomJwtBearerEvents);
                });
                services.AddScoped<IJwtManager, JwtManager>(s => new JwtManager(tokenValidationParameters, configuration));
                // services.AddScoped<CustomJwtBearerEvents>(); // Why this needs to be registered again
            });
    }

    public async Task InitializeAsync()
    {
        await _msSqlImage.CreateAsync();
        await _msSqlContainer.StartAsync();
        await _mySqlContainer.StartAsync();
        // HttpClient = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        await _mySqlContainer.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
        await _msSqlImage.DeleteAsync();
    }
}
