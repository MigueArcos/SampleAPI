using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Domain.UnitOfWork;
using ArchitectureTest.Infrastructure.AppConfiguration;
using ArchitectureTest.Infrastructure.Jwt;
using ArchitectureTest.Web.ActionFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace ArchitectureTest.Web {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			services.AddDbContext<DatabaseContext>(options => options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			ConfigData config = new ConfigData();
			Configuration.GetSection("ConfigData").Bind(config);
			services.AddSingleton(config);
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddScoped<IJwtManager, JwtManager>();
			services.AddScoped<CustomJwtBearerEvents>();
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
				options.TokenValidationParameters = new TokenValidationParameters {
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = config.Jwt.Issuer,
					ValidAudience = config.Jwt.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Jwt.Secret))
				};
				options.EventsType = typeof(CustomJwtBearerEvents);
			});
			services.AddMvc().AddJsonOptions(o => {
				o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
				o.SerializerSettings.ContractResolver = new DefaultContractResolver();
			}).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}
			else {
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseMvc(routes => {
				routes.MapRoute("Home", "", new { controller = "Home", action = "Index" });
				/*routes.MapRoute("TestPost", "notespost", new { controller = "Notes", action = "Post" });
				routes.MapRoute("TestGet", "notesget/{id}", new { controller = "Notes", action = "GetById" });
				routes.MapRoute("TestGetAll", "notesgetall/{userId}", new { controller = "Notes", action = "GetAll" });*/
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
