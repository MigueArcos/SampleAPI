using System.Diagnostics;
using System.Text;
using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.ServiceLayer.AuthService;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;
using ArchitectureTest.Domain.ServiceLayer.JwtManager;
using ArchitectureTest.Domain.ServiceLayer.PasswordHasher;
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
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("ConfigData:Jwt:Secret")!)),
    ClockSkew = Debugger.IsAttached ? TimeSpan.Zero : TimeSpan.FromMinutes(10)
};

builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(configuration.GetConnectionString("SQLServer")));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IJwtManager, JwtManager>(s => new JwtManager(tokenValidationParameters));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICrudService<Note, NoteDTO>, NotesCrudService>();
builder.Services.AddScoped<ICrudService<Checklist, ChecklistDTO>, ChecklistCrudService>();
builder.Services.AddScoped<CustomJwtBearerEvents>();
builder.Services.AddAuthentication().AddJwtBearer(options => {
    options.TokenValidationParameters = tokenValidationParameters;
    options.EventsType = typeof(CustomJwtBearerEvents);
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
