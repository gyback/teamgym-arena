using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Modules.Organisations.Contracts;

/// <summary>Identifies a gymnast, which a club owns.</summary>
public readonly record struct GymnastId(Guid Value) : IStronglyTypedId<GymnastId>
{
    public static GymnastId From(Guid value) => new(value);
}
