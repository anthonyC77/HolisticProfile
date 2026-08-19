using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using System.Text;

namespace HolisticProfile.Infrastructure.KnowledgeBase;

/// <summary>
/// Charge les fiches Markdown de la base de connaissances astrologique.
///
/// Structure attendue :
///   {BasePath}/
///     planets_in_signs/{planet}_{sign}.md     ex: sun_aries.md
///     planets_in_houses/{planet}_house_{N}.md ex: moon_house_4.md
///     aspects/{planet1}_{aspect}_{planet2}.md ex: sun_square_moon.md
///
/// Les fichiers absents sont silencieusement ignorés.
/// </summary>
public class AstrologyKnowledgeBaseRepository : IAstrologyKnowledgeBaseRepository
{
    private readonly string _basePath;

    public AstrologyKnowledgeBaseRepository(string basePath)
    {
        _basePath = Path.GetFullPath(basePath);
    }

    public async Task<string> LoadProfileContentAsync(NatalChartProfile profile)
    {
        var sb = new StringBuilder();

        foreach (var planet in profile.Planets)
        {
            // Planète en signe
            var inSignContent = await TryReadAsync(
                "planets_in_signs",
                $"{planet.Planet.ToFileKey()}_{planet.Sign.ToFileKey()}.md");

            if (!string.IsNullOrWhiteSpace(inSignContent))
            {
                sb.AppendLine($"### {planet.Planet.ToFrench()} en {planet.Sign.ToFrench()}");
                sb.AppendLine(inSignContent);
                sb.AppendLine();
            }

            // Planète en maison
            var inHouseContent = await TryReadAsync(
                "planets_in_houses",
                $"{planet.Planet.ToFileKey()}_house_{planet.House}.md");

            if (!string.IsNullOrWhiteSpace(inHouseContent))
            {
                sb.AppendLine($"### {planet.Planet.ToFrench()} en Maison {planet.House}");
                sb.AppendLine(inHouseContent);
                sb.AppendLine();
            }
        }

        // Aspects majeurs (orbe ≤ 3° = très puissants)
        var majorAspects = profile.Aspects
            .Where(a => a.Orb <= 3.0)
            .Take(8); // limite pour ne pas saturer le contexte LLM

        foreach (var aspect in majorAspects)
        {
            var aspectContent = await TryReadAsync(
                "aspects",
                $"{aspect.Planet1.ToFileKey()}_{aspect.Type.ToFileKey()}_{aspect.Planet2.ToFileKey()}.md");

            if (!string.IsNullOrWhiteSpace(aspectContent))
            {
                sb.AppendLine($"### Aspect : {aspect.Planet1.ToFrench()} {aspect.Type.ToFrench()} {aspect.Planet2.ToFrench()}");
                sb.AppendLine(aspectContent);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private async Task<string?> TryReadAsync(string subfolder, string fileName)
    {
        var path = Path.Combine(_basePath, subfolder, fileName);
        if (!File.Exists(path)) return null;
        return await File.ReadAllTextAsync(path);
    }
}
