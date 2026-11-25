using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Respawn;
using Respawn.Graph;
using TeamGym.Arena.Api.IntegrationTests.Infrastructure;
using TeamGym.Arena.Common.Persistence;
using Testcontainers.PostgreSql;
using Xunit.Sdk;
using Xunit.v3;

// One PostgreSQL container for the whole assembly. The tests share it, so they run one at a time, and each test
// that writes data starts from an empty database with ResetAsync().
[assembly: AssemblyFixture(typeof(PostgresDatabase))]
[assembly: Parallelization(Mode = ParallelMode.None)]

namespace TeamGym.Arena.Api.IntegrationTests.Infrastructure;

/// <summary>
/// A real PostgreSQL 17 database in a container (ADR-0007). It starts empty and gets every module's migrations
/// applied, exactly as the migration runner applies them, so every test run checks that the migrations apply cleanly
/// from an empty database.
/// </summary>
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "xUnit injects this fixture into public test classes' constructors, so it must be public too.")]
public sealed class PostgresDatabase : IAsyncLifetime
{
    /// <summary>The image that local development and production use too (ADR-0007).</summary>
    public const string Image = "postgres:17-alpine";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(Image).Build();

    private Respawner? _respawner;

    public string ConnectionString => _container.GetConnectionString();

    /// <summary>The module databases that the API registers.</summary>
    public IReadOnlyList<ModuleDatabase> ModuleDatabases { get; private set; } = [];

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await _container.StartAsync(cancellationToken);

        // The API registers every module's DbContext, so migrating through its services covers every module.
        await using (var api = new ApiFactory(this))
        {
            ModuleDatabases = [.. api.Services.GetServices<ModuleDatabase>()];
            await ModuleDatabaseMigrator.MigrateAsync(api.Services, NullLogger.Instance, cancellationToken);
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [.. ModuleDatabases.Select(database => database.Schema)],
            TablesToIgnore = [new Table(ModuleDatabaseExtensions.MigrationsHistoryTable)],
        });
    }

    /// <summary>Deletes every row from every module's tables, keeping the schema and the migration history.</summary>
    public async Task ResetAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException("The database has not been initialised.");
        }

        await using var connection = await OpenConnectionAsync(TestContext.Current.CancellationToken);
        await _respawner.ResetAsync(connection);
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
