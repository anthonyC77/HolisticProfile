using FluentAssertions;
using HolisticProfile.Core.Engines;
using HolisticProfile.Core.Models;
using HolisticProfile.Infrastructure.Cache;

namespace HolisticProfile.Infrastructure.Tests.Cache;

public class FileSynthesisCacheRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileSynthesisCacheRepository _repo;
    private readonly MillmanCalculationEngine _engine = new();

    public FileSynthesisCacheRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _repo = new FileSynthesisCacheRepository(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public async Task SaveAsync_CreatesFileInCacheDirectory()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15)); // 34/7

        await _repo.SaveAsync(profile, "synthèse de test");

        Directory.GetFiles(_tempDir).Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveAsync_FileNameContainsBirthDateAndPathKey()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15)); // 34/7

        await _repo.SaveAsync(profile, "synthèse de test");

        var file = Directory.GetFiles(_tempDir).Single();
        Path.GetFileName(file).Should().Contain("15_03_1987");
        Path.GetFileName(file).Should().Contain("34_7");
    }

    [Fact]
    public async Task LoadAsync_ExistingCache_ReturnsSavedText()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15));
        var expected = "synthèse persistée";

        await _repo.SaveAsync(profile, expected);
        var result = await _repo.LoadAsync(profile);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task LoadAsync_NoCache_ReturnsNull()
    {
        var profile = MakeProfile(new DateTime(1992, 12, 17)); // pas de cache

        var result = await _repo.LoadAsync(profile);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingCache()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15));

        await _repo.SaveAsync(profile, "première synthèse");
        await _repo.SaveAsync(profile, "synthèse mise à jour");
        var result = await _repo.LoadAsync(profile);

        result.Should().Be("synthèse mise à jour");
        Directory.GetFiles(_tempDir).Should().HaveCount(1); // pas de doublon
    }

    [Fact]
    public async Task SaveAsync_ThreePartPath_CreatesCorrectFileName()
    {
        var profile = MakeProfile(new DateTime(1964, 7, 2)); // 29/11/2

        await _repo.SaveAsync(profile, "synthèse 29/11/2");

        var file = Directory.GetFiles(_tempDir).Single();
        Path.GetFileName(file).Should().Contain("02_07_1964");
        Path.GetFileName(file).Should().Contain("29_11_2");
    }

    private BirthProfile MakeProfile(DateTime date)
    {
        var lifePath = _engine.Calculate(date);
        return new BirthProfile(date, lifePath);
    }
}
