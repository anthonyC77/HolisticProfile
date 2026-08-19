using FluentAssertions;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Tests.Models;

public class NatalChartInputTests
{
    private static NatalChartInput Paris(TimeSpan offset) => new(
        new DateTime(1987, 3, 15, 14, 30, 0), offset, 48.8566, 2.3522, "Paris");

    [Fact]
    public void BirthDateTimeUT_SubtractsUtcOffset()
    {
        var input = Paris(TimeSpan.FromHours(1));

        input.BirthDateTimeUT.Should().Be(new DateTime(1987, 3, 15, 13, 30, 0));
    }

    [Fact]
    public void BirthDateTimeUT_NegativeOffset_AddsHours()
    {
        var input = Paris(TimeSpan.FromHours(-5));

        input.BirthDateTimeUT.Should().Be(new DateTime(1987, 3, 15, 19, 30, 0));
    }

    [Fact]
    public void CacheKey_IncludesDateTimeCoordinatesAndOffset()
    {
        var input = Paris(TimeSpan.FromHours(1));

        input.CacheKey.Should().Be("15_03_1987_1430_48p8566_2p3522_utcp0100");
    }

    [Fact]
    public void CacheKey_DiffersWhenOnlyTheOffsetDiffers()
    {
        // Deux décalages = deux thèmes : le cache ne doit pas les confondre
        var winter = Paris(TimeSpan.FromHours(1));
        var summer = Paris(TimeSpan.FromHours(2));

        winter.CacheKey.Should().NotBe(summer.CacheKey);
    }

    [Fact]
    public void CacheKey_NegativeOffset_IsMarked()
    {
        var input = Paris(TimeSpan.FromHours(-5));

        input.CacheKey.Should().EndWith("_utcm0500");
    }

    [Fact]
    public void CacheKey_OffsetWithSeconds_KeepsSeconds()
    {
        // Heure du méridien de Paris (avant 1911) : +0h09'21"
        var input = Paris(new TimeSpan(0, 9, 21));

        input.CacheKey.Should().EndWith("_utcp000921");
    }

    [Fact]
    public void CacheKey_ContainsNoPathSeparator()
    {
        var input = Paris(TimeSpan.FromHours(-3.5));

        input.CacheKey.Should().NotContainAny("/", "\\", ":", ".");
    }
}
