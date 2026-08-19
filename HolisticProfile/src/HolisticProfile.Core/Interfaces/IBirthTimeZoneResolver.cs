using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

/// <summary>
/// Convertit une heure locale de naissance en décalage UTC, en appliquant
/// les règles historiques du fuseau (heure d'été, changements de méridien de référence).
/// </summary>
public interface IBirthTimeZoneResolver
{
    /// <summary>
    /// Résout le décalage UTC applicable à <paramref name="localDateTime"/> dans le fuseau
    /// <paramref name="timeZoneId"/> (identifiant IANA, ex: "Europe/Paris").
    /// </summary>
    /// <exception cref="ArgumentException">Fuseau inconnu.</exception>
    BirthTimeResolution Resolve(DateTime localDateTime, string timeZoneId);
}
