using Microsoft.EntityFrameworkCore;

namespace TeamGym.Arena.Common.Persistence;

/// <summary>
/// The base class of every module's <c>DbContext</c> (ADR-0007). It puts the module's tables in the module's own
/// PostgreSQL schema and stores every strongly typed ID as a <c>uuid</c>. Register the context with
/// <see cref="ModuleDatabaseExtensions.AddModuleDbContext{TContext}"/>, which adds the rest of the conventions.
/// </summary>
/// <remarks>
/// Apply the module's <c>IEntityTypeConfiguration&lt;T&gt;</c> classes explicitly in an override of
/// <see cref="OnModelCreating"/> that first calls the base implementation.
/// </remarks>
public abstract class ModuleDbContext : DbContext
{
    private readonly string _schema;

    /// <param name="options">The options that <see cref="ModuleDatabaseExtensions.AddModuleDbContext{TContext}"/> built.</param>
    /// <param name="schema">The module's PostgreSQL schema, such as <c>organisations</c>.</param>
    protected ModuleDbContext(DbContextOptions options, string schema)
        : base(options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        _schema = schema;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(_schema);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        foreach (var idType in StronglyTypedIdConventions.FindIdTypes(GetType().Assembly))
        {
            configurationBuilder.Properties(idType).HaveConversion(StronglyTypedIdConventions.ConverterTypeFor(idType));
        }
    }
}
