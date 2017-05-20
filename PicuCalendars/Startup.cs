using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PicuCalendars.DataAccess;
using System.Security.Principal;
using PicuCalendars.Security;
using System.Security.Claims;

namespace PicuCalendars
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            //string connString = Configuration.GetConnectionString("DefaultConnection");
            services.AddScoped(_ => new CalendarContext(Configuration["ConnectionStrings:DefaultConnection"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //Custom authentication middleware.
            app.Use(async (context, next) =>
            {
                var cc = app.ApplicationServices.GetService<CalendarContext>();
                var rosterAccess = await MyValidationToken.RosterAccess(context, cc);
                if (rosterAccess != null)
                {
                    //use a GenericPrincipal if we want to load roles
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(new GenericIdentity("DefaultUser","Cookies"), new[] { new Claim("RosterAccess", rosterAccess.Value.ToString()) }));
                }

                //Replace the parameter with the username from the request.
                await next();
            });

            app.UseMvc();
        }
    }
}
