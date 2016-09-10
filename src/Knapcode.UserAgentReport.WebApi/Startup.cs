using System.Collections.Generic;
using System.IO;
using Knapcode.UserAgentReport.AccessLogs;
using Knapcode.UserAgentReport.Reporting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "RefreshPeriod", "06:00:00" },
                    { "DatabasePath", Path.Combine(env.ContentRootPath, "user-agents.sqlite3") },
                    { "StatusPath", Path.Combine(env.ContentRootPath, "user-agent-database-status.json") }
                })
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<UserAgentDatabaseUpdaterSettings>(Configuration);
            services.Configure<WebsiteSettings>(Configuration);

            services.AddSingleton<UserAgentDatabaseUpdater>();
            services.AddTransient<IAccessLogParser, CustomAccessLogParser>();
            services.AddSingleton(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<UserAgentDatabaseUpdaterSettings>>();
                var parser = serviceProvider.GetRequiredService<IAccessLogParser>();

                return new UserAgentDatabase(settings, TextWriter.Null, parser);
            });
            
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
