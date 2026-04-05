using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

/// <summary>
/// Cache fichier des synthèses astrologiques générées par le LLM.
/// </summary>
public interface IAstrologySynthesisCacheRepository
{
    Task<string?> LoadAsync(NatalChartInput input);
    Task SaveAsync(NatalChartInput input, string synthesisText);
}
