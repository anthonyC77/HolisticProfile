namespace HolisticProfile.Core.Models;

/// <summary>
/// Données d'entrée nécessaires au calcul d'un thème natal.
/// </summary>
/// <param name="BirthDateTimeLocal">Date et heure locale de naissance.</param>
/// <param name="UtcOffset">Décalage UTC (ex : TimeSpan.FromHours(1) pour Paris en hiver).</param>
/// <param name="Latitude">Latitude du lieu de naissance en degrés décimaux (+N / −S).</param>
/// <param name="Longitude">Longitude du lieu de naissance en degrés décimaux (+E / −W).</param>
/// <param name="PlaceName">Nom du lieu (affiché, non utilisé dans le calcul).</param>
public record NatalChartInput(
    DateTime   BirthDateTimeLocal,
    TimeSpan   UtcOffset,
    double     Latitude,
    double     Longitude,
    string     PlaceName = "")
{
    /// <summary>Date/heure en Temps Universel (UT).</summary>
    public DateTime BirthDateTimeUT => BirthDateTimeLocal - UtcOffset;

    /// <summary>Clé unique pour le cache — inclut heure et lieu.</summary>
    public string CacheKey
    {
        get
        {
            var dt  = BirthDateTimeLocal.ToString("dd_MM_yyyy_HHmm");
            var lat = Latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture).Replace('.', 'p');
            var lon = Longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture).Replace('.', 'p');
            return $"{dt}_{lat}_{lon}";
        }
    }

    public override string ToString()
        => $"{BirthDateTimeLocal:dd/MM/yyyy HH:mm} UTC{(UtcOffset >= TimeSpan.Zero ? "+" : "")}{UtcOffset:hh\\:mm} — {PlaceName} ({Latitude:F2}°, {Longitude:F2}°)";
}
