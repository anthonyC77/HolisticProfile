using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Services;

/// <summary>
/// Orchestrateur du pipeline Astrologie.
/// Cache → [Calcul → Knowledge → Prompt → LLM → Sauvegarde cache] → AstroSynthesisResult
/// </summary>
public class AstrologySynthesisService : IAstrologySynthesisService
{
    private readonly IAstrologyCalculationEngine       _engine;
    private readonly IAstrologyKnowledgeBaseRepository _knowledgeRepo;
    private readonly IAstrologyPromptBuilder           _promptBuilder;
    private readonly ILlmClient                        _llmClient;
    private readonly IAstrologySynthesisCacheRepository _cache;

    public AstrologySynthesisService(
        IAstrologyCalculationEngine        engine,
        IAstrologyKnowledgeBaseRepository  knowledgeRepo,
        IAstrologyPromptBuilder            promptBuilder,
        ILlmClient                         llmClient,
        IAstrologySynthesisCacheRepository cache)
    {
        _engine        = engine;
        _knowledgeRepo = knowledgeRepo;
        _promptBuilder = promptBuilder;
        _llmClient     = llmClient;
        _cache         = cache;
    }

    public async Task<AstroSynthesisResult> RunAsync(
        NatalChartInput   input,
        CancellationToken cancellationToken = default)
    {
        var cached = await _cache.LoadAsync(input);
        if (cached is not null)
        {
            var cachedProfile = _engine.Calculate(input);
            return new AstroSynthesisResult(cachedProfile, cached);
        }

        var profile   = _engine.Calculate(input);
        var knowledge = await _knowledgeRepo.LoadProfileContentAsync(profile);
        var prompt    = _promptBuilder.Build(profile, knowledge);
        var text      = await _llmClient.GenerateAsync(prompt, cancellationToken);

        await _cache.SaveAsync(input, text);

        return new AstroSynthesisResult(profile, text);
    }
}
