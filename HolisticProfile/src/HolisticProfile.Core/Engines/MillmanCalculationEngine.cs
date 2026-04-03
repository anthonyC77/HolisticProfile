using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Engines;

/// <summary>
/// Moteur de calcul du chemin de vie selon la numérologie Dan Millman.
/// Somme tous les chiffres de la date DDMMYYYY, puis réduit.
/// </summary>
public class MillmanCalculationEngine : ICalculationEngine
{
    public MillmanLifePath Calculate(DateTime birthDate)
    {
        if (birthDate > DateTime.Today)
            throw new ArgumentException("La date de naissance ne peut pas être dans le futur.", nameof(birthDate));

        int sum = SumAllDigits(birthDate);

        if (sum < 10)
            return new MillmanLifePath(sum, null, sum);

        int firstReduction = SumDigits(sum);

        if (firstReduction < 10)
            return new MillmanLifePath(sum, null, firstReduction);

        // Chemin 3 parties : ex. 29 -> 11 -> 2
        int secondReduction = SumDigits(firstReduction);
        return new MillmanLifePath(sum, firstReduction, secondReduction);
    }

    private static int SumAllDigits(DateTime date)
    {
        // Formater en DDMMYYYY pour avoir exactement 8 chiffres
        var digits = date.ToString("ddMMyyyy");
        return digits.Sum(c => c - '0');
    }

    private static int SumDigits(int number)
    {
        return number.ToString().Sum(c => c - '0');
    }
}
