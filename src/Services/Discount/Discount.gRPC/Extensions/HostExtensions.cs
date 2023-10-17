using Npgsql;
using Polly;
using Polly.Retry;

namespace Discount.gRPC.Extensions;

public static partial class HostExtensions
{
    public static IHost MigrateDatabase<TContext>(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                LogStartMigration(logger);

                var pipeline = new ResiliencePipelineBuilder()
                    .AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        BackoffType = DelayBackoffType.Exponential,
                        ShouldHandle = new PredicateBuilder().Handle<NpgsqlException>(),
                        OnRetry = args =>
                        {
                            LogResiliencePipelineOnRetryError(logger, args.Outcome.Exception!, args.AttemptNumber);
                            return ValueTask.CompletedTask;
                        }
                    }).Build();

                //if the postgresql server container is not created on run docker compose this
                //migration can't fail for network related exception. The retry options for database operations
                //apply to transient exceptions                    
                pipeline.Execute(() => ExecuteMigrations(configuration));

                LogFinishMigration(logger);
            }
            catch (NpgsqlException ex)
            {
                LogMigrationError(logger, ex);
            }
        }

        return host;
    }

    private static void ExecuteMigrations(IConfiguration configuration)
    {
        using var connection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
        connection.Open();

        using var command = new NpgsqlCommand
        {
            Connection = connection
        };

        command.CommandText = "DROP TABLE IF EXISTS Coupon";
        command.ExecuteNonQuery();

        command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY, 
                                                                ProductName VARCHAR(24) NOT NULL,
                                                                Description TEXT,
                                                                Amount INT)";
        command.ExecuteNonQuery();


        command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150);";
        command.ExecuteNonQuery();

        command.CommandText = "INSERT INTO Coupon(ProductName, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100);";
        command.ExecuteNonQuery();
    }

    [LoggerMessage(Message = "Migrating PostgreSQL Database...", Level = LogLevel.Information, EventId = 0)]
    public static partial void LogStartMigration(ILogger logger);
    
    [LoggerMessage(Message = "Migrated PostgreSQL Database...", Level = LogLevel.Information, EventId = 1)]
    public static partial void LogFinishMigration(ILogger logger);

    [LoggerMessage(Message = "An error occurred while migrating the postresql database", Level = LogLevel.Error, EventId = 2)]
    public static partial void LogMigrationError(ILogger logger, Exception ex);

    [LoggerMessage(Message = "Retry {attemptNumber}, due to: ", Level = LogLevel.Error, EventId = 3)]
    public static partial void LogResiliencePipelineOnRetryError(ILogger logger, Exception ex, int attemptNumber);
}