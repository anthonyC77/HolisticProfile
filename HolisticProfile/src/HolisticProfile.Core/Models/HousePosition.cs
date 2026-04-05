namespace HolisticProfile.Core.Models;

/// <summary>
/// Cuspide d'une maison astrologique.
/// </summary>
/// <param name="Number">Numéro de la maison (1–12).</param>
/// <param name="Longitude">Longitude écliptique de la cuspide (0–360°).</param>
/// <param name="Sign">Signe zodiacal de la cuspide.</param>
/// <param name="DegreeInSign">Degré dans le signe (0–29.999°).</param>
public record HousePosition(
    int        Number,
    double     Longitude,
    ZodiacSign Sign,
    double     DegreeInSign)
{
    public override string ToString()
        => $"Maison {Number,2} — {DegreeInSign:F1}° {Sign.ToFrench()}";
}
