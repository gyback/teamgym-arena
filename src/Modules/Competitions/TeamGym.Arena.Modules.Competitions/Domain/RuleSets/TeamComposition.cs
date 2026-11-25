using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

/// <summary>
/// The size rules for a team in one class (ADR-0006): how many gymnasts the squad has, how many of them compete on
/// <b>Floor</b>, and how many perform in each round on <b>Tumble</b> and <b>Trampet</b>.
/// </summary>
internal sealed record TeamComposition
{
    private TeamComposition(
        int minSquadSize,
        int maxSquadSize,
        int minFloorGymnasts,
        int maxFloorGymnasts,
        int gymnastsPerRound)
    {
        MinSquadSize = minSquadSize;
        MaxSquadSize = maxSquadSize;
        MinFloorGymnasts = minFloorGymnasts;
        MaxFloorGymnasts = maxFloorGymnasts;
        GymnastsPerRound = gymnastsPerRound;
    }

    public int MinSquadSize { get; }

    public int MaxSquadSize { get; }

    /// <summary>The fewest gymnasts that compete on <b>Floor</b>.</summary>
    public int MinFloorGymnasts { get; }

    /// <summary>The most gymnasts that compete on <b>Floor</b>.</summary>
    public int MaxFloorGymnasts { get; }

    /// <summary>The number of gymnasts that perform in each round on <b>Tumble</b> and <b>Trampet</b>.</summary>
    public int GymnastsPerRound { get; }

    /// <summary>
    /// Creates the size rules. Every squad the rules allow must be able to field a <b>Floor</b> team and a full
    /// <b>Tumble</b> and <b>Trampet</b> round, so the <b>Floor</b> bounds and the round size may not exceed the
    /// squad bounds.
    /// </summary>
    public static Result<TeamComposition> Create(
        int minSquadSize,
        int maxSquadSize,
        int minFloorGymnasts,
        int maxFloorGymnasts,
        int gymnastsPerRound)
    {
        if (minSquadSize < 1 || minSquadSize > maxSquadSize)
        {
            return Result.Failure<TeamComposition>(RuleSetErrors.InvalidSquadSize);
        }

        if (minFloorGymnasts < 1
            || minFloorGymnasts > maxFloorGymnasts
            || minFloorGymnasts > minSquadSize
            || maxFloorGymnasts > maxSquadSize)
        {
            return Result.Failure<TeamComposition>(RuleSetErrors.InvalidFloorGymnasts);
        }

        if (gymnastsPerRound < 1 || gymnastsPerRound > minSquadSize)
        {
            return Result.Failure<TeamComposition>(RuleSetErrors.InvalidGymnastsPerRound);
        }

        return Result.Success(new TeamComposition(
            minSquadSize, maxSquadSize, minFloorGymnasts, maxFloorGymnasts, gymnastsPerRound));
    }
}
