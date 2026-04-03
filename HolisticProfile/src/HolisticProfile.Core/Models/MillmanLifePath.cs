namespace HolisticProfile.Core.Models;

/// <summary>
/// Value object représentant un chemin de vie Millman.
/// Chemin 2 parties : 34/7 (somme réduite en 1 étape)
/// Chemin 3 parties : 29/11/2 (somme -> intermédiaire >= 10 -> racine)
///   Millman considère TOUS les chiffres : 2, 9, 1, 1, 2
///   Le chiffre doublé (ici 1) porte un double poids / double défi.
/// </summary>
public record MillmanLifePath
{
    public int Sum { get; }
    public int? IntermediateSum { get; }
    public int Root { get; }

    public MillmanLifePath(int sum, int? intermediateSum, int root)
    {
        Sum = sum;
        IntermediateSum = intermediateSum;
        Root = root;
    }

    /// <summary>
    /// Clé filesystem pour la base de connaissances.
    /// 34/7   -> "34_7"
    /// 29/11/2 -> "29_11_2"  (Millman lit les 3 niveaux)
    /// </summary>
    public string PathKey => IntermediateSum.HasValue
        ? $"{Sum}_{IntermediateSum}_{Root}"
        : $"{Sum}_{Root}";

    public override string ToString() => IntermediateSum.HasValue
        ? $"{Sum}/{IntermediateSum}/{Root}"
        : $"{Sum}/{Root}";
}
