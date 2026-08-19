using FluentAssertions;
using HolisticProfile.Core.Models;
using HolisticProfile.Infrastructure.Places;

namespace HolisticProfile.Infrastructure.Tests.Places;

/// <summary>
/// Le décalage UTC conditionne directement l'Ascendant : une heure d'erreur déplace
/// l'ASC d'environ 15°, soit souvent une maison entière. Ces tests verrouillent
/// les cas historiques que Windows ne sait pas restituer.
/// </summary>
public class NodaTimeZoneResolverTests
{
    private readonly NodaTimeZoneResolver _resolver = new();

    // ─── France — règles historiques ──────────────────────────────────────────

    [Fact]
    public void Resolve_ParisWinter1987_ReturnsPlusOne()
    {
        var result = _resolver.Resolve(new DateTime(1987, 3, 15, 14, 30, 0), "Europe/Paris");

        result.UtcOffset.Should().Be(TimeSpan.FromHours(1));
        result.Kind.Should().Be(BirthTimeKind.Unique);
    }

    [Fact]
    public void Resolve_ParisSummer1977_ReturnsPlusTwo()
    {
        // Heure d'été réintroduite en France le 28 mars 1976
        var result = _resolver.Resolve(new DateTime(1977, 6, 30, 17, 35, 0), "Europe/Paris");

        result.UtcOffset.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void Resolve_ParisSummer1975_ReturnsPlusOne_NoDaylightSavingYet()
    {
        // En 1975 la France n'appliquait pas encore l'heure d'été
        var result = _resolver.Resolve(new DateTime(1975, 6, 30, 17, 35, 0), "Europe/Paris");

        result.UtcOffset.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void Resolve_ParisBefore1911_ReturnsParisMeanTime()
    {
        // Avant le 11 mars 1911, la France vivait à l'heure du méridien de Paris (+9 min 21 s)
        var result = _resolver.Resolve(new DateTime(1900, 6, 15, 10, 0, 0), "Europe/Paris");

        result.UtcOffset.Should().Be(new TimeSpan(0, 9, 21));
    }

    // ─── Transitions d'heure d'été ────────────────────────────────────────────

    [Fact]
    public void Resolve_AmbiguousHour_FlagsBothOffsets()
    {
        // 29/10/2023 : les horloges reculent de 03:00 à 02:00 — 02:30 est vécu deux fois
        var result = _resolver.Resolve(new DateTime(2023, 10, 29, 2, 30, 0), "Europe/Paris");

        result.Kind.Should().Be(BirthTimeKind.Ambiguous);
        result.NeedsConfirmation.Should().BeTrue();
        result.UtcOffset.Should().Be(TimeSpan.FromHours(2));            // première occurrence
        result.AlternativeUtcOffset.Should().Be(TimeSpan.FromHours(1)); // seconde occurrence
    }

    [Fact]
    public void Resolve_SkippedHour_IsFlagged()
    {
        // 26/03/2023 : les horloges avancent de 02:00 à 03:00 — 02:30 n'a pas existé
        var result = _resolver.Resolve(new DateTime(2023, 3, 26, 2, 30, 0), "Europe/Paris");

        result.Kind.Should().Be(BirthTimeKind.Skipped);
        result.NeedsConfirmation.Should().BeTrue();
        result.AlternativeUtcOffset.Should().BeNull();
    }

    [Fact]
    public void Resolve_NormalHour_NeedsNoConfirmation()
    {
        var result = _resolver.Resolve(new DateTime(2023, 7, 14, 12, 0, 0), "Europe/Paris");

        result.Kind.Should().Be(BirthTimeKind.Unique);
        result.NeedsConfirmation.Should().BeFalse();
    }

    // ─── Autres fuseaux ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("America/New_York", 1987, 3, 15, -5)]  // heure d'été US à partir du 5 avril 1987
    [InlineData("America/New_York", 1987, 7, 15, -4)]
    [InlineData("Asia/Tokyo",       1987, 3, 15,  9)]
    [InlineData("America/Martinique", 1987, 7, 15, -4)]
    [InlineData("Indian/Reunion",     1987, 7, 15,  4)]
    public void Resolve_OtherZones_ReturnsExpectedOffset(
        string zoneId, int year, int month, int day, int expectedHours)
    {
        var result = _resolver.Resolve(new DateTime(year, month, day, 12, 0, 0), zoneId);

        result.UtcOffset.Should().Be(TimeSpan.FromHours(expectedHours));
    }

    [Fact]
    public void Resolve_HalfHourZone_ReturnsFractionalOffset()
    {
        var result = _resolver.Resolve(new DateTime(1987, 3, 15, 12, 0, 0), "Asia/Kolkata");

        result.UtcOffset.Should().Be(new TimeSpan(5, 30, 0));
    }

    // ─── Erreurs ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Europe/Atlantide")]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_InvalidZone_ThrowsArgumentException(string zoneId)
    {
        var act = () => _resolver.Resolve(new DateTime(1987, 3, 15, 14, 30, 0), zoneId);

        act.Should().Throw<ArgumentException>();
    }
}
