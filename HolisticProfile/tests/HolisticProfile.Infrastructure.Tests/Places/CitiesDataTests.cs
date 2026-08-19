using FluentAssertions;
using HolisticProfile.Core.Models;
using HolisticProfile.Infrastructure.Places;
using NodaTime;
using System.Text.Json;

namespace HolisticProfile.Infrastructure.Tests.Places;

/// <summary>
/// Contrôles d'intégrité de la table livrée (data/places/cities.json) :
/// une coordonnée ou un fuseau erroné fausse silencieusement tout un thème natal.
/// </summary>
public class CitiesDataTests
{
    private static readonly string CitiesPath =
        Path.Combine(AppContext.BaseDirectory, "data", "places", "cities.json");

    private static List<Place> LoadPlaces()
    {
        File.Exists(CitiesPath).Should().BeTrue($"la table des lieux doit être copiée dans {CitiesPath}");

        var json = File.ReadAllText(CitiesPath);
        return JsonSerializer.Deserialize<List<Place>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    [Fact]
    public void Cities_AreLoadedAndCoverFranceAndAbroad()
    {
        var places = LoadPlaces();

        places.Should().HaveCountGreaterThan(150);
        places.Count(p => p.Country == "France").Should().BeGreaterThan(100);
        places.Select(p => p.Country).Distinct().Should().HaveCountGreaterThan(20);
    }

    [Fact]
    public void Cities_AllTimeZonesExistInTzdb()
    {
        var places = LoadPlaces();

        var unknown = places
            .Where(p => DateTimeZoneProviders.Tzdb.GetZoneOrNull(p.TimeZoneId) is null)
            .Select(p => $"{p.Name} → {p.TimeZoneId}")
            .ToList();

        unknown.Should().BeEmpty("tout fuseau déclaré doit être résolvable par NodaTime");
    }

    [Fact]
    public void Cities_AllCoordinatesAreInRange()
    {
        var places = LoadPlaces();

        places.Should().OnlyContain(p => p.Latitude  >= -90  && p.Latitude  <= 90);
        places.Should().OnlyContain(p => p.Longitude >= -180 && p.Longitude <= 180);
    }

    [Fact]
    public void Cities_HaveNoDuplicateEntries()
    {
        var places = LoadPlaces();

        var duplicates = places
            .GroupBy(p => (p.Name, p.Region, p.Country))
            .Where(g => g.Count() > 1)
            .Select(g => g.Key.ToString())
            .ToList();

        duplicates.Should().BeEmpty();
    }

    [Fact]
    public void Cities_HaveNameAndCountry()
    {
        var places = LoadPlaces();

        places.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Name));
        places.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Country));
    }

    [Fact]
    public async Task Repository_FindsAReferenceCity_WithUsableData()
    {
        var repo = new JsonPlaceRepository(CitiesPath);

        var lyon = (await repo.SearchAsync("Lyon")).First();

        lyon.Latitude.Should().BeApproximately(45.76, 0.05);
        lyon.Longitude.Should().BeApproximately(4.84, 0.05);
        lyon.TimeZoneId.Should().Be("Europe/Paris");
    }
}
