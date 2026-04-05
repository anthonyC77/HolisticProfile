using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

/// <summary>
/// Charge le contenu de la base de connaissances astrologique
/// (planètes en signes, planètes en maisons, aspects).
/// </summary>
public interface IAstrologyKnowledgeBaseRepository
{
    /// <summary>
    /// Charge et concatène les fiches pertinentes pour le thème natal fourni.
    /// Les fichiers absents sont silencieusement ignorés.
    /// </summary>
    Task<string> LoadProfileContentAsync(NatalChartProfile profile);
}
