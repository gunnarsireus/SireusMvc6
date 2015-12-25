using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Facebook;
using Microsoft.AspNet.Authentication.Google;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.AspNet.Authentication.Twitter;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using SireusMvc6.Models;
using SireusMvc6.Services;
using Microsoft.AspNet.Http;

namespace SireusMvc6
{
    public class Startup
    {
        private static readonly Random GlobalRandom = new Random();

        public static int Random100000()
        {
            return GlobalRandom.Next(100000);
        }

        public static Dictionary<string, object> Session { get; set; } = new Dictionary<string, object>();

        public static string RootPathUpload { get; set; }
        public static string DbConnection { get; set; }

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            //DbConnection = "Data Source=GUNNAR-DELL;Initial Catalog=C:db\\PERSONAL.mdf;Trusted_Connection=True";
            DbConnection="Server=tcp:e268yd7b87.database.windows.net,1433;Database=PERSONAL.mdf;User ID=gunnarsireus@e268yd7b87;Password=GS1@azure;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
            RootPathUpload = appEnv.ApplicationBasePath + @"\Upload";
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Entity Framework services to the services container.
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add MVC services to the services container.
            services.AddMvc();
            services.AddCaching();

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            // Register application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Configure the HTTP request pipeline.

            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage(DatabaseErrorPageOptions.ShowAll);
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
            }

            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

            // Add and configure the options for authentication middleware to the request pipeline.
            // You can add options for middleware as shown below.
            // For more information see http://go.microsoft.com/fwlink/?LinkID=532715
            //app.UseFacebookAuthentication(options =>
            //{
            //    options.AppId = Configuration["Authentication:Facebook:AppId"];
            //    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            //});
            //app.UseGoogleAuthentication(options =>
            //{
            //    options.ClientId = Configuration["Authentication:Google:ClientId"];
            //    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            //});
            //app.UseMicrosoftAccountAuthentication(options =>
            //{
            //    options.ClientId = Configuration["Authentication:MicrosoftAccount:ClientId"];
            //    options.ClientSecret = Configuration["Authentication:MicrosoftAccount:ClientSecret"];
            //});
            //app.UseTwitterAuthentication(options =>
            //{
            //    options.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
            //    options.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
            //});

            // Add MVC to the request pipeline.

            app.UseMvc(routes =>
            {
                //routes.MapRoute(
                //   name: "Calculateen", // Route name
                //  template: "{Controller}/{action}/{Conf}/{RLD}/{LTBMM}/{LTBDD}/{LTBYY}/{EOSMM}/{EOSDD}/{EOSYY}/{IB0}/{IB1}/{IB2}/{IB3}/{IB4}/{IB5}/{IB6}/{IB7}/{IB8}/{IB9}/{IB10}/{RS0}/{RS1}/{RS2}/{RS3}/{RS4}/{RS5}/{RS6}/{RS7}/{RS8}/{RS9}/{FR0}/{FR1}/{FR2}/{FR3}/{FR4}/{FR5}/{FR6}/{FR7}/{FR8}/{FR9}/{RL0}/{RL1}/{RL2}/{RL3}/{RL4}/{RL5}/{RL6}/{RL7}/{RL8}/{RL9}");

                //routes.MapRoute(
                //   name: "Calculatesv", // Route name
                //  template: "{Controller}/{action}/{Conf}/{RLD}/{LTB}/{EOS}/{IB0}/{IB1}/{IB2}/{IB3}/{IB4}/{IB5}/{IB6}/{IB7}/{IB8}/{IB9}/{IB10}/{RS0}/{RS1}/{RS2}/{RS3}/{RS4}/{RS5}/{RS6}/{RS7}/{RS8}/{RS9}/{FR0}/{FR1}/{FR2}/{FR3}/{FR4}/{FR5}/{FR6}/{FR7}/{FR8}/{FR9}/{RL0}/{RL1}/{RL2}/{RL3}/{RL4}/{RL5}/{RL6}/{RL7}/{RL8}/{RL9}");

                routes.MapRoute(
                    name: "Handler", // Route name
                    template: "{Controller}/{action}/{arg1}/{arg2}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{data?}");

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });
        }
    }
}
