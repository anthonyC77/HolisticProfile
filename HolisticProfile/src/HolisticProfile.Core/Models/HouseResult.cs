namespace HolisticProfile.Core.Models;

/// <summary>
/// Résultat d'une maison du Référentiel de Naissance.
/// M1–M13 portent un Arcane Majeur. M14 porte un Arcane Mineur.
/// </summary>
public record HouseResult
{
    private static readonly string[] HouseNames =
    [
        "",
        "Personnalité",          // 1
        "Quête / Idéal",         // 2
        "Pensées / Désirs",      // 3
        "Action / Mission",      // 4
        "Passage Obligé",        // 5
        "Ressources / Talents",  // 6
        "Défis / Obstacles",     // 7
        "Année en cours",        // 8
        "Soi Profond",           // 9
        "Échecs / Ombre",        // 10
        "Mémoire / Héritage",    // 11
        "Guérison / Idéal",      // 12
        "Paradoxes",             // 13
        "Ressource Universelle", // 14
    ];

    public int HouseNumber { get; }
    public string HouseName => HouseNames[HouseNumber];
    public MajorArcane? Major { get; }
    public MinorArcane? Minor { get; }

    /// <summary>Maisons 1–13 : Arcane Majeur.</summary>
    public HouseResult(int houseNumber, MajorArcane major)
    {
        ValidateHouseNumber(houseNumber);
        if (houseNumber == 14)
            throw new ArgumentException("La Maison 14 requiert un Arcane Mineur.", nameof(houseNumber));
        HouseNumber = houseNumber;
        Major = major;
    }

    /// <summary>Maison 14 : Arcane Mineur.</summary>
    public HouseResult(int houseNumber, MinorArcane minor)
    {
        ValidateHouseNumber(houseNumber);
        if (houseNumber != 14)
            throw new ArgumentException("Seule la Maison 14 porte un Arcane Mineur.", nameof(houseNumber));
        HouseNumber = houseNumber;
        Minor = minor;
    }

    public override string ToString() => Major is not null
        ? $"M{HouseNumber} {HouseName} : {Major}"
        : $"M{HouseNumber} {HouseName} : {Minor}";

    private static void ValidateHouseNumber(int n)
    {
        if (n < 1 || n > 14)
            throw new ArgumentOutOfRangeException(nameof(n), $"Le numéro de maison doit être entre 1 et 14, reçu : {n}");
    }
}
