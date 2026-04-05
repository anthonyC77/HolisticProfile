using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using System.Text;

namespace HolisticProfile.Infrastructure.Prompt;

public class ReferentielPromptBuilder : IReferentielPromptBuilder
{
    public string Build(ReferentielNaissanceProfile profile, string knowledgeContent)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Tu es un assistant spécialisé en accompagnement thérapeutique.");
        sb.AppendLine("Tu utilises le Référentiel de Naissance (méthode Georges Colleuil, Tarot de Marseille).");
        sb.AppendLine("Réponds en français, avec un ton chaleureux et bienveillant, sans jargon ésotérique non expliqué.");
        sb.AppendLine();

        sb.AppendLine("## Profil");
        sb.AppendLine($"Date de naissance : {profile.BirthDate:dd/MM/yyyy} — Année en cours : {profile.CurrentYear}");
        sb.AppendLine();

        sb.AppendLine("### Les 14 Maisons (vue d'ensemble)");
        foreach (var house in profile.Houses)
            sb.AppendLine($"- M{house.HouseNumber} {house.HouseName} : {(house.Major is not null ? house.Major.ToString() : house.Minor!.ToString())}");

        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(knowledgeContent))
        {
            sb.AppendLine("## Détail des maisons clés");
            sb.AppendLine(knowledgeContent);
        }

        sb.AppendLine("## Consigne");
        sb.AppendLine("Produis une synthèse thérapeutique personnalisée (environ 400 mots) qui met en lumière :");
        sb.AppendLine("1. La mission de vie (M4) et le passage obligé (M5) — le cœur du chemin");
        sb.AppendLine("2. Les ressources disponibles (M6, M14) pour traverser ce passage");
        sb.AppendLine("3. Le soi profond (M9) et l'ombre à intégrer (M10)");
        sb.AppendLine("4. L'énergie de l'année en cours (M8) — ce qui est actif maintenant");
        sb.AppendLine("5. Les fils conducteurs entre les maisons (thèmes récurrents)");
        sb.AppendLine("Rends la synthèse directement utilisable en séance d'accompagnement.");

        return sb.ToString();
    }
}
