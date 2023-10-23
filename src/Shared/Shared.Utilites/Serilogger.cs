using Microsoft.Extensions.Hosting;

using Serilog.Debugging;

namespace Serilog;

public static class Serilogger
{
    public static Action<HostBuilderContext, LoggerConfiguration> Configure 
        => (context, configuration)
        =>
        {
#if DEBUG
            SelfLog.Enable(Console.Error);
#endif
            configuration
                         .Enrich.FromLogContext()
                         .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                         .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                         .ReadFrom.Configuration(context.Configuration);
        };
}
