namespace HolisticProfile.Core.Models;

/// <summary>
/// Les 12 signes du zodiaque (ordre héliocentrique, Bélier = 0).
/// </summary>
public enum ZodiacSign
{
    Aries        = 0,   // Bélier
    Taurus       = 1,   // Taureau
    Gemini       = 2,   // Gémeaux
    Cancer       = 3,   // Cancer
    Leo          = 4,   // Lion
    Virgo        = 5,   // Vierge
    Libra        = 6,   // Balance
    Scorpio      = 7,   // Scorpion
    Sagittarius  = 8,   // Sagittaire
    Capricorn    = 9,   // Capricorne
    Aquarius     = 10,  // Verseau
    Pisces       = 11,  // Poissons
}

public static class ZodiacSignExtensions
{
    public static string ToFrench(this ZodiacSign sign) => sign switch
    {
        ZodiacSign.Aries        => "Bélier",
        ZodiacSign.Taurus       => "Taureau",
        ZodiacSign.Gemini       => "Gémeaux",
        ZodiacSign.Cancer       => "Cancer",
        ZodiacSign.Leo          => "Lion",
        ZodiacSign.Virgo        => "Vierge",
        ZodiacSign.Libra        => "Balance",
        ZodiacSign.Scorpio      => "Scorpion",
        ZodiacSign.Sagittarius  => "Sagittaire",
        ZodiacSign.Capricorn    => "Capricorne",
        ZodiacSign.Aquarius     => "Verseau",
        ZodiacSign.Pisces       => "Poissons",
        _                       => sign.ToString(),
    };

    /// <summary>Clé fichier KB en minuscules anglais (ex: "aries").</summary>
    public static string ToFileKey(this ZodiacSign sign) => sign.ToString().ToLowerInvariant();

    /// <summary>Calcule le signe depuis une longitude écliptique (0–360°).</summary>
    public static ZodiacSign FromLongitude(double longitude)
    {
        longitude = ((longitude % 360) + 360) % 360;
        return (ZodiacSign)((int)(longitude / 30));
    }

    /// <summary>Degré dans le signe (0–29.999°).</summary>
    public static double DegreeInSign(double longitude)
    {
        longitude = ((longitude % 360) + 360) % 360;
        return longitude % 30;
    }
}
