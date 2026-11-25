using TeamGym.Arena.Modules.Competitions.Domain.RuleSets;

namespace TeamGym.Arena.Modules.Competitions.UnitTests.Domain;

public sealed class TeamCompositionTests
{
    [Fact]
    public void CreateKeepsTheBounds()
    {
        var result = TeamComposition.Create(6, 12, 6, 10, 6);

        var composition = result.Value;
        Assert.Equal(6, composition.MinSquadSize);
        Assert.Equal(12, composition.MaxSquadSize);
        Assert.Equal(6, composition.MinFloorGymnasts);
        Assert.Equal(10, composition.MaxFloorGymnasts);
        Assert.Equal(6, composition.GymnastsPerRound);
    }

    [Fact]
    public void CreateAllowsAFixedSquadSize()
    {
        var result = TeamComposition.Create(6, 6, 6, 6, 6);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(11, 10)]
    public void CreateRejectsInvalidSquadBounds(int minSquadSize, int maxSquadSize)
    {
        var result = TeamComposition.Create(minSquadSize, maxSquadSize, 1, 1, 1);

        Assert.Equal(RuleSetErrors.InvalidSquadSize, result.Error);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 9)]
    [InlineData(9, 10)]
    [InlineData(8, 11)]
    public void CreateRejectsFloorBoundsOutsideTheSquadBounds(int minFloorGymnasts, int maxFloorGymnasts)
    {
        var result = TeamComposition.Create(8, 10, minFloorGymnasts, maxFloorGymnasts, 6);

        Assert.Equal(RuleSetErrors.InvalidFloorGymnasts, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public void CreateRejectsARoundThatTheSmallestSquadCannotField(int gymnastsPerRound)
    {
        var result = TeamComposition.Create(8, 10, 8, 10, gymnastsPerRound);

        Assert.Equal(RuleSetErrors.InvalidGymnastsPerRound, result.Error);
    }
}
