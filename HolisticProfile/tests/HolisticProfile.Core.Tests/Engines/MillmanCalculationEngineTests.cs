using FluentAssertions;
using HolisticProfile.Core.Engines;

namespace HolisticProfile.Core.Tests.Engines;

// Calcul Millman : somme de TOUS les chiffres de DDMMYYYY
// Chemin 2 parties : 15/03/1987 -> 1+5+0+3+1+9+8+7=34 -> 3+4=7 -> "34/7"
// Chemin 3 parties : somme -> intermédiaire >= 10 -> racine
//   Ex: 29/11/2 => chiffres 2, 9, 1, 1, 2 (le 1 est doublé = double poids / double défi)
//   PathKey 3 parties = "29_11_2" (les 3 niveaux, Millman les lit tous)
public class MillmanCalculationEngineTests
{
    private readonly MillmanCalculationEngine _engine = new();

    // --- Chemins 2 parties ---

    [Theory]
    [InlineData(1987, 3,  15, 34, null, 7, "34/7")]  // 1+5+0+3+1+9+8+7=34, 3+4=7
    [InlineData(1992, 12, 17, 32, null, 5, "32/5")]  // 1+7+1+2+1+9+9+2=32, 3+2=5
    [InlineData(1976, 2,  29, 36, null, 9, "36/9")]  // 2+9+0+2+1+9+7+6=36, 3+6=9
    [InlineData(1989, 8,   5, 40, null, 4, "40/4")]  // 0+5+0+8+1+9+8+9=40, 4+0=4
    [InlineData(1979, 9,  19, 45, null, 9, "45/9")]  // 1+9+0+9+1+9+7+9=45, 4+5=9
    [InlineData(1969, 9,  28, 44, null, 8, "44/8")]  // 2+8+0+9+1+9+6+9=44, 4+4=8
    public void Calculate_TwoPartPath_ReturnsCorrectPath(
        int year, int month, int day,
        int expectedSum, int? expectedIntermediate, int expectedRoot, string expectedNotation)
    {
        var result = _engine.Calculate(new DateTime(year, month, day));

        result.Sum.Should().Be(expectedSum);
        result.IntermediateSum.Should().Be(expectedIntermediate);
        result.Root.Should().Be(expectedRoot);
        result.ToString().Should().Be(expectedNotation);
    }

    // --- Chemins 3 parties ---
    // Millman considère tous les chiffres : 29/11/2 => 2, 9, 1, 1, 2
    // Le 1 est doublé => double poids, double défi

    [Theory]
    [InlineData(1964, 7,  2, 29, 11, 2, "29/11/2")]  // 0+2+0+7+1+9+6+4=29, 2+9=11, 1+1=2
    [InlineData(1972, 9, 19, 38, 11, 2, "38/11/2")]  // 1+9+0+9+1+9+7+2=38, 3+8=11, 1+1=2
    [InlineData(1964, 9,  8, 37, 10, 1, "37/10/1")]  // 0+8+0+9+1+9+6+4=37, 3+7=10, 1+0=1
    [InlineData(1966, 8,  9, 39, 12, 3, "39/12/3")]  // 0+9+0+8+1+9+6+6=39, 3+9=12, 1+2=3
    public void Calculate_ThreePartPath_ReturnsCorrectPath(
        int year, int month, int day,
        int expectedSum, int? expectedIntermediate, int expectedRoot, string expectedNotation)
    {
        var result = _engine.Calculate(new DateTime(year, month, day));

        result.Sum.Should().Be(expectedSum);
        result.IntermediateSum.Should().Be(expectedIntermediate);
        result.Root.Should().Be(expectedRoot);
        result.ToString().Should().Be(expectedNotation);
    }

    // --- Somme très faible (chiffre unique direct) ---

    [Fact]
    public void Calculate_LowDigitSum_ReturnsSingleDigitPath()
    {
        // 01/01/2000 -> 0+1+0+1+2+0+0+0=4
        var result = _engine.Calculate(new DateTime(2000, 1, 1));

        result.Sum.Should().Be(4);
        result.IntermediateSum.Should().BeNull();
        result.Root.Should().Be(4);
        result.ToString().Should().Be("4/4");
    }

    // --- PathKey ---

    [Fact]
    public void Calculate_TwoPartPath_PathKey_IsFileSystemSafe()
    {
        // 34/7 -> "34_7"
        var result = _engine.Calculate(new DateTime(1987, 3, 15));

        result.PathKey.Should().Be("34_7");
    }

    [Fact]
    public void Calculate_ThreePartPath_PathKey_IncludesAllThreeLevels()
    {
        // 29/11/2 -> "29_11_2" (Millman lit les 3 niveaux : 2,9,1,1,2)
        var result = _engine.Calculate(new DateTime(1964, 7, 2));

        result.PathKey.Should().Be("29_11_2");
    }

    [Fact]
    public void Calculate_AnotherThreePartPath_PathKey_IncludesAllThreeLevels()
    {
        // 37/10/1 -> "37_10_1"
        var result = _engine.Calculate(new DateTime(1964, 9, 8));

        result.PathKey.Should().Be("37_10_1");
    }

    // --- Immutabilité et égalité ---

    [Fact]
    public void Calculate_SameDateTwice_ReturnsEqualRecords()
    {
        var result1 = _engine.Calculate(new DateTime(1987, 3, 15));
        var result2 = _engine.Calculate(new DateTime(1987, 3, 15));

        result1.Should().Be(result2);
    }

    // --- Validation ---

    [Fact]
    public void Calculate_FutureDate_ThrowsArgumentException()
    {
        var act = () => _engine.Calculate(DateTime.Today.AddDays(1));

        act.Should().Throw<ArgumentException>()
           .WithParameterName("birthDate");
    }
}
