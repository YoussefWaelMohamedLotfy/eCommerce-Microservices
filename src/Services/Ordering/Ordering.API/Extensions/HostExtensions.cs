using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace Discount.gRPC.Extensions;

public static partial class HostExtensions
{
    public static IHost MigrateDatabase<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder = null!) where TContext : DbContext
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetService<TContext>();

            try
            {
                LogStartMigration(logger);

                var pipeline = new ResiliencePipelineBuilder()
                    .AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        BackoffType = DelayBackoffType.Exponential,
                        ShouldHandle = new PredicateBuilder().Handle<SqlException>(),
                        OnRetry = args =>
                        {
                            LogResiliencePipelineOnRetryError(logger, args.Outcome.Exception!, args.AttemptNumber);
                            return ValueTask.CompletedTask;
                        }
                    }).Build();

                pipeline.Execute(() => InvokeSeeder(seeder, context!, services));

                LogFinishMigration(logger);
            }
            catch (SqlException ex)
            {
                LogMigrationError(logger, ex);
            }
        }

        return host;
    }

    private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context,
                                                IServiceProvider services) where TContext : DbContext
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            seeder(context, services);
        }

    }

    [LoggerMessage(Message = "Migrating SQL Server Database...", Level = LogLevel.Information, EventId = 0)]
    public static partial void LogStartMigration(ILogger logger);

    [LoggerMessage(Message = "Migrated SQL Server Database...", Level = LogLevel.Information, EventId = 1)]
    public static partial void LogFinishMigration(ILogger logger);

    [LoggerMessage(Message = "An error occurred while migrating the SQL Server database", Level = LogLevel.Error, EventId = 2)]
    public static partial void LogMigrationError(ILogger logger, Exception ex);

    [LoggerMessage(Message = "Retry {attemptNumber}, due to: ", Level = LogLevel.Error, EventId = 3)]
    public static partial void LogResiliencePipelineOnRetryError(ILogger logger, Exception ex, int attemptNumber);
}