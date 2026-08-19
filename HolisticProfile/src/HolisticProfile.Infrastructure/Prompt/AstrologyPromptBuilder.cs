using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using System.Text;

namespace HolisticProfile.Infrastructure.Prompt;

public class AstrologyPromptBuilder : IAstrologyPromptBuilder
{
    public string Build(NatalChartProfile profile, string knowledgeContent)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Tu es un assistant spécialisé en astrologie occidentale thérapeutique.");
        sb.AppendLine("Tu analyses le thème natal pour produire une synthèse personnalisée destinée à l'accompagnement.");
        sb.AppendLine("Réponds toujours en français, avec un ton chaleureux et bienveillant.");
        sb.AppendLine("Contextualise le vocabulaire astrologique pour le rendre accessible.");
        sb.AppendLine();

        // --- Données de naissance ---
        sb.AppendLine("## Données de naissance");
        sb.AppendLine($"Naissance : {profile.Input}");
        sb.AppendLine();

        // --- Positions planétaires ---
        sb.AppendLine("## Positions planétaires");
        sb.AppendLine($"**ASC** : {profile.Ascendant.DegreeInSign:F1}° {profile.Ascendant.Sign.ToFrench()}");
        sb.AppendLine($"**MC** : {profile.MidHeaven.DegreeInSign:F1}° {profile.MidHeaven.Sign.ToFrench()}");
        sb.AppendLine();

        var personalPlanets = new[] { Planet.Sun, Planet.Moon, Planet.Mercury, Planet.Venus, Planet.Mars };
        var socialPlanets   = new[] { Planet.Jupiter, Planet.Saturn };
        var outerPlanets    = new[] { Planet.Uranus, Planet.Neptune, Planet.Pluto };

        AppendPlanetGroup(sb, "Planètes personnelles", personalPlanets, profile);
        AppendPlanetGroup(sb, "Planètes sociales",     socialPlanets,   profile);
        AppendPlanetGroup(sb, "Planètes transpersonnelles", outerPlanets, profile);

        var northNode = profile.GetPlanet(Planet.NorthNode);
        if (northNode is not null)
            sb.AppendLine($"- {northNode}");

        sb.AppendLine();

        // --- Maisons ---
        sb.AppendLine("## Maisons astrologiques (Placidus)");
        foreach (var house in profile.Houses)
            sb.AppendLine($"- {house}");
        sb.AppendLine();

        // --- Aspects ---
        if (profile.Aspects.Count > 0)
        {
            sb.AppendLine("## Aspects majeurs");
            foreach (var aspect in profile.Aspects.Take(15))
                sb.AppendLine($"- {aspect}");
            sb.AppendLine();
        }

        // --- Knowledge base ---
        if (!string.IsNullOrWhiteSpace(knowledgeContent))
        {
            sb.AppendLine("## Base de connaissances");
            sb.AppendLine(knowledgeContent);
            sb.AppendLine();
        }

        // --- Consigne ---
        sb.AppendLine("## Consigne");
        sb.AppendLine("Produis une synthèse astrologique personnalisée et accessible qui met en lumière :");
        sb.AppendLine("- L'identité fondamentale (Soleil, Ascendant, Lune)");
        sb.AppendLine("- Les ressources et talents naturels");
        sb.AppendLine("- Les axes de tension et de croissance (aspects difficiles)");
        sb.AppendLine("- Les grandes harmoniques et soutiens (aspects doux)");
        sb.AppendLine("- Le chemin d'âme (Nœud Nord)");
        sb.AppendLine("La synthèse doit être directement utile comme support d'accompagnement thérapeutique.");

        return sb.ToString();
    }

    private static void AppendPlanetGroup(
        StringBuilder sb,
        string groupLabel,
        IEnumerable<Planet> planets,
        NatalChartProfile profile)
    {
        sb.AppendLine($"**{groupLabel}**");
        foreach (var planet in planets)
        {
            var pos = profile.GetPlanet(planet);
            if (pos is not null)
                sb.AppendLine($"- {pos}");
        }
    }
}
