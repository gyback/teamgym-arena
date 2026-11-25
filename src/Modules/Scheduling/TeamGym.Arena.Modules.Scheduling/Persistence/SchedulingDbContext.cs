using Microsoft.EntityFrameworkCore;
using TeamGym.Arena.Common.Persistence;

namespace TeamGym.Arena.Modules.Scheduling.Persistence;

/// <summary>The database context of the Scheduling module. Its tables live in the <c>scheduling</c> schema (ADR-0007).</summary>
internal sealed class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : ModuleDbContext(options, Schema)
{
    public const string Schema = "scheduling";
}
