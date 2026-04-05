using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

public interface IReferentielCalculationEngine
{
    ReferentielNaissanceProfile Calculate(DateTime birthDate, int currentYear);
}
