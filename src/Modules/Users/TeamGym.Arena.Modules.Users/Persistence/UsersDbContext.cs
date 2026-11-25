using Microsoft.EntityFrameworkCore;
using TeamGym.Arena.Common.Persistence;

namespace TeamGym.Arena.Modules.Users.Persistence;

/// <summary>The database context of the Users module. Its tables live in the <c>users</c> schema (ADR-0007).</summary>
internal sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : ModuleDbContext(options, Schema)
{
    public const string Schema = "users";
}
