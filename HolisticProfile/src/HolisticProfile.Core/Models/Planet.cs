namespace HolisticProfile.Core.Models;

/// <summary>
/// Planètes calculées dans le thème natal.
/// Les valeurs entières correspondent aux constantes Swiss Ephemeris (SE_SUN=0, SE_MOON=1, etc.).
/// </summary>
public enum Planet
{
    Sun       = 0,   // Soleil
    Moon      = 1,   // Lune
    Mercury   = 2,   // Mercure
    Venus     = 3,   // Vénus
    Mars      = 4,   // Mars
    Jupiter   = 5,   // Jupiter
    Saturn    = 6,   // Saturne
    Uranus    = 7,   // Uranus
    Neptune   = 8,   // Neptune
    Pluto     = 9,   // Pluton
    NorthNode = 11,  // Nœud Nord (vrai) — SE_TRUE_NODE = 11
    Chiron    = 15,  // Chiron — SE_CHIRON = 15
}

public static class PlanetExtensions
{
    public static string ToFrench(this Planet planet) => planet switch
    {
        Planet.Sun       => "Soleil",
        Planet.Moon      => "Lune",
        Planet.Mercury   => "Mercure",
        Planet.Venus     => "Vénus",
        Planet.Mars      => "Mars",
        Planet.Jupiter   => "Jupiter",
        Planet.Saturn    => "Saturne",
        Planet.Uranus    => "Uranus",
        Planet.Neptune   => "Neptune",
        Planet.Pluto     => "Pluton",
        Planet.NorthNode => "Nœud Nord",
        Planet.Chiron    => "Chiron",
        _                => planet.ToString(),
    };

    /// <summary>Clé fichier KB en minuscules anglais (ex: "sun", "north_node").</summary>
    public static string ToFileKey(this Planet planet) => planet switch
    {
        Planet.NorthNode => "north_node",
        _                => planet.ToString().ToLowerInvariant(),
    };

    /// <summary>Planètes utilisées pour le calcul des aspects (exclut NorthNode et Chiron).</summary>
    public static readonly IReadOnlyList<Planet> AspectPlanets = new[]
    {
        Planet.Sun, Planet.Moon, Planet.Mercury, Planet.Venus,
        Planet.Mars, Planet.Jupiter, Planet.Saturn,
        Planet.Uranus, Planet.Neptune, Planet.Pluto,
    };
}
