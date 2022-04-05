using System;

using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlueprintRepository {
    public class Program {
        public static void Main(string[] arguments) {
            Host.CreateDefaultBuilder(arguments).ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
                webBuilder.ConfigureLogging(loggingBuilder => {
                    loggingBuilder.ClearProviders();
                });
            })

            .Build().Run();
        }
    }

    public static class Extensions {
        private static readonly DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTime(this DateTime date) {
            return (long)(date - epochTime).TotalMilliseconds;
        }
    }

    public static class HelperFunctions {
        public static string GetConfigurationWFallback(this IConfiguration configuration, string name) {
            return configuration.GetValue<string>(name) ?? Environment.GetEnvironmentVariable(name);
        }
    }
}
