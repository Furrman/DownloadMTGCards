using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog;
using NLog.Extensions.Logging;

namespace ConsoleApp.Configuration;

internal static class NLogConfigurator
{
    public static ILoggerFactory SetupNLog()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageProperties = true,
                    CaptureMessageTemplates = true
                });
            })
            .BuildServiceProvider();
        LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return loggerFactory;
    }
}
