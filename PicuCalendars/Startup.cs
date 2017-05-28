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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CreateAtRoute",
                    policy => policy.Requirements.Add(new AccessRoute(ValidationUtilities.RequestClaimBase.AccessLevel.CreateResource, "rosterId")));
                options.AddPolicy("UpdateAtRoute",
                    policy => policy.Requirements.Add(new AccessRoute(ValidationUtilities.RequestClaimBase.AccessLevel.UpdateResource, "rosterId")));
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationHandler, AccessUrlHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Custom authentication middleware.
            app.Use(async (context, next) =>
            {
                var cc = app.ApplicationServices.GetService<CalendarContext>();
                var rosterAccess = await MyValidationToken.RosterAccess(context, cc);
                if (rosterAccess != null)
                {
                    //use a GenericPrincipal if we want to load roles
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(new GenericIdentity("DefaultUser","Cookies"), new[] { new Claim(rosterAccess.Access.ToString(), rosterAccess.ResourceId.ToString()) }));
                }

                //Replace the parameter with the username from the request.
                await next();
            });

            app.UseMvc();
        }
    }
}
