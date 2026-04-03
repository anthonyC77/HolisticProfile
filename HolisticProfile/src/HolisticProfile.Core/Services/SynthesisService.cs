using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Services;

/// <summary>
/// Orchestrateur (Facade) : coordonne le pipeline complet.
/// Cache → [Calcul → Knowledge → Prompt → LLM → Sauvegarde cache] → SynthesisResult
/// </summary>
public class SynthesisService : ISynthesisService
{
    private readonly ICalculationEngine _engine;
    private readonly IKnowledgeBaseRepository _knowledgeRepo;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILlmClient _llmClient;
    private readonly ISynthesisCacheRepository _cache;

    public SynthesisService(
        ICalculationEngine engine,
        IKnowledgeBaseRepository knowledgeRepo,
        IPromptBuilder promptBuilder,
        ILlmClient llmClient,
        ISynthesisCacheRepository cache)
    {
        _engine        = engine;
        _knowledgeRepo = knowledgeRepo;
        _promptBuilder = promptBuilder;
        _llmClient     = llmClient;
        _cache         = cache;
    }

    public async Task<SynthesisResult> RunAsync(DateTime birthDate, CancellationToken cancellationToken = default)
    {
        var lifePath = _engine.Calculate(birthDate);
        var profile  = new BirthProfile(birthDate, lifePath);

        var cached = await _cache.LoadAsync(profile);
        if (cached is not null)
            return new SynthesisResult(profile, cached);

        var knowledge = await _knowledgeRepo.LoadAsync(lifePath.PathKey);
        var prompt    = _promptBuilder.Build(profile, knowledge);
        var text      = await _llmClient.GenerateAsync(prompt, cancellationToken);

        await _cache.SaveAsync(profile, text);

        return new SynthesisResult(profile, text);
    }
}
