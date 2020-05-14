using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Data.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
