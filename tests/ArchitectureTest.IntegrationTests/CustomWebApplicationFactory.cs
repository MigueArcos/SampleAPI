using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.Infrastructure.Services;
using ArchitectureTest.Web.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ArchitectureTest.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")
            .Build();

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
}