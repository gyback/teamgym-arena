using System.Reflection;
using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Common.Persistence;

/// <summary>Finds the strongly typed IDs that a module's model can use.</summary>
internal static class StronglyTypedIdConventions
{
    private static readonly Assembly s_commonAssembly = typeof(IStronglyTypedId<>).Assembly;

    /// <summary>
    /// Finds every strongly typed ID declared in <paramref name="moduleAssembly"/> or in an assembly it references,
    /// such as its own contracts and the contracts of other modules. Only assemblies that reference this shared kernel
    /// can declare one, so the others are skipped.
    /// </summary>
    public static IReadOnlyList<Type> FindIdTypes(Assembly moduleAssembly)
    {
        ArgumentNullException.ThrowIfNull(moduleAssembly);

        var commonName = s_commonAssembly.GetName().Name;

        return
        [
            .. moduleAssembly.GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Where(assembly => assembly.GetReferencedAssemblies().Any(reference => reference.Name == commonName))
                .Prepend(moduleAssembly)
                .Distinct()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(IsStronglyTypedId),
        ];
    }

    /// <summary>Gets the <see cref="StronglyTypedIdConverter{TId}"/> type for <paramref name="idType"/>.</summary>
    public static Type ConverterTypeFor(Type idType) => typeof(StronglyTypedIdConverter<>).MakeGenericType(idType);

    private static bool IsStronglyTypedId(Type type) =>
        type.IsValueType
        && type.GetInterfaces().Any(contract =>
            contract.IsGenericType
            && contract.GetGenericTypeDefinition() == typeof(IStronglyTypedId<>)
            && contract.GenericTypeArguments[0] == type);
}
