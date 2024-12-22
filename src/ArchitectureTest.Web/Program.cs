using System.Diagnostics;
using System.Text;
using ArchitectureTest.Databases.SqlServer;
using ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services;
using ArchitectureTest.Domain.Services.Application.AuthService;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl;
using ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl.Contracts;
using ArchitectureTest.Domain.Services.Infrastructure;
using ArchitectureTest.Domain.Services.Infrastructure.JwtManager;
using ArchitectureTest.Domain.Services.Infrastructure.PasswordHasher;
using ArchitectureTest.Infrastructure.SqlEFCore;
using ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;
using ArchitectureTest.Infrastructure.SqlEFCore.UnitOfWork;
using ArchitectureTest.Web;
using ArchitectureTest.Web.Authentication;
using AutoMapper;
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
var connectionString = configuration.GetConnectionString("SqlServer");
// SqlServer
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));

// Mysql
// builder.Services.AddDbContext<DatabaseContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IJwtManager, JwtManager>(s => new JwtManager(tokenValidationParameters, configuration));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDomainUnitOfWork, SqlSeverUnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
// builder.Services.AddScoped<IDomainRepository<NoteEntity>, SqlRepository<NoteEntity, Note>>();
builder.Services.AddScoped<ICrudService<NoteEntity>, NotesCrudService>();
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



public class MappingProfile : Profile
{
    private IList<ChecklistDetailEntity>? GetChecklistDetails(ICollection<ChecklistDetailEntity>? details, long? parentDetailId = null){
        var selection = details?.Where(d => d.ParentDetailId == parentDetailId).Select(cD => new ChecklistDetailEntity {
            Id = cD.Id,
            ChecklistId = cD.ChecklistId,
            ParentDetailId = cD.ParentDetailId,
            TaskName = cD.TaskName,
            Status = cD.Status,
            CreationDate = cD.CreationDate,
            ModificationDate = cD.ModificationDate
        }).ToList();
        selection?.ForEach(i => {
            i.SubItems = GetChecklistDetails(details, i.Id);
        });
        return selection;
    }
    public MappingProfile()
    {
        CreateMap<NoteEntity, ArchitectureTest.Databases.SqlServer.Entities.Note>().ReverseMap();
        CreateMap<NoteEntity, ArchitectureTest.Databases.MySql.Entities.Note>().ReverseMap();

        CreateMap<ArchitectureTest.Databases.SqlServer.Entities.Checklist, ChecklistEntity>()
            .ForMember(e => e.Details, o => o.MapFrom(src => src.ChecklistDetails))
            .AfterMap((src, dest, context) => {
                dest.Details = GetChecklistDetails(dest.Details);
            });
        CreateMap<ChecklistEntity, ArchitectureTest.Databases.SqlServer.Entities.Checklist>();

        CreateMap<ArchitectureTest.Databases.MySql.Entities.Checklist, ChecklistEntity>()
            .ForMember(e => e.Details, o => o.MapFrom(src => src.ChecklistDetails))
            .AfterMap((src, dest, context) => {
                dest.Details = GetChecklistDetails(dest.Details);
            });
        CreateMap<ChecklistEntity, ArchitectureTest.Databases.MySql.Entities.Checklist>();

        CreateMap<ChecklistDetailEntity, ArchitectureTest.Databases.SqlServer.Entities.ChecklistDetail>().ReverseMap();
        CreateMap<ChecklistDetailEntity, ArchitectureTest.Databases.MySql.Entities.ChecklistDetail>().ReverseMap();
   }
}

