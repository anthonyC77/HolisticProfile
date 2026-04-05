namespace HolisticProfile.Core.Models;

/// <summary>
/// Value object représentant un Arcane Majeur du Tarot de Marseille.
/// Numéroté de 1 (Le Bateleur) à 22 (Le Mat) dans le système Colleuil.
/// Le Mat = 22 (et non 0 comme dans d'autres systèmes).
/// </summary>
public record MajorArcane
{
    private static readonly string[] Names =
    [
        "",                   // index 0 non utilisé
        "Le Bateleur",        // 1
        "La Papesse",         // 2
        "L'Impératrice",      // 3
        "L'Empereur",         // 4
        "Le Pape",            // 5
        "L'Amoureux",         // 6
        "Le Chariot",         // 7
        "La Justice",         // 8
        "L'Hermite",          // 9
        "La Roue de Fortune", // 10
        "La Force",           // 11
        "Le Pendu",           // 12
        "L'Arcane Sans Nom",  // 13
        "Tempérance",         // 14
        "Le Diable",          // 15
        "La Maison Dieu",     // 16
        "L'Étoile",           // 17
        "La Lune",            // 18
        "Le Soleil",          // 19
        "Le Jugement",        // 20
        "Le Monde",           // 21
        "Le Mat",             // 22
    ];

    public int Numero { get; }
    public string Nom => Names[Numero];

    public MajorArcane(int numero)
    {
        if (numero < 1 || numero > 22)
            throw new ArgumentOutOfRangeException(nameof(numero), $"Un arcane majeur doit être entre 1 et 22, reçu : {numero}");
        Numero = numero;
    }

    public override string ToString() => $"{Numero} — {Nom}";
}
