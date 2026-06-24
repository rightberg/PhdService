using Serilog;
using Serilog.Events;
using System;
using System.Configuration;
using System.Web.Http;

namespace PHDTagService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // log configuration
            var configValue = ConfigurationManager.AppSettings["logLevel"] ?? "Fatal";
            var logConfig = new LoggerConfiguration().MinimumLevel.Verbose();

            LogEventLevel GetLevel(string key, LogEventLevel defaultLevel = LogEventLevel.Information)
            {
                var val = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrEmpty(val)) return defaultLevel;
                if (val.Equals("INFO", StringComparison.OrdinalIgnoreCase)) val = "Information";
                return Enum.TryParse(val, true, out LogEventLevel result) ? result : defaultLevel;
            }

            logConfig.WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(Serilog.Filters.Matching.WithProperty("api"))
                .WriteTo.File(
                    path: AppDomain.CurrentDomain.BaseDirectory + "Logs\\api-.txt",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: GetLevel("logLevelApi"),
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                ));

            logConfig.WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(Serilog.Filters.Matching.WithProperty("phd-api"))
                .WriteTo.File(
                    path: AppDomain.CurrentDomain.BaseDirectory + "Logs\\phd-api-.txt",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: GetLevel("logLevelPHDApi"),
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                ));

            Log.Logger = logConfig.CreateLogger();

            // start info
            Log.Information("Service started");
        }

        protected void Application_End()
        {
            Log.Information("Service shutdown");
            Log.CloseAndFlush();
        }
    }
}
