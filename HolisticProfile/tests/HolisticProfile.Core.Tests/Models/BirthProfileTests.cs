using FluentAssertions;
using HolisticProfile.Core.Engines;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Tests.Models;

public class BirthProfileTests
{
    private readonly MillmanCalculationEngine _engine = new();

    [Fact]
    public void BirthProfile_Creation_StoresBirthDateAndLifePath()
    {
        var date = new DateTime(1987, 3, 15);
        var lifePath = _engine.Calculate(date);

        var profile = new BirthProfile(date, lifePath);

        profile.BirthDate.Should().Be(date);
        profile.MillmanLifePath.Should().Be(lifePath);
    }

    [Fact]
    public void BirthProfile_TwoPartPath_PathKeyMatchesLifePath()
    {
        var date = new DateTime(1987, 3, 15); // 34/7
        var lifePath = _engine.Calculate(date);
        var profile = new BirthProfile(date, lifePath);

        profile.MillmanLifePath.PathKey.Should().Be("34_7");
    }

    [Fact]
    public void BirthProfile_ThreePartPath_PathKeyIncludesAllThreeLevels()
    {
        var date = new DateTime(1964, 7, 2); // 29/11/2
        var lifePath = _engine.Calculate(date);
        var profile = new BirthProfile(date, lifePath);

        profile.MillmanLifePath.PathKey.Should().Be("29_11_2");
    }

    [Fact]
    public void BirthProfile_IsImmutable()
    {
        var date = new DateTime(1987, 3, 15);
        var lifePath = _engine.Calculate(date);

        var profile1 = new BirthProfile(date, lifePath);
        var profile2 = new BirthProfile(date, lifePath);

        profile1.Should().Be(profile2);
    }
}
