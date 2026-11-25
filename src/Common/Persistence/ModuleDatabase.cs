namespace TeamGym.Arena.Common.Persistence;

/// <summary>
/// A module's database: its <c>DbContext</c> type and its PostgreSQL schema. Every
/// <see cref="ModuleDatabaseExtensions.AddModuleDbContext{TContext}"/> call registers one as a singleton, so that the
/// migration runner and the tests can find every module's database without naming types that are internal to a
/// module.
/// </summary>
public sealed record ModuleDatabase(Type ContextType, string Schema);
