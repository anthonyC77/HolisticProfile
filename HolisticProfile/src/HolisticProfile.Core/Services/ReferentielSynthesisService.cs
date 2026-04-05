using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Services;

/// <summary>
/// Orchestrateur du pipeline Référentiel de Naissance.
/// Cache → [Calcul → Knowledge → Prompt → LLM → Sauvegarde cache] → ReferentielSynthesisResult
/// </summary>
public class ReferentielSynthesisService : IReferentielSynthesisService
{
    private readonly IReferentielCalculationEngine       _engine;
    private readonly IReferentielKnowledgeBaseRepository _knowledgeRepo;
    private readonly IReferentielPromptBuilder           _promptBuilder;
    private readonly ILlmClient                         _llmClient;
    private readonly IReferentielSynthesisCacheRepository _cache;

    public ReferentielSynthesisService(
        IReferentielCalculationEngine        engine,
        IReferentielKnowledgeBaseRepository  knowledgeRepo,
        IReferentielPromptBuilder            promptBuilder,
        ILlmClient                           llmClient,
        IReferentielSynthesisCacheRepository cache)
    {
        _engine        = engine;
        _knowledgeRepo = knowledgeRepo;
        _promptBuilder = promptBuilder;
        _llmClient     = llmClient;
        _cache         = cache;
    }

    public async Task<ReferentielSynthesisResult> RunAsync(
        DateTime birthDate,
        CancellationToken cancellationToken = default)
    {
        var currentYear = DateTime.Today.Year;
        var profile     = _engine.Calculate(birthDate, currentYear);

        var cached = await _cache.LoadAsync(profile);
        if (cached is not null)
            return new ReferentielSynthesisResult(profile, cached);

        // Maisons clés pour la synthèse thérapeutique — évite un prompt trop lourd pour le LLM local
        // M4=mission, M5=passage obligé, M6=ressources, M8=année, M9=soi profond, M10=ombre, M14=ressource universelle
        var keyHouses = new[] { 4, 5, 6, 8, 9, 10, 14 };
        var knowledge = await _knowledgeRepo.LoadKeyHousesContentAsync(profile, keyHouses);
        var prompt    = _promptBuilder.Build(profile, knowledge);
        var text      = await _llmClient.GenerateAsync(prompt, cancellationToken);

        await _cache.SaveAsync(profile, text);

        return new ReferentielSynthesisResult(profile, text);
    }
}
