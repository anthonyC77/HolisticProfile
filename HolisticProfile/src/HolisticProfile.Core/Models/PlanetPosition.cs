namespace HolisticProfile.Core.Models;

/// <summary>
/// Position d'une planète dans le thème natal.
/// </summary>
/// <param name="Planet">Planète concernée.</param>
/// <param name="Longitude">Longitude écliptique (0–360°).</param>
/// <param name="Sign">Signe zodiacal.</param>
/// <param name="DegreeInSign">Degré dans le signe (0–29.999°).</param>
/// <param name="House">Numéro de maison astrologique (1–12).</param>
/// <param name="IsRetrograde">Vrai si la planète est en mouvement rétrograde.</param>
public record PlanetPosition(
    Planet      Planet,
    double      Longitude,
    ZodiacSign  Sign,
    double      DegreeInSign,
    int         House,
    bool        IsRetrograde)
{
    public override string ToString()
    {
        var retro = IsRetrograde ? " ℞" : "";
        return $"{Planet.ToFrench()} {DegreeInSign:F1}° {Sign.ToFrench()} (Maison {House}){retro}";
    }
}
