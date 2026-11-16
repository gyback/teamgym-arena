using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Common.UnitTests.Results;

public sealed class ResultTests
{
    private static readonly Error s_error = Error.Conflict("Competitions.Full", "The competition is full.");

    [Fact]
    public void SuccessHasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void FailureCarriesTheError()
    {
        var result = Result.Failure(s_error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Same(s_error, result.Error);
    }

    [Fact]
    public void SuccessWithValueExposesTheValue()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void FailureWithValueTypeCarriesTheError()
    {
        var result = Result.Failure<int>(s_error);

        Assert.True(result.IsFailure);
        Assert.Same(s_error, result.Error);
    }

    [Fact]
    public void ReadingTheValueOfAFailureThrowsAndNamesTheErrorCode()
    {
        var result = Result.Failure<int>(s_error);

        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains(s_error.Code, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FailureRequiresAnError()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
        Assert.Throws<ArgumentNullException>(() => Result.Failure<int>(null!));
    }
}
