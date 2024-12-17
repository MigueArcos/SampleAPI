using System.Diagnostics;
using System.Text;
using ArchitectureTest.Data.Database.SQLServer;
using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Domain.Services.Application.AuthService;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.Domain.Services.Infrastructure.JwtManager;
using ArchitectureTest.Domain.Services.Infrastructure.PasswordHasher;
using ArchitectureTest.Infrastructure.SqlEFCore.UnitOfWork;
using ArchitectureTest.Web;
using ArchitectureTest.Web.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;
TokenValidationParameters tokenValidationParameters = new()
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = configuration.GetValue<string>("ConfigData:Jwt:Issuer")!,
    ValidAudience = configuration.GetValue<string>("ConfigData:Jwt:Audience"),
    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(configuration.GetValue<string>("ConfigData:Jwt:Secret")!)
    ),
    ClockSkew = Debugger.IsAttached ? TimeSpan.Zero : TimeSpan.FromMinutes(10)
};

builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(configuration.GetConnectionString("SQLServer")));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IJwtManager, JwtManager>(s => new JwtManager(tokenValidationParameters, configuration));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICrudService<Note, NoteDTO>, NotesCrudService>();
builder.Services.AddScoped<IChecklistCrudService, ChecklistCrudService>();
builder.Services.AddScoped<CustomJwtBearerEvents>();
builder.Services.AddAuthentication().AddJwtBearer(options => {
    options.TokenValidationParameters = tokenValidationParameters;
    options.EventsType = typeof(CustomJwtBearerEvents);
});
builder.Services.AddExceptionHandler<GlobalHttpExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
