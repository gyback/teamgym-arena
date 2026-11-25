using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace TeamGym.Arena.Common.Persistence;

/// <summary>Registers a module's <c>DbContext</c> with the shared conventions of ADR-0007.</summary>
public static class ModuleDatabaseExtensions
{
    /// <summary>
    /// The name of the connection string that every module uses, because all modules share one PostgreSQL database
    /// (ADR-0007). Operators set it with the <c>ConnectionStrings__teamgym</c> environment variable (ADR-0009).
    /// </summary>
    public const string ConnectionStringName = "teamgym";

    /// <summary>The name of the migration history table, which every module keeps in its own schema.</summary>
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    /// <summary>
    /// How long a module's database health check may take before <c>/health</c> reports it unhealthy. The check
    /// connects through the retrying execution strategy, which would otherwise keep a probe waiting for a minute
    /// while the database is down.
    /// </summary>
    public static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Registers <typeparamref name="TContext"/> for PostgreSQL in <paramref name="schema"/>, with snake_case names,
    /// the migration history table in the module's schema, domain event dispatch, and the Aspire health check,
    /// retries and telemetry for Npgsql.
    /// </summary>
    /// <remarks>
    /// The connection string is read when a context is first created, so that hosts that never open a connection,
    /// such as the build-time OpenAPI export and <c>dotnet ef</c>, run without one.
    /// </remarks>
    public static IHostApplicationBuilder AddModuleDbContext<TContext>(this IHostApplicationBuilder builder, string schema)
        where TContext : ModuleDbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        builder.Services.TryAddScoped<DomainEventDispatchInterceptor>();

        builder.Services.AddDbContext<TContext>((services, options) =>
        {
            var connectionString = services.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString) && !EF.IsDesignTime)
            {
                throw new InvalidOperationException(
                    $"The connection string '{ConnectionStringName}' is not set. Set the ConnectionStrings__{ConnectionStringName} environment variable.");
            }

            options
                .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable(MigrationsHistoryTable, schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(services.GetRequiredService<DomainEventDispatchInterceptor>());
        });

        builder.Services.AddSingleton(new ModuleDatabase(typeof(TContext), schema));

        // Adds the health check (named after the context type), retries on transient failures, and Npgsql telemetry.
        builder.EnrichNpgsqlDbContext<TContext>();

        builder.Services.PostConfigure<HealthCheckServiceOptions>(options =>
        {
            foreach (var registration in options.Registrations.Where(registration => registration.Name == typeof(TContext).Name))
            {
                registration.Timeout = HealthCheckTimeout;
            }
        });

        return builder;
    }
}
