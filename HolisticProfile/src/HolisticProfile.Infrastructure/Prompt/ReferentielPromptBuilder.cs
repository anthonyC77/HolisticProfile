using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using System.Text;

namespace HolisticProfile.Infrastructure.Prompt;

public class ReferentielPromptBuilder : IReferentielPromptBuilder
{
    public string Build(ReferentielNaissanceProfile profile, string knowledgeContent)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Tu es un assistant spécialisé en accompagnement thérapeutique et développement personnel.");
        sb.AppendLine("Tu utilises le Référentiel de Naissance de Georges Colleuil (Tarot de Marseille) pour produire une synthèse personnalisée.");
        sb.AppendLine("Réponds toujours en français, avec un ton chaleureux et bienveillant.");
        sb.AppendLine("Évite le jargon ésotérique sauf si contextualisé et explicité.");
        sb.AppendLine();

        sb.AppendLine("## Profil calculé");
        sb.AppendLine($"Date de naissance : {profile.BirthDate:dd/MM/yyyy}");
        sb.AppendLine($"Année de référence pour la Maison 8 : {profile.CurrentYear}");
        sb.AppendLine();

        sb.AppendLine("### Les 14 Maisons");
        foreach (var house in profile.Houses)
            sb.AppendLine($"- {house}");

        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(knowledgeContent))
        {
            sb.AppendLine("## Base de connaissances");
            sb.AppendLine(knowledgeContent);
            sb.AppendLine();
        }

        sb.AppendLine("## Consigne");
        sb.AppendLine("Produis une synthèse personnalisée et accessible qui met en lumière :");
        sb.AppendLine("- Les fils conducteurs entre les maisons (thèmes récurrents)");
        sb.AppendLine("- La mission de vie (Maison 4) et le passage obligé (Maison 5)");
        sb.AppendLine("- Les ressources mobilisables (Maison 6, Maison 14)");
        sb.AppendLine("- L'ombre et les schémas d'échec (Maison 10)");
        sb.AppendLine("- L'énergie de l'année en cours (Maison 8)");
        sb.AppendLine("La synthèse doit être directement utile comme support d'accompagnement thérapeutique.");

        return sb.ToString();
    }
}
