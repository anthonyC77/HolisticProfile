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

        sb.AppendLine("## Notation Millman — à lire attentivement");
        sb.AppendLine("Dans le système Dan Millman, la notation A/B signifie :");
        sb.AppendLine("  - A = somme brute de tous les chiffres de la date de naissance");
        sb.AppendLine("  - B = racine obtenue par réduction de A (ex: 16 → 1+6 = 7)");
        sb.AppendLine("IMPORTANT : ne jamais inverser cette notation. 16/7 n'est pas 7/16.");
        sb.AppendLine();

        sb.AppendLine("## Profil calculé");
        sb.AppendLine($"Date de naissance : {profile.BirthDate:dd/MM/yyyy}");

        var lp = profile.MillmanLifePath;
        if (lp.IntermediateSum.HasValue)
        {
            sb.AppendLine($"Chemin de vie Millman : {lp} " +
                          $"(somme={lp.Sum}, intermédiaire={lp.IntermediateSum}, racine={lp.Root})");
            sb.AppendLine($"Chemin à 3 niveaux : chaque niveau porte un sens propre. " +
                          $"Le chiffre {lp.IntermediateSum % 10} est présent deux fois → double poids, double défi.");
        }
        else
        {
            sb.AppendLine($"Chemin de vie Millman : {lp} (somme={lp.Sum}, racine={lp.Root})");
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
