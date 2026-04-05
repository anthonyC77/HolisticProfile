namespace HolisticProfile.Core.Models;

/// <summary>
/// Résultat final du pipeline astrologie : profil calculé + synthèse LLM.
/// </summary>
public record AstroSynthesisResult(NatalChartProfile Profile, string Text);
