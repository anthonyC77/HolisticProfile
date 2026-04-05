using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

/// <summary>
/// Orchestre le pipeline astrologie complet :
/// Calcul → Knowledge base → Prompt → LLM → Cache → AstroSynthesisResult.
/// </summary>
public interface IAstrologySynthesisService
{
    Task<AstroSynthesisResult> RunAsync(
        NatalChartInput   input,
        CancellationToken cancellationToken = default);
}
