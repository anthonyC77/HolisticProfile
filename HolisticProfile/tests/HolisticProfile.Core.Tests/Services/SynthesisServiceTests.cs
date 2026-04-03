using FluentAssertions;
using HolisticProfile.Core.Engines;
using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using HolisticProfile.Core.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HolisticProfile.Core.Tests.Services;

public class SynthesisServiceTests
{
    private readonly ICalculationEngine _engine;
    private readonly IKnowledgeBaseRepository _knowledgeRepo;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILlmClient _llmClient;
    private readonly ISynthesisCacheRepository _cache;
    private readonly SynthesisService _service;

    public SynthesisServiceTests()
    {
        _engine        = Substitute.For<ICalculationEngine>();
        _knowledgeRepo = Substitute.For<IKnowledgeBaseRepository>();
        _promptBuilder = Substitute.For<IPromptBuilder>();
        _llmClient     = Substitute.For<ILlmClient>();
        _cache         = Substitute.For<ISynthesisCacheRepository>();

        _service = new SynthesisService(_engine, _knowledgeRepo, _promptBuilder, _llmClient, _cache);
    }

    // --- Comportement existant (inchangé) ---

    [Fact]
    public async Task RunAsync_HappyPath_ReturnsSynthesisResultWithProfileAndText()
    {
        var birthDate = new DateTime(1987, 3, 15);
        var lifePath  = new MillmanCalculationEngine().Calculate(birthDate);
        var profile   = new BirthProfile(birthDate, lifePath);

        _engine.Calculate(birthDate).Returns(lifePath);
        _knowledgeRepo.LoadAsync(lifePath.PathKey).Returns("contenu de la fiche");
        _promptBuilder.Build(profile, "contenu de la fiche").Returns("le prompt assemblé");
        _llmClient.GenerateAsync("le prompt assemblé", Arg.Any<CancellationToken>()).Returns("la synthèse générée");
        _cache.LoadAsync(profile).Returns((string?)null);

        var result = await _service.RunAsync(birthDate);

        result.Profile.Should().Be(profile);
        result.Text.Should().Be("la synthèse générée");
    }

    [Fact]
    public async Task RunAsync_CallsEngineWithBirthDate()
    {
        var birthDate = new DateTime(1987, 3, 15);
        SetupDefaults(birthDate);

        await _service.RunAsync(birthDate);

        _engine.Received(1).Calculate(birthDate);
    }

    [Fact]
    public async Task RunAsync_LoadsKnowledgeUsingPathKey()
    {
        var birthDate = new DateTime(1987, 3, 15);
        var lifePath  = SetupDefaults(birthDate);

        await _service.RunAsync(birthDate);

        await _knowledgeRepo.Received(1).LoadAsync(lifePath.PathKey);
    }

    [Fact]
    public async Task RunAsync_KnowledgeNotFound_StillCallsLlm()
    {
        var birthDate = new DateTime(1987, 3, 15);
        SetupDefaults(birthDate, knowledge: null);

        await _service.RunAsync(birthDate);

        await _llmClient.Received(1).GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_LlmThrows_PropagatesException()
    {
        var birthDate = new DateTime(1987, 3, 15);
        SetupDefaults(birthDate);
        _llmClient.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                  .ThrowsAsync(new HttpRequestException("Ollama injoignable"));

        var act = () => _service.RunAsync(birthDate);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // --- Comportement du cache ---

    [Fact]
    public async Task RunAsync_CacheHit_ReturnsCachedTextWithoutCallingLlm()
    {
        var birthDate = new DateTime(1987, 3, 15);
        var lifePath  = new MillmanCalculationEngine().Calculate(birthDate);
        var profile   = new BirthProfile(birthDate, lifePath);

        _engine.Calculate(birthDate).Returns(lifePath);
        _cache.LoadAsync(profile).Returns("synthèse déjà en cache");

        var result = await _service.RunAsync(birthDate);

        result.Text.Should().Be("synthèse déjà en cache");
        await _llmClient.DidNotReceive().GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_CacheMiss_SavesSynthesisAfterLlmCall()
    {
        var birthDate = new DateTime(1987, 3, 15);
        var lifePath  = new MillmanCalculationEngine().Calculate(birthDate);
        var profile   = new BirthProfile(birthDate, lifePath);

        _engine.Calculate(birthDate).Returns(lifePath);
        _cache.LoadAsync(profile).Returns((string?)null);
        _knowledgeRepo.LoadAsync(lifePath.PathKey).Returns("contenu");
        _promptBuilder.Build(profile, "contenu").Returns("prompt");
        _llmClient.GenerateAsync("prompt", Arg.Any<CancellationToken>()).Returns("nouvelle synthèse");

        await _service.RunAsync(birthDate);

        await _cache.Received(1).SaveAsync(profile, "nouvelle synthèse");
    }

    [Fact]
    public async Task RunAsync_CacheHit_SkipsKnowledgeBaseAndPromptBuilder()
    {
        var birthDate = new DateTime(1987, 3, 15);
        var lifePath  = new MillmanCalculationEngine().Calculate(birthDate);
        var profile   = new BirthProfile(birthDate, lifePath);

        _engine.Calculate(birthDate).Returns(lifePath);
        _cache.LoadAsync(profile).Returns("en cache");

        await _service.RunAsync(birthDate);

        await _knowledgeRepo.DidNotReceive().LoadAsync(Arg.Any<string>());
        _promptBuilder.DidNotReceive().Build(Arg.Any<BirthProfile>(), Arg.Any<string?>());
    }

    // Helper
    private MillmanLifePath SetupDefaults(DateTime birthDate, string? knowledge = "contenu")
    {
        var lifePath = new MillmanCalculationEngine().Calculate(birthDate);
        var profile  = new BirthProfile(birthDate, lifePath);

        _engine.Calculate(birthDate).Returns(lifePath);
        _cache.LoadAsync(profile).Returns((string?)null);
        _knowledgeRepo.LoadAsync(lifePath.PathKey).Returns(knowledge);
        _promptBuilder.Build(profile, knowledge).Returns("prompt");
        _llmClient.GenerateAsync("prompt", Arg.Any<CancellationToken>()).Returns("synthèse");

        return lifePath;
    }
}
