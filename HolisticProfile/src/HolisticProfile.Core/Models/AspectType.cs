namespace HolisticProfile.Core.Models;

/// <summary>
/// Aspects géométriques majeurs utilisés en astrologie occidentale.
/// </summary>
public enum AspectType
{
    Conjunction  = 0,   // Conjonction  — 0°
    Opposition   = 1,   // Opposition   — 180°
    Trine        = 2,   // Trigone      — 120°
    Square       = 3,   // Carré        — 90°
    Sextile      = 4,   // Sextile      — 60°
    Quincunx     = 5,   // Quinconce    — 150°
}

public static class AspectTypeExtensions
{
    public static string ToFrench(this AspectType type) => type switch
    {
        AspectType.Conjunction => "Conjonction",
        AspectType.Opposition  => "Opposition",
        AspectType.Trine       => "Trigone",
        AspectType.Square      => "Carré",
        AspectType.Sextile     => "Sextile",
        AspectType.Quincunx    => "Quinconce",
        _                      => type.ToString(),
    };

    public static string Symbol(this AspectType type) => type switch
    {
        AspectType.Conjunction => "☌",
        AspectType.Opposition  => "☍",
        AspectType.Trine       => "△",
        AspectType.Square      => "□",
        AspectType.Sextile     => "⚹",
        AspectType.Quincunx    => "⚻",
        _                      => "?",
    };

    /// <summary>Angle exact de l'aspect (en degrés).</summary>
    public static double ExactAngle(this AspectType type) => type switch
    {
        AspectType.Conjunction => 0,
        AspectType.Opposition  => 180,
        AspectType.Trine       => 120,
        AspectType.Square      => 90,
        AspectType.Sextile     => 60,
        AspectType.Quincunx    => 150,
        _                      => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>Orbe maximum toléré pour cet aspect (en degrés).</summary>
    public static double MaxOrb(this AspectType type) => type switch
    {
        AspectType.Conjunction => 8,
        AspectType.Opposition  => 8,
        AspectType.Trine       => 8,
        AspectType.Square      => 8,
        AspectType.Sextile     => 6,
        AspectType.Quincunx    => 3,
        _                      => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>Clé fichier KB en minuscules anglais (ex: "trine").</summary>
    public static string ToFileKey(this AspectType type) => type.ToString().ToLowerInvariant();
}
