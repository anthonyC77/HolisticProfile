using FluentAssertions;
using HolisticProfile.Core.Models;
using HolisticProfile.Infrastructure.Engines;

namespace HolisticProfile.Infrastructure.Tests.Engines;

/// <summary>
/// Tests du moteur de calcul Swiss Ephemeris (mode Moshier — aucun fichier éphémérides requis).
///
/// Cas de référence validé avec AstroSeek (15/03/1987 14:30, Paris, UTC+1) :
///   Ascendant   ≈ Cancer (env. 10–15°)
///   Soleil      ≈ Poissons (24°)
///   Lune        ≈ Scorpion
///   Mercure     ≈ Bélier (début)
/// </summary>
public class SwissEphAstrologyCalculationEngineTests : IDisposable
{
    private readonly SwissEphAstrologyCalculationEngine _engine = new(ephemerisPath: null);

    // ─── Entrée de référence — Marie, née 15/03/1987 à 14:30 à Paris ─────────

    private static readonly NatalChartInput ParisMar1987 = new(
        new DateTime(1987, 3, 15, 14, 30, 0),
        TimeSpan.FromHours(1),
        48.8566,
        2.3522,
        "Paris");

    // ─── Tests de structure ───────────────────────────────────────────────────

    [Fact]
    public void Calculate_Returns12Houses()
    {
        var profile = _engine.Calculate(ParisMar1987);

        profile.Houses.Should().HaveCount(12);
    }

    [Fact]
    public void Calculate_ReturnsPlanetsForAllExpectedBodies()
    {
        var profile = _engine.Calculate(ParisMar1987);

        var planets = profile.Planets.Select(p => p.Planet).ToList();

        planets.Should().Contain(Planet.Sun,     "le Soleil est toujours calculable");
        planets.Should().Contain(Planet.Moon,    "la Lune est toujours calculable");
        planets.Should().Contain(Planet.Mercury, "Mercure est toujours calculable");
        planets.Should().Contain(Planet.Venus,   "Vénus est toujours calculable");
        planets.Should().Contain(Planet.Mars,    "Mars est toujours calculable");
    }

    [Fact]
    public void Calculate_AllPlanetLongitudesInRange()
    {
        var profile = _engine.Calculate(ParisMar1987);

        foreach (var p in profile.Planets)
        {
            p.Longitude.Should().BeInRange(0, 360,
                $"{p.Planet} doit avoir une longitude entre 0 et 360°");
            p.DegreeInSign.Should().BeInRange(0, 30,
                $"{p.Planet} doit avoir un degré dans le signe entre 0 et 30");
        }
    }

    [Fact]
    public void Calculate_AllHouseLongitudesInRange()
    {
        var profile = _engine.Calculate(ParisMar1987);

        foreach (var h in profile.Houses)
        {
            h.Longitude.Should().BeInRange(0, 360, $"Maison {h.Number}");
            h.DegreeInSign.Should().BeInRange(0, 30, $"Maison {h.Number}");
            h.Number.Should().BeInRange(1, 12);
        }
    }

    [Fact]
    public void Calculate_AllPlanetsInValidHouse()
    {
        var profile = _engine.Calculate(ParisMar1987);

        foreach (var p in profile.Planets)
            p.House.Should().BeInRange(1, 12, $"{p.Planet} doit être dans une maison valide");
    }

    [Fact]
    public void Calculate_AspectOrbsArePositiveAndWithinMaxOrb()
    {
        var profile = _engine.Calculate(ParisMar1987);

        foreach (var a in profile.Aspects)
        {
            a.Orb.Should().BeGreaterThanOrEqualTo(0, $"l'orbe de {a} doit être positif");
            a.Orb.Should().BeLessThanOrEqualTo(a.Type.MaxOrb(),
                $"l'orbe de {a} doit être ≤ l'orbe max ({a.Type.MaxOrb()}°)");
        }
    }

    // ─── Tests astronomiques de référence ─────────────────────────────────────

    [Fact]
    public void Calculate_ParisMar1987_SunInPisces()
    {
        var profile = _engine.Calculate(ParisMar1987);

        var sun = profile.GetPlanet(Planet.Sun);
        sun.Should().NotBeNull();
        sun!.Sign.Should().Be(ZodiacSign.Pisces, "au 15/03/1987 le Soleil est en Poissons (~24°)");
    }

    [Fact]
    public void Calculate_ParisMar1987_AscendantIsCancerOrLeo()
    {
        var profile = _engine.Calculate(ParisMar1987);

        // 14h30 heure locale Paris (UTC+1) → 13h30 UT
        // L'ascendant pour Paris à cette heure se situe en Cancer ou Lion
        profile.Ascendant.Sign.Should().BeOneOf(
            new[] { ZodiacSign.Cancer, ZodiacSign.Leo },
            "l'ascendant à 14h30 à Paris le 15/03/1987 est en Cancer ou Lion (précision Moshier)");
    }

    [Fact]
    public void Calculate_InputPreservedInProfile()
    {
        var profile = _engine.Calculate(ParisMar1987);

        profile.Input.Should().Be(ParisMar1987);
        profile.Input.PlaceName.Should().Be("Paris");
        profile.Input.Latitude.Should().BeApproximately(48.8566, 0.0001);
    }

    // ─── Tests des helpers géométriques ───────────────────────────────────────

    [Theory]
    [InlineData(0,   0,   0)]    // même longitude
    [InlineData(0,  180, 180)]   // opposition
    [InlineData(0,   90,  90)]   // carré
    [InlineData(10, 370,   0)]   // modulo 360
    [InlineData(350,  10,  20)]  // enjambement 0°
    public void AngleDifference_ReturnsExpected(double lon1, double lon2, double expected)
        => SwissEphAstrologyCalculationEngine.AngleDifference(lon1, lon2)
           .Should().BeApproximately(expected, 0.001);

    [Theory]
    [InlineData(15, new[] { 0.0, 10.0, 40.0, 70.0, 100.0, 130.0, 160.0, 190.0, 220.0, 250.0, 280.0, 310.0, 340.0 }, 1)]
    [InlineData(55, new[] { 0.0, 10.0, 40.0, 70.0, 100.0, 130.0, 160.0, 190.0, 220.0, 250.0, 280.0, 310.0, 340.0 }, 2)]
    [InlineData(5,  new[] { 0.0, 350.0, 20.0, 50.0, 80.0, 110.0, 140.0, 170.0, 200.0, 230.0, 260.0, 290.0, 320.0 }, 1)] // H1 enjambe 0°
    public void GetHouseNumber_ReturnsExpected(double lon, double[] cusps, int expectedHouse)
        => SwissEphAstrologyCalculationEngine.GetHouseNumber(lon, cusps)
           .Should().Be(expectedHouse);

    // ─── Validation des entrées ───────────────────────────────────────────────

    [Fact]
    public void Calculate_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _engine.Calculate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ─── IDisposable ─────────────────────────────────────────────────────────

    public void Dispose() => _engine.Dispose();
}
