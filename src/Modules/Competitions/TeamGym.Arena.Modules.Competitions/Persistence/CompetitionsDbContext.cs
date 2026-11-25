using Microsoft.EntityFrameworkCore;
using TeamGym.Arena.Common.Persistence;

namespace TeamGym.Arena.Modules.Competitions.Persistence;

/// <summary>The database context of the Competitions module. Its tables live in the <c>competitions</c> schema (ADR-0007).</summary>
internal sealed class CompetitionsDbContext(DbContextOptions<CompetitionsDbContext> options) : ModuleDbContext(options, Schema)
{
    public const string Schema = "competitions";
}
