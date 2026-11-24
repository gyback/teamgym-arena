using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Modules.Users.Contracts;

/// <summary>Identifies a user. Every other module refers to users only by this ID, never by identity provider identifiers (ADR-0008).</summary>
public readonly record struct UserId(Guid Value) : IStronglyTypedId<UserId>
{
    public static UserId From(Guid value) => new(value);
}
