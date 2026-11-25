using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TeamGym.Arena.Common.Persistence;

/// <summary>
/// Applies the EF Core migrations of every registered <see cref="ModuleDatabase"/>, one module after the other
/// (ADR-0007). Only the migration runner and the integration tests call it. The API never migrates the schema.
/// </summary>
public static partial class ModuleDatabaseMigrator
{
    public static async Task MigrateAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logger);

        foreach (var database in services.GetServices<ModuleDatabase>())
        {
            var scope = services.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                var context = (DbContext)scope.ServiceProvider.GetRequiredService(database.ContextType);

                LogMigrating(logger, database.Schema);
                try
                {
                    await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    LogFailed(logger, exception, database.Schema);
                    throw;
                }

                LogMigrated(logger, database.Schema);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying the migrations of the {Schema} schema")]
    private static partial void LogMigrating(ILogger logger, string schema);

    [LoggerMessage(Level = LogLevel.Information, Message = "The {Schema} schema is up to date")]
    private static partial void LogMigrated(ILogger logger, string schema);

    [LoggerMessage(Level = LogLevel.Error, Message = "The migrations of the {Schema} schema failed")]
    private static partial void LogFailed(ILogger logger, Exception exception, string schema);
}
