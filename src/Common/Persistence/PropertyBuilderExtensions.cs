using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TeamGym.Arena.Common.Persistence;

/// <summary>Property mapping conventions that the modules' entity configurations share (ADR-0007).</summary>
public static class PropertyBuilderExtensions
{
    /// <summary>The maximum length of an enum member name stored by <see cref="StoreAsName{TEnum}"/>.</summary>
    public const int EnumNameMaxLength = 50;

    /// <summary>
    /// Stores an enum as its member name, such as <c>Club</c>, rather than as a number. The rows stay readable, and
    /// reordering the enum's members cannot change what stored rows mean.
    /// </summary>
    public static PropertyBuilder<TEnum> StoreAsName<TEnum>(this PropertyBuilder<TEnum> builder)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.HasConversion<string>().HasMaxLength(EnumNameMaxLength);
    }
}
