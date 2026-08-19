using FluentAssertions;
using HolisticProfile.Core.Models;
using HolisticProfile.Infrastructure.Prompt;

namespace HolisticProfile.Infrastructure.Tests.Prompt;

public class AstrologyPromptBuilderTests
{
    private readonly AstrologyPromptBuilder _builder = new();

    private static NatalChartProfile BuildProfile()
    {
        var input = new NatalChartInput(
            new DateTime(1987, 3, 15, 14, 30, 0),
            TimeSpan.FromHours(1),
            48.8566, 2.3522, "Paris");

        var planets = new List<PlanetPosition>
        {
            new(Planet.Sun,     352.5, ZodiacSign.Pisces,   22.5, 10, false),
            new(Planet.Moon,    212.0, ZodiacSign.Scorpio,   2.0,  7, false),
            new(Planet.Mercury,  14.0, ZodiacSign.Aries,    14.0,  9, false),
            new(Planet.Venus,    22.0, ZodiacSign.Aries,    22.0, 10, false),
            new(Planet.Mars,    280.0, ZodiacSign.Capricorn, 10.0,  8, false),
            new(Planet.Jupiter, 340.0, ZodiacSign.Pisces,   10.0,  9, true),
            new(Planet.Saturn,  270.0, ZodiacSign.Capricorn, 0.0,  8, false),
            new(Planet.Uranus,  272.0, ZodiacSign.Capricorn, 2.0,  8, false),
            new(Planet.Neptune,  280.5, ZodiacSign.Capricorn, 10.5, 8, false),
            new(Planet.Pluto,   220.0, ZodiacSign.Scorpio,  10.0,  7, false),
            new(Planet.NorthNode, 350.0, ZodiacSign.Pisces, 20.0,  9, false),
        };

        var houses = Enumerable.Range(1, 12)
            .Select(h => new HousePosition(h, h * 30.0, (ZodiacSign)(h - 1), 0.0))
            .ToList();

        var aspects = new List<AstroAspect>
        {
            new(Planet.Sun, Planet.Moon,    AspectType.Trine,       2.5),
            new(Planet.Sun, Planet.Jupiter, AspectType.Conjunction,  1.2),
            new(Planet.Moon, Planet.Saturn, AspectType.Square,       4.0),
        };

        return new NatalChartProfile(input, planets, houses, aspects);
    }

    [Fact]
    public void Build_ContainsDataDeNaissance()
    {
        var prompt = _builder.Build(BuildProfile(), string.Empty);

        prompt.Should().Contain("Paris", "le lieu de naissance doit apparaître");
        prompt.Should().Contain("1987", "l'année de naissance doit apparaître");
    }

    [Fact]
    public void Build_ContainsPositionsPlanetaires()
    {
        var prompt = _builder.Build(BuildProfile(), string.Empty);

        prompt.Should().Contain("Soleil",      "la planète Soleil doit figurer dans le prompt");
        prompt.Should().Contain("Lune",        "la planète Lune doit figurer");
        prompt.Should().Contain("Poissons",    "le signe du Soleil doit figurer");
        prompt.Should().Contain("Scorpion",    "le signe de la Lune doit figurer");
    }

    [Fact]
    public void Build_ContainsAscendantAndMc()
    {
        var prompt = _builder.Build(BuildProfile(), string.Empty);

        prompt.Should().Contain("ASC",  "l'ascendant doit être mentionné");
        prompt.Should().Contain("MC",   "le milieu du ciel doit être mentionné");
    }

    [Fact]
    public void Build_ContainsAspects()
    {
        var prompt = _builder.Build(BuildProfile(), string.Empty);

        prompt.Should().Contain("Trigone",      "l'aspect trigone Sun/Moon doit figurer");
        prompt.Should().Contain("Conjonction",  "l'aspect conjonction Sun/Jupiter doit figurer");
    }

    [Fact]
    public void Build_WithKnowledgeContent_IncludesItInPrompt()
    {
        const string kb = "## Soleil en Poissons\nSensibilité, intuition, compassion.";
        var prompt = _builder.Build(BuildProfile(), kb);

        prompt.Should().Contain(kb);
    }

    [Fact]
    public void Build_WithEmptyKnowledge_DoesNotIncludeKbSection()
    {
        var prompt = _builder.Build(BuildProfile(), string.Empty);

        prompt.Should().NotContain("## Base de connaissances");
    }

    [Fact]
    public void Build_ContainsConsigne()
    {
        var prompt = _builder.Build(BuildProfile(), string.Empty);

        prompt.Should().Contain("Consigne", "la section consigne doit être présente");
    }
}
