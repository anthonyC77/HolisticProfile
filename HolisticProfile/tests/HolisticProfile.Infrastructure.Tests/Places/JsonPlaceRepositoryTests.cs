using FluentAssertions;
using HolisticProfile.Infrastructure.Places;

namespace HolisticProfile.Infrastructure.Tests.Places;

public class JsonPlaceRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _filePath;

    public JsonPlaceRepositoryTests()
    {
        _tempDir  = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _filePath = Path.Combine(_tempDir, "cities.json");

        File.WriteAllText(_filePath, """
        [
          { "name": "Paris",         "region": "Île-de-France", "country": "France",   "latitude": 48.8566, "longitude": 2.3522,  "timeZoneId": "Europe/Paris" },
          { "name": "Orléans",       "region": "Loiret",        "country": "France",   "latitude": 47.9029, "longitude": 1.9093,  "timeZoneId": "Europe/Paris" },
          { "name": "Saint-Étienne", "region": "Loire",         "country": "France",   "latitude": 45.4397, "longitude": 4.3872,  "timeZoneId": "Europe/Paris" },
          { "name": "Le Havre",      "region": "Seine-Maritime","country": "France",   "latitude": 49.4944, "longitude": 0.1079,  "timeZoneId": "Europe/Paris" },
          { "name": "Fort-de-France","region": "Martinique",    "country": "France",   "latitude": 14.6161, "longitude": -61.0588,"timeZoneId": "America/Martinique" },
          { "name": "Bruxelles",     "region": null,            "country": "Belgique", "latitude": 50.8503, "longitude": 4.3517,  "timeZoneId": "Europe/Brussels" }
        ]
        """);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    private JsonPlaceRepository Repository => new(_filePath);

    [Fact]
    public async Task SearchAsync_ExactName_ReturnsPlaceWithCoordinatesAndZone()
    {
        var results = await Repository.SearchAsync("Paris");

        var paris = results.Should().ContainSingle().Subject;
        paris.Name.Should().Be("Paris");
        paris.Latitude.Should().BeApproximately(48.8566, 0.0001);
        paris.Longitude.Should().BeApproximately(2.3522, 0.0001);
        paris.TimeZoneId.Should().Be("Europe/Paris");
    }

    [Theory]
    [InlineData("orleans")]   // sans accent
    [InlineData("ORLÉANS")]   // casse différente
    [InlineData("Orlé")]      // début de nom
    public async Task SearchAsync_IgnoresCaseAndAccents(string query)
    {
        var results = await Repository.SearchAsync(query);

        results.Should().ContainSingle(p => p.Name == "Orléans");
    }

    [Theory]
    [InlineData("saint etienne")] // tiret remplacé par un espace
    [InlineData("st etienne")]    // abréviation courante
    [InlineData("St-Etienne")]
    public async Task SearchAsync_HandlesSaintAbbreviationAndHyphens(string query)
    {
        var results = await Repository.SearchAsync(query);

        results.Should().ContainSingle(p => p.Name == "Saint-Étienne");
    }

    [Fact]
    public async Task SearchAsync_MatchesOnAnyWordOfTheName()
    {
        var results = await Repository.SearchAsync("havre");

        results.Should().ContainSingle(p => p.Name == "Le Havre");
    }

    [Fact]
    public async Task SearchAsync_MatchesOnRegionWhenNameDoesNotMatch()
    {
        var results = await Repository.SearchAsync("Martinique");

        results.Should().ContainSingle(p => p.Name == "Fort-de-France");
    }

    [Fact]
    public async Task SearchAsync_CountryName_ReturnsPlacesOfThatCountry()
    {
        var results = await Repository.SearchAsync("France");

        results.Should().HaveCount(5);
        results.Should().OnlyContain(p => p.Country == "France");

        // Fort-de-France correspond par le nom : il passe devant les correspondances par pays
        results[0].Name.Should().Be("Fort-de-France");
    }

    [Fact]
    public async Task SearchAsync_UnknownPlace_ReturnsEmpty()
    {
        var results = await Repository.SearchAsync("Zzzzville");

        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-")]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty(string query)
    {
        var results = await Repository.SearchAsync(query);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_RespectsMaxResults()
    {
        var results = await Repository.SearchAsync("France", maxResults: 2);

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_MissingFile_ThrowsFileNotFound()
    {
        var repo = new JsonPlaceRepository(Path.Combine(_tempDir, "absent.json"));

        var act = () => repo.SearchAsync("Paris");

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task SearchAsync_PlaceWithoutRegion_DisplaysCountryOnly()
    {
        var results = await Repository.SearchAsync("Bruxelles");

        results.Single().DisplayName.Should().Be("Bruxelles (Belgique)");
    }
}
