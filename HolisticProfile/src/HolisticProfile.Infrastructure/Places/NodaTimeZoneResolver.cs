using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using NodaTime;

namespace HolisticProfile.Infrastructure.Places;

/// <summary>
/// Résout le décalage UTC d'une heure locale de naissance à partir de la base TZDB
/// embarquée dans NodaTime (règles historiques complètes : heure d'été depuis 1916,
/// occupation 1940-1945, réintroduction de 1976, changements de méridien de référence…).
///
/// Windows ne conserve pas ces règles anciennes : <see cref="TimeZoneInfo"/> donnerait
/// un décalage faux pour la plupart des naissances antérieures aux années 2000.
/// </summary>
public class NodaTimeZoneResolver : IBirthTimeZoneResolver
{
    private readonly IDateTimeZoneProvider _provider;

    public NodaTimeZoneResolver(IDateTimeZoneProvider? provider = null)
    {
        _provider = provider ?? DateTimeZoneProviders.Tzdb;
    }

    public BirthTimeResolution Resolve(DateTime localDateTime, string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new ArgumentException("Identifiant de fuseau vide.", nameof(timeZoneId));

        var zone = _provider.GetZoneOrNull(timeZoneId)
            ?? throw new ArgumentException($"Fuseau horaire inconnu : '{timeZoneId}'", nameof(timeZoneId));

        var local   = LocalDateTime.FromDateTime(localDateTime);
        var mapping = zone.MapLocal(local);

        return mapping.Count switch
        {
            // Cas normal
            1 => new BirthTimeResolution(mapping.Single().Offset.ToTimeSpan()),

            // Heure vécue deux fois (retour à l'heure d'hiver) :
            // on retient la première occurrence, l'autre reste proposée au praticien.
            2 => new BirthTimeResolution(
                     mapping.First().Offset.ToTimeSpan(),
                     BirthTimeKind.Ambiguous,
                     mapping.Last().Offset.ToTimeSpan()),

            // Heure inexistante (passage à l'heure d'été) : le décalage d'avant la
            // transition décale l'instant vers l'avant, comme le ferait une horloge.
            _ => new BirthTimeResolution(
                     mapping.EarlyInterval.WallOffset.ToTimeSpan(),
                     BirthTimeKind.Skipped),
        };
    }
}
