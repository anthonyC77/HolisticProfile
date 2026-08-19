using FluentAssertions;
using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using HolisticProfile.Core.Services;
using NSubstitute;

namespace HolisticProfile.Core.Tests.Services;

/// <summary>
/// Tests unitaires de AstrologySynthesisService.
/// Toutes les dépendances sont mockées — aucun appel LLM réel.
/// </summary>
public class AstrologySynthesisServiceTests
{
    // ─── Fixtures ─────────────────────────────────────────────────────────────

    private static readonly NatalChartInput SampleInput = new(
        new DateTime(1987, 3, 15, 14, 30, 0),
        TimeSpan.FromHours(1),
        48.8566,
        2.3522,
        "Paris");

    private static NatalChartProfile BuildFakeProfile()
    {
        var planets = new List<PlanetPosition>
        {
            new(Planet.Sun,  352.5, ZodiacSign.Pisces,  22.5, 10, false),
            new(Planet.Moon, 210.0, ZodiacSign.Scorpio,  0.0,  7, false),
        };
        var houses = Enumerable.Range(1, 12)
            .Select(h => new HousePosition(h, h * 30.0, (ZodiacSign)(h - 1), 0.0))
            .ToList();
        var aspects = new List<AstroAspect>
        {
            new(Planet.Sun, Planet.Moon, AspectType.Trine, 2.5),
        };
        return new NatalChartProfile(SampleInput, planets, houses, aspects);
    }

    // ─── Helpers pour construire le SUT ───────────────────────────────────────

    private (
        IAstrologyCalculationEngine       engine,
        IAstrologyKnowledgeBaseRepository knowledgeRepo,
        IAstrologyPromptBuilder           promptBuilder,
        ILlmClient                        llmClient,
        IAstrologySynthesisCacheRepository cache,
        AstrologySynthesisService         sut)
    CreateSut()
    {
        var engine       = Substitute.For<IAstrologyCalculationEngine>();
        var knowledgeRepo= Substitute.For<IAstrologyKnowledgeBaseRepository>();
        var promptBuilder= Substitute.For<IAstrologyPromptBuilder>();
        var llmClient    = Substitute.For<ILlmClient>();
        var cache        = Substitute.For<IAstrologySynthesisCacheRepository>();

        var sut = new AstrologySynthesisService(engine, knowledgeRepo, promptBuilder, llmClient, cache);

        return (engine, knowledgeRepo, promptBuilder, llmClient, cache, sut);
    }

    // ─── Tests ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenCacheHit_ReturnsCachedTextWithoutCallingLlm()
    {
        var (engine, _, _, llmClient, cache, sut) = CreateSut();

        var fakeProfile = BuildFakeProfile();
        engine.Calculate(Arg.Any<NatalChartInput>()).Returns(fakeProfile);
        cache.LoadAsync(Arg.Any<NatalChartInput>()).Returns("Synthèse en cache");

        var result = await sut.RunAsync(SampleInput);

        result.Text.Should().Be("Synthèse en cache");
        await llmClient.DidNotReceive().GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenCacheMiss_CallsLlmAndSavesCache()
    {
        var (engine, knowledgeRepo, promptBuilder, llmClient, cache, sut) = CreateSut();

        var fakeProfile = BuildFakeProfile();
        engine.Calculate(Arg.Any<NatalChartInput>()).Returns(fakeProfile);
        cache.LoadAsync(Arg.Any<NatalChartInput>()).Returns((string?)null);
        knowledgeRepo.LoadProfileContentAsync(Arg.Any<NatalChartProfile>()).Returns("Contenu KB");
        promptBuilder.Build(Arg.Any<NatalChartProfile>(), Arg.Any<string>()).Returns("Prompt");
        llmClient.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("Nouvelle synthèse");

        var result = await sut.RunAsync(SampleInput);

        result.Text.Should().Be("Nouvelle synthèse");
        await cache.Received(1).SaveAsync(SampleInput, "Nouvelle synthèse");
    }

    [Fact]
    public async Task RunAsync_ProfileContainsCalculatedData()
    {
        var (engine, knowledgeRepo, promptBuilder, llmClient, cache, sut) = CreateSut();

        var fakeProfile = BuildFakeProfile();
        engine.Calculate(Arg.Any<NatalChartInput>()).Returns(fakeProfile);
        cache.LoadAsync(Arg.Any<NatalChartInput>()).Returns((string?)null);
        knowledgeRepo.LoadProfileContentAsync(Arg.Any<NatalChartProfile>()).Returns(string.Empty);
        promptBuilder.Build(Arg.Any<NatalChartProfile>(), Arg.Any<string>()).Returns("Prompt");
        llmClient.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("Synthèse");

        var result = await sut.RunAsync(SampleInput);

        result.Profile.Should().BeSameAs(fakeProfile);
        result.Profile.Planets.Should().HaveCount(2);
        result.Profile.Aspects.Should().HaveCount(1);
    }

    [Fact]
    public async Task RunAsync_PromptBuilderReceivesCorrectArguments()
    {
        var (engine, knowledgeRepo, promptBuilder, llmClient, cache, sut) = CreateSut();

        var fakeProfile    = BuildFakeProfile();
        const string kbContent = "Contenu de la KB";

        engine.Calculate(Arg.Any<NatalChartInput>()).Returns(fakeProfile);
        cache.LoadAsync(Arg.Any<NatalChartInput>()).Returns((string?)null);
        knowledgeRepo.LoadProfileContentAsync(Arg.Any<NatalChartProfile>()).Returns(kbContent);
        promptBuilder.Build(Arg.Any<NatalChartProfile>(), Arg.Any<string>()).Returns("Prompt");
        llmClient.GenerateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("Synthèse");

        await sut.RunAsync(SampleInput);

        promptBuilder.Received(1).Build(fakeProfile, kbContent);
    }
}
