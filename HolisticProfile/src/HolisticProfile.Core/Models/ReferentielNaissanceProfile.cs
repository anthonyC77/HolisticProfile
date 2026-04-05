namespace HolisticProfile.Core.Models;

/// <summary>
/// Profil complet du Référentiel de Naissance (méthode Georges Colleuil).
/// Contient les 14 maisons calculées à partir de la date de naissance.
/// La Maison 8 est dynamique : elle dépend de l'année en cours.
/// </summary>
public record ReferentielNaissanceProfile
{
    private readonly IReadOnlyList<HouseResult> _houses;

    public DateTime BirthDate { get; }
    public int CurrentYear { get; }
    public IReadOnlyList<HouseResult> Houses => _houses;

    public HouseResult this[int houseNumber]
    {
        get
        {
            if (houseNumber < 1 || houseNumber > 14)
                throw new ArgumentOutOfRangeException(nameof(houseNumber));
            return _houses[houseNumber - 1];
        }
    }

    public ReferentielNaissanceProfile(DateTime birthDate, int currentYear, IReadOnlyList<HouseResult> houses)
    {
        if (houses.Count != 14)
            throw new ArgumentException("Le profil doit contenir exactement 14 maisons.", nameof(houses));

        BirthDate   = birthDate;
        CurrentYear = currentYear;
        _houses     = houses;
    }
}
