using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Engines;

/// <summary>
/// Moteur de calcul du Référentiel de Naissance selon la méthode Georges Colleuil.
/// Calcule les 14 maisons (arcanes) à partir de la date de naissance et de l'année en cours.
///
/// Règles de réduction :
///   Base 22 (maisons 1–13) : réduit jusqu'à obtenir 1–22 ; 0 → 22.
///   Base 9 (année universelle) : réduit jusqu'à 1–9 ; 11 et 22 sont des maîtres nombres conservés.
///   Soustraction négative : ajoute 22 si résultat ≤ 0.
///
/// ⚠️ Formule M7 = M3 - M2 (source : synthèse Colleuil).
///    Variante existante : M7 = M1 - M3. À valider avec le livre officiel si les résultats divergent.
/// </summary>
public class ReferentielCalculationEngine : IReferentielCalculationEngine
{
    public ReferentielNaissanceProfile Calculate(DateTime birthDate, int currentYear)
    {
        if (birthDate > DateTime.Today)
            throw new ArgumentException("La date de naissance ne peut pas être dans le futur.", nameof(birthDate));

        int day   = birthDate.Day;
        int month = birthDate.Month;
        int year  = birthDate.Year;

        // --- Maisons de base ---
        int m1 = ReduceBase22(day);
        int m2 = month; // toujours 1–12, jamais réduit
        int m3 = ReduceBase22(SumDigits(year));

        // --- Maisons dérivées ---
        int m4 = ReduceBase22(m1 + m2 + m3);

        int m5Raw = m1 + m2 + m3 + m4;
        int m5    = ReduceBase22(m5Raw);

        int m6 = ReduceBase22(m1 + m2);

        int m7 = m3 - m2;
        if (m7 <= 0) m7 += 22;

        // --- Maison dynamique ---
        int anneeUniverselle = ReduceBase9WithMasterNumbers(SumDigits(currentYear));
        int m8 = ReduceBase22(m6 + anneeUniverselle);

        // --- Maisons de profondeur ---
        int m9  = ReduceBase22(m6 + m7);
        int m10 = 22 - m9;
        if (m10 == 0) m10 = 22;

        int m11 = ReduceBase22(m7 + m3 + m10);
        int m12 = ReduceBase22(m6 + m2 + m4);
        // M5 comptée deux fois intentionnellement (Colleuil)
        int m13 = ReduceBase22(m12 + m1 + m5 + m3 + m11 + m4 + m5 + m2 + m9);

        // --- Maison 14 : Arcane Mineur ---
        int m14Numero = ConvertToMinorArcaneNumber(m5Raw);

        var houses = new List<HouseResult>
        {
            new(1,  new MajorArcane(m1)),
            new(2,  new MajorArcane(m2)),
            new(3,  new MajorArcane(m3)),
            new(4,  new MajorArcane(m4)),
            new(5,  new MajorArcane(m5)),
            new(6,  new MajorArcane(m6)),
            new(7,  new MajorArcane(m7)),
            new(8,  new MajorArcane(m8)),
            new(9,  new MajorArcane(m9)),
            new(10, new MajorArcane(m10)),
            new(11, new MajorArcane(m11)),
            new(12, new MajorArcane(m12)),
            new(13, new MajorArcane(m13)),
            new(14, new MinorArcane(m14Numero)),
        };

        return new ReferentielNaissanceProfile(birthDate, currentYear, houses);
    }

    /// <summary>
    /// Réduit n en base 22 : additionne les chiffres tant que n > 22.
    /// Cas limite : n = 0 → 22 (Le Mat).
    /// </summary>
    public static int ReduceBase22(int n)
    {
        while (n > 22)
            n = SumDigits(n);

        return n == 0 ? 22 : n;
    }

    /// <summary>
    /// Réduit n en base 9 en préservant les maîtres nombres 11 et 22.
    /// </summary>
    public static int ReduceBase9WithMasterNumbers(int n)
    {
        if (n == 11 || n == 22) return n;

        while (n > 9)
        {
            n = SumDigits(n);
            if (n == 11 || n == 22) return n;
        }

        return n == 0 ? 9 : n;
    }

    /// <summary>
    /// Convertit le nombre brut M5 (avant réduction) en numéro d'Arcane Mineur (1–56).
    /// Réduit par somme de chiffres si > 56 ; 0 → 56.
    /// </summary>
    public static int ConvertToMinorArcaneNumber(int n)
    {
        while (n > 56)
            n = SumDigits(n);

        return n == 0 ? 56 : n;
    }

    public static int SumDigits(int n)
        => Math.Abs(n).ToString().Sum(c => c - '0');
}
