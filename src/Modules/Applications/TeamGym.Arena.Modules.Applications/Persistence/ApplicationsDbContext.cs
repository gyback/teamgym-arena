using Microsoft.EntityFrameworkCore;
using TeamGym.Arena.Common.Persistence;

namespace TeamGym.Arena.Modules.Applications.Persistence;

/// <summary>The database context of the Applications module. Its tables live in the <c>applications</c> schema (ADR-0007).</summary>
internal sealed class ApplicationsDbContext(DbContextOptions<ApplicationsDbContext> options) : ModuleDbContext(options, Schema)
{
    public const string Schema = "applications";
}
