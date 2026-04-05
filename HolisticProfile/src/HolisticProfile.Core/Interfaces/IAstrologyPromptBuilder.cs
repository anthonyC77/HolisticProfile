using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

/// <summary>
/// Construit le prompt LLM pour la synthèse astrologique.
/// </summary>
public interface IAstrologyPromptBuilder
{
    string Build(NatalChartProfile profile, string knowledgeContent);
}
