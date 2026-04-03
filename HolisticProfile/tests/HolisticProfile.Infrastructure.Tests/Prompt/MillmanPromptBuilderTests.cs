using FluentAssertions;
using HolisticProfile.Core.Engines;
using HolisticProfile.Core.Models;
using HolisticProfile.Infrastructure.Prompt;

namespace HolisticProfile.Infrastructure.Tests.Prompt;

public class MillmanPromptBuilderTests
{
    private readonly MillmanCalculationEngine _engine = new();
    private readonly MillmanPromptBuilder _builder = new();

    [Fact]
    public void Build_WithKnowledge_ContainsLifePathNotation()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15)); // 34/7

        var prompt = _builder.Build(profile, "contenu connaissances");

        prompt.Should().Contain("34/7");
    }

    [Fact]
    public void Build_WithKnowledge_ContainsBirthDate()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15));

        var prompt = _builder.Build(profile, "contenu connaissances");

        prompt.Should().Contain("15/03/1987");
    }

    [Fact]
    public void Build_WithKnowledge_ContainsKnowledgeContent()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15));
        var knowledge = "## Forces\nCapacité analytique profonde.";

        var prompt = _builder.Build(profile, knowledge);

        prompt.Should().Contain(knowledge);
    }

    [Fact]
    public void Build_WithKnowledge_ContainsSynthesisInstruction()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15));

        var prompt = _builder.Build(profile, "contenu");

        prompt.Should().Contain("synthèse");
        prompt.Should().Contain("Consigne");
    }

    [Fact]
    public void Build_ThreePartPath_MentionsAllLevels()
    {
        var profile = MakeProfile(new DateTime(1964, 7, 2)); // 29/11/2

        var prompt = _builder.Build(profile, "contenu");

        prompt.Should().Contain("29/11/2");
    }

    [Fact]
    public void Build_NullKnowledge_StillProducesValidPrompt()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15));

        var prompt = _builder.Build(profile, null);

        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("34/7");
    }

    [Fact]
    public void Build_PromptIsInFrench()
    {
        var profile = MakeProfile(new DateTime(1987, 3, 15));

        var prompt = _builder.Build(profile, "contenu");

        // Le prompt doit demander une réponse en français
        prompt.Should().ContainAny("français", "french", "French");
    }

    private BirthProfile MakeProfile(DateTime date)
    {
        var lifePath = _engine.Calculate(date);
        return new BirthProfile(date, lifePath);
    }
}
