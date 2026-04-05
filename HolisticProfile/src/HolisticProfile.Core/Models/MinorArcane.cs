namespace HolisticProfile.Core.Models;

/// <summary>
/// Value object représentant un Arcane Mineur du Tarot de Marseille.
/// Numéroté de 1 à 56 (4 familles × 14 cartes).
/// Utilisé uniquement pour la Maison 14 du Référentiel de Naissance.
///
/// Table de correspondance :
///   Bâtons  :  1–14  (Roi=1, Reine=2, Cavalier=3, Valet=4, As=5, 2=6 … 10=14)
///   Coupes  : 15–28
///   Épées   : 29–42
///   Deniers : 43–56
/// </summary>
public record MinorArcane
{
    private static readonly string[] Familles = ["Bâtons", "Coupes", "Épées", "Deniers"];

    public int Numero { get; }
    public string Famille { get; }
    public string Carte { get; }

    public MinorArcane(int numero)
    {
        if (numero < 1 || numero > 56)
            throw new ArgumentOutOfRangeException(nameof(numero), $"Un arcane mineur doit être entre 1 et 56, reçu : {numero}");

        Numero = numero;
        Famille = Familles[(numero - 1) / 14];

        var rang = ((numero - 1) % 14) + 1;
        Carte = rang switch
        {
            1 => "Roi",
            2 => "Reine",
            3 => "Cavalier",
            4 => "Valet",
            5 => "As",
            _ => (rang - 4).ToString() // 6→2, 7→3 … 14→10
        };
    }

    public override string ToString() => $"{Carte} de {Famille} (n°{Numero})";
}
