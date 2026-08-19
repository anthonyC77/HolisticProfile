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

    /// <summary>
    /// Clé unique pour le cache — inclut heure, lieu et décalage UTC.
    /// Le décalage en fait partie : un même lieu à une même heure donne un thème
    /// différent selon le fuseau retenu (heure d'été, correction de saisie).
    /// </summary>
    public string CacheKey
    {
        get
        {
            var dt  = BirthDateTimeLocal.ToString("dd_MM_yyyy_HHmm");
            var lat = Latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture).Replace('.', 'p');
            var lon = Longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture).Replace('.', 'p');

            var abs  = UtcOffset.Duration();
            var sign = UtcOffset < TimeSpan.Zero ? "m" : "p";
            var off  = abs.Seconds == 0
                ? $"utc{sign}{abs.Hours:D2}{abs.Minutes:D2}"
                : $"utc{sign}{abs.Hours:D2}{abs.Minutes:D2}{abs.Seconds:D2}";

            return $"{dt}_{lat}_{lon}_{off}";
        }
    }

    public override string ToString()
        => $"{BirthDateTimeLocal:dd/MM/yyyy HH:mm} UTC{(UtcOffset >= TimeSpan.Zero ? "+" : "")}{UtcOffset:hh\\:mm} — {PlaceName} ({Latitude:F2}°, {Longitude:F2}°)";
}
