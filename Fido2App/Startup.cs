using System;
using Business;
using Business.Service.Identity;
using Core.Settings;
using Domain.Entity.Concrete;
using Infrastructure;
using Infrastructure.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fido2App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
            
            #region AppSettings Configuration
            
            services.Configure<AppSettings>(Configuration);
            var appSettings = Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            services.AddSingleton(appSettings);
            
            #endregion

            services.AddDbContext<AppDbContext>();

            #region Identity

            services
                .AddIdentity<AppUser, AppRole>(opt => opt.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<AppDbContext>()
                .AddUserManager<CustomUserManager<AppUser>>()
                .AddSignInManager<CustomSignInManager<AppUser>>()
                .AddDefaultTokenProviders();

            #endregion

            #region Fido2Net

            services.AddFido2(options =>
                {
                    options.ServerDomain = appSettings.FidoSetting.ServerDomain;
                    options.ServerName = appSettings.FidoSetting.ServerName;
                    options.Origin = appSettings.FidoSetting.Origin;
                    options.TimestampDriftTolerance = appSettings.FidoSetting.TimestampDriftTolerance;
                    options.MDSAccessKey = appSettings.FidoSetting.MDSAccessKey;
                    options.MDSCacheDirPath = null;
                })
                .AddCachedMetadataService(config =>
                {
                    //They'll be used in a "first match wins" way in the order registered

                    if (!string.IsNullOrWhiteSpace(appSettings.FidoSetting.MDSAccessKey))
                    {
                        config.AddFidoMetadataRepository(appSettings.FidoSetting.MDSAccessKey);
                    }

                    config.AddStaticMetadataRepository();
                });
            

            #endregion
            
            services.AddInfrastructureService();
            services.AddBusinessService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}