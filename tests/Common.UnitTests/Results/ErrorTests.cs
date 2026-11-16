using TeamGym.Arena.Common.Results;

namespace TeamGym.Arena.Common.UnitTests.Results;

public sealed class ErrorTests
{
    public static TheoryData<Func<string, string, Error>, ErrorType> Factories => new()
    {
        { Error.Failure, ErrorType.Failure },
        { Error.Validation, ErrorType.Validation },
        { Error.NotFound, ErrorType.NotFound },
        { Error.Conflict, ErrorType.Conflict },
        { Error.Forbidden, ErrorType.Forbidden },
    };

    [Theory]
    [MemberData(nameof(Factories))]
    public void FactorySetsTheMatchingType(Func<string, string, Error> factory, ErrorType expectedType)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var error = factory("Teams.NotFound", "The team was not found.");

        Assert.Equal(new Error("Teams.NotFound", "The team was not found.", expectedType), error);
    }
}
