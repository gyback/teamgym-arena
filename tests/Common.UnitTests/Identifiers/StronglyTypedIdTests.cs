using Microsoft.Extensions.Time.Testing;
using TeamGym.Arena.Common.Identifiers;

namespace TeamGym.Arena.Common.UnitTests.Identifiers;

public sealed class StronglyTypedIdTests
{
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2027, 3, 14, 9, 30, 0, TimeSpan.Zero));

    [Fact]
    public void NewCreatesAVersion7Uuid()
    {
        var id = StronglyTypedId.New<SampleId>(_timeProvider);

        Assert.Equal(7, id.Value.Version);
    }

    [Fact]
    public void NewStampsTheIdWithTheCurrentTimeOfTheTimeProvider()
    {
        var id = StronglyTypedId.New<SampleId>(_timeProvider);

        // The first 48 bits of a UUIDv7 are the Unix time in milliseconds.
        var unixMilliseconds = Convert.ToInt64(id.Value.ToString("N")[..12], 16);
        Assert.Equal(_timeProvider.GetUtcNow().ToUnixTimeMilliseconds(), unixMilliseconds);
    }

    [Fact]
    public void IdsCreatedLaterSortAfterEarlierIds()
    {
        var earlier = StronglyTypedId.New<SampleId>(_timeProvider);
        _timeProvider.Advance(TimeSpan.FromMilliseconds(1));
        var later = StronglyTypedId.New<SampleId>(_timeProvider);

        Assert.True(earlier.Value.CompareTo(later.Value) < 0);
    }

    private readonly record struct SampleId(Guid Value) : IStronglyTypedId<SampleId>
    {
        public static SampleId From(Guid value) => new(value);
    }
}
