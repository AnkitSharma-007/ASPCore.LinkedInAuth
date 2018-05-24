using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LinkdinAuth.Data;
using LinkdinAuth.Models;
using LinkdinAuth.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OAuth;
  
namespace LinkdinAuth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
  
        public IConfiguration Configuration { get; }
  
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
  
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
  
            services.AddAuthentication().AddLinkedIn(options =>
            {
                options.ClientId = Configuration["Authentication:LinkedIn:ClientId"];
                options.ClientSecret = Configuration["Authentication:LinkedIn:ClientSecret"];
  
                options.Events= new OAuthEvents()
                {
                    OnRemoteFailure = loginFailureHandler =>
                    {
                        var authProperties = options.StateDataFormat.Unprotect(loginFailureHandler.Request.Query["state"]);
                        loginFailureHandler.Response.Redirect("/Account/login");
                        loginFailureHandler.HandleResponse();
                        return Task.FromResult(0);
                    }
                };
  
            }); 
  
            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
  
            services.AddMvc();
        }
  
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
              
            app.UseStaticFiles();
  
            app.UseAuthentication();
  
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
