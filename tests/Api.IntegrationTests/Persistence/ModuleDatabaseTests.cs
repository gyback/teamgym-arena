using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamGym.Arena.Api.IntegrationTests.Infrastructure;
using TeamGym.Arena.Common.Persistence;

namespace TeamGym.Arena.Api.IntegrationTests.Persistence;

/// <summary>
/// The per-module database rules of ADR-0007, checked against the database that <see cref="PostgresDatabase"/>
/// migrated from empty.
/// </summary>
public sealed class ModuleDatabaseTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly string[] s_moduleSchemas = ["applications", "competitions", "organisations", "scheduling", "users"];

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public static TheoryData<string> Schemas => [.. s_moduleSchemas];

    [Fact]
    public void EveryModuleHasItsOwnDbContextAndSchema()
    {
        var databases = factory.Services.GetServices<ModuleDatabase>().ToList();

        Assert.Equal(s_moduleSchemas, databases.Select(database => database.Schema).Order(StringComparer.Ordinal));
        Assert.Distinct(databases.Select(database => database.ContextType));
    }

    [Theory]
    [MemberData(nameof(Schemas))]
    public async Task MigrationsAreAppliedAndMatchTheModel(string schema)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        using var context = ResolveContext(scope.ServiceProvider, schema);

        Assert.Empty(await context.Database.GetPendingMigrationsAsync(CancellationToken));
        Assert.False(
            context.Database.HasPendingModelChanges(),
            $"The model of the {schema} schema has changes without a migration. Run 'dotnet ef migrations add'.");
    }

    [Theory]
    [MemberData(nameof(Schemas))]
    public void ModelPutsEveryTableInTheModuleSchema(string schema)
    {
        using var scope = factory.Services.CreateScope();
        using var context = ResolveContext(scope.ServiceProvider, schema);

        Assert.All(context.Model.GetEntityTypes(), entityType => Assert.Equal(schema, entityType.GetSchema()));
    }

    [Fact]
    public async Task EveryMigratedModuleKeepsItsMigrationHistoryInItsOwnSchema()
    {
        var historyTableSchemas = await QueryStringsAsync(
            $"SELECT table_schema AS value FROM information_schema.tables WHERE table_name = {ModuleDatabaseExtensions.MigrationsHistoryTable}");

        Assert.Contains("organisations", historyTableSchemas);
        Assert.All(historyTableSchemas, schema => Assert.Contains(schema, s_moduleSchemas));
    }

    [Fact]
    public async Task EveryTableIsInAModuleSchema()
    {
        var schemas = await QueryStringsAsync(
            $"SELECT DISTINCT table_schema AS value FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog', 'information_schema')");

        Assert.NotEmpty(schemas);
        Assert.All(schemas, schema => Assert.Contains(schema, s_moduleSchemas));
    }

    [Fact]
    public async Task NoForeignKeyCrossesASchemaBoundary()
    {
        var crossSchemaForeignKeys = await QueryStringsAsync(
            $"""
            SELECT constraint_record.conname::text AS value
            FROM pg_constraint AS constraint_record
            JOIN pg_class AS dependent ON dependent.oid = constraint_record.conrelid
            JOIN pg_class AS principal ON principal.oid = constraint_record.confrelid
            WHERE constraint_record.contype = 'f' AND dependent.relnamespace <> principal.relnamespace
            """);

        Assert.Empty(crossSchemaForeignKeys);
    }

    private static DbContext ResolveContext(IServiceProvider services, string schema)
    {
        var database = Assert.Single(services.GetServices<ModuleDatabase>(), database => database.Schema == schema);
        return (DbContext)services.GetRequiredService(database.ContextType);
    }

    /// <summary>Runs a query with one text column named <c>value</c> through any module's context.</summary>
    private async Task<List<string>> QueryStringsAsync(FormattableString sql)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        using var context = ResolveContext(scope.ServiceProvider, "organisations");

        return await context.Database.SqlQuery<string>(sql).ToListAsync(CancellationToken);
    }
}
