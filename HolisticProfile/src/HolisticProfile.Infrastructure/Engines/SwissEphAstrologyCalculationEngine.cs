using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using SwissEphNet;

namespace HolisticProfile.Infrastructure.Engines;

/// <summary>
/// Moteur de calcul astrologique basé sur Swiss Ephemeris (via SwissEphNet).
///
/// Mode éphémérides :
///   • Si <see cref="EphemerisPath"/> est renseigné et que les fichiers .se1 sont présents →
///     calculs via Swiss Ephemeris (haute précision, ±1″).
///   • Sinon → Moshier (algorithmique, aucun fichier nécessaire, précision ~1′ suffisante
///     pour l'usage thérapeutique).
///
/// Système de maisons : Placidus (hsys = 'P').
/// Rétrogradation : vitesse en longitude &lt; 0.
/// </summary>
public class SwissEphAstrologyCalculationEngine : IAstrologyCalculationEngine, IDisposable
{
    private readonly SwissEph _swe;
    private bool _disposed;

    /// <summary>
    /// Crée le moteur avec le chemin optionnel vers les fichiers éphémérides Swiss Ephemeris (.se1).
    /// Passer null ou chaîne vide pour utiliser Moshier (sans fichiers).
    /// </summary>
    public SwissEphAstrologyCalculationEngine(string? ephemerisPath = null)
    {
        _swe = new SwissEph();

        if (!string.IsNullOrWhiteSpace(ephemerisPath) && Directory.Exists(ephemerisPath))
            _swe.swe_set_ephe_path(ephemerisPath);
        else
            _swe.swe_set_ephe_path(null); // Moshier
    }

    public NatalChartProfile Calculate(NatalChartInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var ut      = input.BirthDateTimeUT;
        double julDay = _swe.swe_julday(
            ut.Year, ut.Month, ut.Day,
            ut.Hour + ut.Minute / 60.0 + ut.Second / 3600.0,
            SwissEph.SE_GREG_CAL);

        var planets = CalculatePlanets(julDay, input.Latitude, input.Longitude, out var cusps);
        var houses  = BuildHousePositions(cusps);
        var aspects = CalculateAspects(planets);

        return new NatalChartProfile(input, planets, houses, aspects);
    }

    // ─── Planètes ─────────────────────────────────────────────────────────────

    private List<PlanetPosition> CalculatePlanets(
        double julDay, double lat, double lon,
        out double[] cusps)
    {
        // Calcul des maisons (nécessaire pour assigner chaque planète à sa maison)
        cusps = new double[13];
        var ascmc = new double[10];
        _swe.swe_houses(julDay, lat, lon, 'P', cusps, ascmc);

        int flags = SwissEph.SEFLG_MOSEPH | SwissEph.SEFLG_SPEED;

        var positions = new List<PlanetPosition>();

        foreach (Planet planet in Enum.GetValues<Planet>())
        {
            var xx   = new double[6];
            string serr = string.Empty;
            int    ipl  = (int)planet;

            int ret = _swe.swe_calc_ut(julDay, ipl, flags, xx, ref serr);
            if (ret < 0) continue; // planète non calculable, on saute silencieusement

            double longitude  = xx[0];
            bool   retrograde = xx[3] < 0;

            var sign         = ZodiacSignExtensions.FromLongitude(longitude);
            var degreeInSign = ZodiacSignExtensions.DegreeInSign(longitude);
            int house        = GetHouseNumber(longitude, cusps);

            positions.Add(new PlanetPosition(planet, longitude, sign, degreeInSign, house, retrograde));
        }

        return positions;
    }

    // ─── Maisons ──────────────────────────────────────────────────────────────

    private static List<HousePosition> BuildHousePositions(double[] cusps)
    {
        // cusps[1..12] — cusps[0] est inutilisé par swe_houses
        var houses = new List<HousePosition>(12);
        for (int h = 1; h <= 12; h++)
        {
            double lon        = cusps[h];
            var    sign       = ZodiacSignExtensions.FromLongitude(lon);
            double degInSign  = ZodiacSignExtensions.DegreeInSign(lon);
            houses.Add(new HousePosition(h, lon, sign, degInSign));
        }
        return houses;
    }

    // ─── Aspects ──────────────────────────────────────────────────────────────

    private static List<AstroAspect> CalculateAspects(List<PlanetPosition> planets)
    {
        var aspects = new List<AstroAspect>();

        var planetsForAspects = planets
            .Where(p => PlanetExtensions.AspectPlanets.Contains(p.Planet))
            .ToList();

        for (int i = 0; i < planetsForAspects.Count - 1; i++)
        for (int j = i + 1; j < planetsForAspects.Count; j++)
        {
            var p1 = planetsForAspects[i];
            var p2 = planetsForAspects[j];

            double diff = AngleDifference(p1.Longitude, p2.Longitude);

            foreach (AspectType type in Enum.GetValues<AspectType>())
            {
                double exact = type.ExactAngle();
                double orb   = Math.Abs(diff - exact);

                if (orb <= type.MaxOrb())
                {
                    aspects.Add(new AstroAspect(p1.Planet, p2.Planet, type, orb));
                    break; // une seule catégorie par paire
                }
            }
        }

        // Trier par orbe croissant (les aspects les plus exacts en tête)
        aspects.Sort((a, b) => a.Orb.CompareTo(b.Orb));

        return aspects;
    }

    // ─── Helpers géométriques ─────────────────────────────────────────────────

    /// <summary>
    /// Détermine le numéro de maison (1–12) d'une longitude écliptique.
    /// Gère le cas où une maison enjambe 0°/360°.
    /// </summary>
    public static int GetHouseNumber(double planetLon, double[] cusps)
    {
        planetLon = Normalize(planetLon);

        for (int h = 1; h <= 11; h++)
        {
            double start = Normalize(cusps[h]);
            double end   = Normalize(cusps[h + 1]);

            if (start <= end)
            {
                if (planetLon >= start && planetLon < end) return h;
            }
            else // maison qui enjambe 0°
            {
                if (planetLon >= start || planetLon < end) return h;
            }
        }
        return 12;
    }

    /// <summary>Différence angulaire absolue minimale entre deux longitudes (0–180°).</summary>
    public static double AngleDifference(double lon1, double lon2)
    {
        double diff = Math.Abs(Normalize(lon1) - Normalize(lon2));
        return diff > 180 ? 360 - diff : diff;
    }

    private static double Normalize(double lon)
        => ((lon % 360) + 360) % 360;

    // ─── IDisposable ──────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (!_disposed)
        {
            _swe.swe_close();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
