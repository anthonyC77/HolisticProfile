using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Infrastructure.Cache;

/// <summary>
/// Cache fichier pour les synthèses du Référentiel de Naissance.
/// Clé de cache : {JJ_MM_AAAA}_referentiel_{anneeEnCours}.md
/// La Maison 8 étant dynamique, le cache est par année.
/// </summary>
public class ReferentielSynthesisCacheRepository : IReferentielSynthesisCacheRepository
{
    private readonly string _basePath;

    public ReferentielSynthesisCacheRepository(string basePath)
    {
        _basePath = Path.GetFullPath(basePath);
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string?> LoadAsync(ReferentielNaissanceProfile profile)
    {
        var filePath = BuildFilePath(profile);
        if (!File.Exists(filePath)) return null;
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task SaveAsync(ReferentielNaissanceProfile profile, string synthesisText)
        => await File.WriteAllTextAsync(BuildFilePath(profile), synthesisText);

    private string BuildFilePath(ReferentielNaissanceProfile profile)
    {
        var datePart = profile.BirthDate.ToString("dd_MM_yyyy");
        var fileName = $"{datePart}_referentiel_{profile.CurrentYear}.md";
        return Path.Combine(_basePath, fileName);
    }
}
