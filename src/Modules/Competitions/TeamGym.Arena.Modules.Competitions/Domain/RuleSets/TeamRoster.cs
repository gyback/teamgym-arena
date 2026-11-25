namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>
/// The counts of a team's gymnasts by competitive gender: all that rule validation needs. It never holds gymnast
/// personal data (ADR-0006, ADR-0007).
/// </summary>
internal readonly record struct TeamRoster
{
    public TeamRoster(int femaleGymnastCount, int maleGymnastCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(femaleGymnastCount);
        ArgumentOutOfRangeException.ThrowIfNegative(maleGymnastCount);

        FemaleGymnastCount = femaleGymnastCount;
        MaleGymnastCount = maleGymnastCount;
    }

    public int FemaleGymnastCount { get; }

    public int MaleGymnastCount { get; }

    public int GymnastCount => FemaleGymnastCount + MaleGymnastCount;
}
