namespace HolisticProfile.Core.Models;

/// <summary>
/// Lieu de naissance issu de la table locale des villes.
/// Le fuseau est stocké au format IANA (ex: "Europe/Paris") pour permettre
/// la résolution historique du décalage UTC (heure d'été, changements de fuseau).
/// </summary>
/// <param name="Name">Nom de la ville (ex: "Orléans").</param>
/// <param name="Country">Pays (ex: "France").</param>
/// <param name="Latitude">Latitude en degrés décimaux (+N / −S).</param>
/// <param name="Longitude">Longitude en degrés décimaux (+E / −W).</param>
/// <param name="TimeZoneId">Identifiant IANA du fuseau (ex: "Europe/Paris").</param>
/// <param name="Region">Région / département / état, facultatif — sert à distinguer les homonymes.</param>
public record Place(
    string Name,
    string Country,
    double Latitude,
    double Longitude,
    string TimeZoneId,
    string? Region = null)
{
    /// <summary>Libellé affichable : "Orléans (Loiret, France)".</summary>
    public string DisplayName
        => string.IsNullOrWhiteSpace(Region)
            ? $"{Name} ({Country})"
            : $"{Name} ({Region}, {Country})";

    public override string ToString()
        => $"{DisplayName} — {Latitude:F4}°, {Longitude:F4}° — {TimeZoneId}";
}
