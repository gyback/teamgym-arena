namespace TeamGym.Arena.Common;

/// <summary>
/// A TeamGym section. <see cref="Mixed"/> is 50 % male and 50 % female; <see cref="Open"/> allows any gender
/// composition. The exact composition rules come from the configured rule set, not from this type.
/// </summary>
public enum Section
{
    Women,
    Men,
    Mixed,
    Open,
}
