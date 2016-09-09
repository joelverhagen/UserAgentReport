using System;
using System.IO;
using Knapcode.UserAgentReport.AccessLogs;
using Knapcode.UserAgentReport.Reporting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Knapcode.UserAgentReport.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAccessLogParser, CustomAccessLogParser>();

            services.AddTransient(serviceProvider =>
            {
                var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

                return new UserAgentDatabaseUpdaterSettings
                {
                    RefreshPeriod = TimeSpan.FromHours(6),
                    DatabasePath = Path.Combine(env.ContentRootPath, "user-agents.sqlite3"),
                    StatusPath = Path.Combine(env.ContentRootPath, "user-agent-database-status.json"),
                    DatabaseUri = new Uri("http://prancer.knapcode.com/data/user-agents.sqlite3")
                };
            });

            services.AddSingleton(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<UserAgentDatabaseUpdaterSettings>();
                var parser = serviceProvider.GetRequiredService<IAccessLogParser>();

                return new UserAgentDatabase(settings.DatabasePath, TextWriter.Null, parser);
            });
            
            services.AddSingleton<UserAgentDatabaseUpdater>();

            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
