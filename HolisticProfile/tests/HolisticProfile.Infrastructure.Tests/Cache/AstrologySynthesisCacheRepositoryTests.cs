using FluentAssertions;
using HolisticProfile.Core.Models;
using HolisticProfile.Infrastructure.Cache;

namespace HolisticProfile.Infrastructure.Tests.Cache;

public class AstrologySynthesisCacheRepositoryTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"astro_cache_tests_{Guid.NewGuid():N}");
    private readonly AstrologySynthesisCacheRepository _repo;

    private static readonly NatalChartInput SampleInput = new(
        new DateTime(1987, 3, 15, 14, 30, 0),
        TimeSpan.FromHours(1),
        48.8566,
        2.3522,
        "Paris");

    public AstrologySynthesisCacheRepositoryTests()
        => _repo = new AstrologySynthesisCacheRepository(_tempDir);

    [Fact]
    public async Task LoadAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        var result = await _repo.LoadAsync(SampleInput);
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrip_ReturnsOriginalText()
    {
        const string expected = "Synthèse astrologique de test.";

        await _repo.SaveAsync(SampleInput, expected);
        var loaded = await _repo.LoadAsync(SampleInput);

        loaded.Should().Be(expected);
    }

    [Fact]
    public async Task SaveAsync_CreatesFileInBasePath()
    {
        await _repo.SaveAsync(SampleInput, "Test");

        var files = Directory.GetFiles(_tempDir, "*_astro.md");
        files.Should().HaveCount(1);
    }

    [Fact]
    public async Task DifferentInputs_ProduceDifferentCacheFiles()
    {
        var input2 = new NatalChartInput(
            new DateTime(1990, 6, 20, 10, 0, 0),
            TimeSpan.FromHours(2),
            45.7640,
            4.8357,
            "Lyon");

        await _repo.SaveAsync(SampleInput, "Synthèse Paris");
        await _repo.SaveAsync(input2, "Synthèse Lyon");

        var files = Directory.GetFiles(_tempDir, "*_astro.md");
        files.Should().HaveCount(2, "deux dates/lieux différents → deux fichiers distincts");
    }

    [Fact]
    public async Task SaveAsync_OverwritesPreviousCache()
    {
        await _repo.SaveAsync(SampleInput, "Ancienne synthèse");
        await _repo.SaveAsync(SampleInput, "Nouvelle synthèse");

        var loaded = await _repo.LoadAsync(SampleInput);
        loaded.Should().Be("Nouvelle synthèse");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
