using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace TeamGym.Arena.MigrationService.DesignTime;

/// <summary>
/// Generates migrations as <c>internal</c> classes, because every type in a module except its registration class is
/// internal (ADR-0004), which the architecture tests enforce. EF Core finds internal migrations as well. The model
/// snapshot and the migration's designer file are already generated without an access modifier.
/// </summary>
internal sealed class InternalMigrationsGenerator(
    MigrationsCodeGeneratorDependencies dependencies,
    CSharpMigrationsGeneratorDependencies csharpDependencies)
    : CSharpMigrationsGenerator(dependencies, csharpDependencies)
{
    public override string GenerateMigration(
        string? migrationNamespace,
        string migrationName,
        IReadOnlyList<MigrationOperation> upOperations,
        IReadOnlyList<MigrationOperation> downOperations)
    {
        var code = base.GenerateMigration(migrationNamespace, migrationName, upOperations, downOperations);
        var publicDeclaration = $"public partial class {migrationName} ";

        if (!code.Contains(publicDeclaration, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The generated migration has no '{publicDeclaration.TrimEnd()}' declaration to make internal. EF Core's code generation may have changed.");
        }

        return code.Replace(publicDeclaration, $"internal partial class {migrationName} ", StringComparison.Ordinal);
    }
}
