using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

/// <summary>
/// Calcule un thème natal (positions planétaires, maisons, aspects)
/// à partir des données de naissance.
/// </summary>
public interface IAstrologyCalculationEngine
{
    /// <summary>
    /// Calcule le thème natal complet.
    /// </summary>
    /// <param name="input">Données de naissance (date, heure, lieu).</param>
    /// <returns>Thème natal calculé.</returns>
    NatalChartProfile Calculate(NatalChartInput input);
}
