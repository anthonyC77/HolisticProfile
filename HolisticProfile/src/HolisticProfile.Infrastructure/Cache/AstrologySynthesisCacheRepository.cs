using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Infrastructure.Cache;

/// <summary>
/// Cache fichier des synthèses astrologiques.
/// Clé de cache : {CacheKey}_astro.md (date + heure + lieu).
/// </summary>
public class AstrologySynthesisCacheRepository : IAstrologySynthesisCacheRepository
{
    private readonly string _basePath;

    public AstrologySynthesisCacheRepository(string basePath)
    {
        _basePath = Path.GetFullPath(basePath);
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string?> LoadAsync(NatalChartInput input)
    {
        var filePath = BuildFilePath(input);
        if (!File.Exists(filePath)) return null;
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task SaveAsync(NatalChartInput input, string synthesisText)
        => await File.WriteAllTextAsync(BuildFilePath(input), synthesisText);

    private string BuildFilePath(NatalChartInput input)
        => Path.Combine(_basePath, $"{input.CacheKey}_astro.md");
}
