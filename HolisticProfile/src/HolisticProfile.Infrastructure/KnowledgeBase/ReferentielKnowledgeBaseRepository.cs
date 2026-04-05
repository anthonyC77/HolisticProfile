using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Infrastructure.KnowledgeBase;

/// <summary>
/// Charge les fiches Markdown du Référentiel de Naissance depuis le filesystem.
///
/// Structure attendue :
///   {basePath}/arcanes/arcane_XX_*.md   (arcanes majeurs)
///   {basePath}/positions/position_YY_*.md (positions/maisons)
///
/// La méthode LoadAsync(profile) retourne la concaténation de :
///   - la fiche de l'arcane de chaque maison
///   - la fiche de la position de chaque maison
/// </summary>
public class ReferentielKnowledgeBaseRepository : IReferentielKnowledgeBaseRepository
{
    private readonly string _basePath;

    public ReferentielKnowledgeBaseRepository(string basePath)
    {
        _basePath = Path.GetFullPath(basePath);
    }

    /// <summary>Charge la fiche d'un arcane majeur (ex: numéro 6 → arcane_06_*.md).</summary>
    public async Task<string?> LoadArcaneAsync(int arcaneNumero)
    {
        var prefix = $"arcane_{arcaneNumero:D2}_";
        return await LoadByPrefixAsync(Path.Combine(_basePath, "arcanes"), prefix);
    }

    /// <summary>Charge la fiche d'une position/maison (ex: maison 3 → position_03_*.md).</summary>
    public async Task<string?> LoadPositionAsync(int houseNumber)
    {
        var prefix = $"position_{houseNumber:D2}_";
        return await LoadByPrefixAsync(Path.Combine(_basePath, "positions"), prefix);
    }

    /// <summary>
    /// Charge le contenu de toutes les maisons du profil sous forme de texte consolidé.
    /// Chaque maison = fiche arcane + fiche position.
    /// </summary>
    public async Task<string> LoadProfileContentAsync(ReferentielNaissanceProfile profile)
    {
        var sb = new System.Text.StringBuilder();

        foreach (var house in profile.Houses)
        {
            sb.AppendLine($"## Maison {house.HouseNumber} — {house.HouseName}");

            if (house.Major is not null)
            {
                var arcaneContent   = await LoadArcaneAsync(house.Major.Numero);
                var positionContent = await LoadPositionAsync(house.HouseNumber);

                sb.AppendLine($"**Arcane :** {house.Major}");
                sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(arcaneContent))
                    sb.AppendLine(arcaneContent);

                if (!string.IsNullOrWhiteSpace(positionContent))
                    sb.AppendLine(positionContent);
            }
            else if (house.Minor is not null)
            {
                sb.AppendLine($"**Arcane Mineur :** {house.Minor}");
                sb.AppendLine();

                var positionContent = await LoadPositionAsync(house.HouseNumber);
                if (!string.IsNullOrWhiteSpace(positionContent))
                    sb.AppendLine(positionContent);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Charge uniquement les maisons spécifiées (par numéro) pour alléger le prompt LLM.
    /// </summary>
    public async Task<string> LoadKeyHousesContentAsync(
        ReferentielNaissanceProfile profile,
        IEnumerable<int> houseNumbers)
    {
        var sb = new System.Text.StringBuilder();
        var numbers = houseNumbers.ToHashSet();

        foreach (var house in profile.Houses.Where(h => numbers.Contains(h.HouseNumber)))
        {
            sb.AppendLine($"## Maison {house.HouseNumber} — {house.HouseName}");

            if (house.Major is not null)
            {
                sb.AppendLine($"**Arcane :** {house.Major}");
                sb.AppendLine();

                var arcaneContent   = await LoadArcaneAsync(house.Major.Numero);
                var positionContent = await LoadPositionAsync(house.HouseNumber);

                if (!string.IsNullOrWhiteSpace(arcaneContent))
                    sb.AppendLine(arcaneContent);

                if (!string.IsNullOrWhiteSpace(positionContent))
                    sb.AppendLine(positionContent);
            }
            else if (house.Minor is not null)
            {
                sb.AppendLine($"**Arcane Mineur :** {house.Minor}");
                sb.AppendLine();

                var positionContent = await LoadPositionAsync(house.HouseNumber);
                if (!string.IsNullOrWhiteSpace(positionContent))
                    sb.AppendLine(positionContent);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private async Task<string?> LoadByPrefixAsync(string directory, string prefix)
    {
        if (!Directory.Exists(directory))
            return null;

        // Sécurité : le préfixe ne doit pas contenir de séparateurs de chemin
        if (prefix.Contains('/') || prefix.Contains('\\') || prefix.Contains(".."))
            return null;

        var file = Directory
            .EnumerateFiles(directory, $"{prefix}*.md")
            .FirstOrDefault();

        if (file is null) return null;

        var resolved = Path.GetFullPath(file);
        if (!resolved.StartsWith(Path.GetFullPath(directory), StringComparison.OrdinalIgnoreCase))
            return null;

        return await File.ReadAllTextAsync(resolved);
    }
}
