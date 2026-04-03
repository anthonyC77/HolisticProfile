using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using System.Text;

namespace HolisticProfile.Infrastructure.Prompt;

public class MillmanPromptBuilder : IPromptBuilder
{
    public string Build(BirthProfile profile, string? knowledgeContent)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Tu es un assistant spécialisé en accompagnement thérapeutique et développement personnel.");
        sb.AppendLine("Tu utilises la numérologie de Dan Millman pour produire une synthèse personnalisée.");
        sb.AppendLine("Réponds toujours en français, avec un ton chaleureux et bienveillant.");
        sb.AppendLine("Évite le jargon technique sauf si contextualisé.");
        sb.AppendLine();

        sb.AppendLine("## Profil calculé");
        sb.AppendLine($"Date de naissance : {profile.BirthDate:dd/MM/yyyy}");
        sb.AppendLine($"Chemin de vie Millman : {profile.MillmanLifePath}");

        if (profile.MillmanLifePath.IntermediateSum.HasValue)
        {
            sb.AppendLine($"(Chemin à 3 niveaux : {profile.MillmanLifePath.Sum} / " +
                          $"{profile.MillmanLifePath.IntermediateSum} / {profile.MillmanLifePath.Root} — " +
                          $"chaque niveau porte un sens, les chiffres répétés ont un double poids)");
        }

        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(knowledgeContent))
        {
            sb.AppendLine("## Base de connaissances");
            sb.AppendLine(knowledgeContent);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("## Base de connaissances");
            sb.AppendLine("(Aucune fiche spécifique disponible pour ce chemin. Utilise ta connaissance générale du système Millman.)");
            sb.AppendLine();
        }

        sb.AppendLine("## Consigne");
        sb.AppendLine("Produis une synthèse personnalisée et accessible qui met en lumière :");
        sb.AppendLine("- Les forces principales de ce chemin de vie");
        sb.AppendLine("- Les défis récurrents et axes de travail");
        sb.AppendLine("- Les leviers d'action pour la transformation personnelle");
        sb.AppendLine("La synthèse doit être directement utile comme support d'accompagnement thérapeutique.");

        return sb.ToString();
    }
}
