using FluentAssertions;
using HolisticProfile.Core.Engines;

namespace HolisticProfile.Core.Tests.Engines;

/// <summary>
/// Tests du moteur de calcul du Référentiel de Naissance (méthode Georges Colleuil).
///
/// Cas de référence principal tiré de ALGO_ReferentielNaissance.md :
///   Date : 24/06/1971, Année : 2026
///   M1=6, M2=6, M3=18, M4=3, M5=6, M6=12, M7=12, M8=13, M9=6, M10=16, M11=10, M12=21, M13=10
///   M14 = As d'Épée (n°33)
/// </summary>
public class ReferentielCalculationEngineTests
{
    private readonly ReferentielCalculationEngine _engine = new();

    // ─── Cas de référence complet ───────────────────────────────────────────────

    [Fact]
    public void Calculate_ReferenceCase_24061971_Year2026_AllHousesCorrect()
    {
        var profile = _engine.Calculate(new DateTime(1971, 6, 24), 2026);

        profile[1].Major!.Numero.Should().Be(6,  "M1 = JOUR 24 → 2+4 = 6");
        profile[2].Major!.Numero.Should().Be(6,  "M2 = MOIS 6");
        profile[3].Major!.Numero.Should().Be(18, "M3 = 1+9+7+1 = 18");
        profile[4].Major!.Numero.Should().Be(3,  "M4 = 6+6+18 = 30 → 3+0 = 3");
        profile[5].Major!.Numero.Should().Be(6,  "M5 = 6+6+18+3 = 33 → 3+3 = 6");
        profile[6].Major!.Numero.Should().Be(12, "M6 = 6+6 = 12");
        profile[7].Major!.Numero.Should().Be(12, "M7 = 18-6 = 12");
        profile[8].Major!.Numero.Should().Be(13, "M8 = 12 + AU(2026=1) = 13");
        profile[9].Major!.Numero.Should().Be(6,  "M9 = 12+12 = 24 → 2+4 = 6");
        profile[10].Major!.Numero.Should().Be(16, "M10 = 22-6 = 16");
        profile[11].Major!.Numero.Should().Be(10, "M11 = 12+18+16 = 46 → 4+6 = 10");
        profile[12].Major!.Numero.Should().Be(21, "M12 = 12+6+3 = 21");
        profile[13].Major!.Numero.Should().Be(10, "M13 = 21+6+6+18+10+3+6+6+6 = 82 → 8+2 = 10");
        profile[14].Minor!.Numero.Should().Be(33, "M14 = M5Raw=33 → As d'Épée");
        profile[14].Minor!.Carte.Should().Be("As");
        profile[14].Minor!.Famille.Should().Be("Épées");
    }

    // ─── ReduceBase22 ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1,  1)]
    [InlineData(22, 22)]
    [InlineData(23, 5)]   // 2+3
    [InlineData(25, 7)]   // 2+5
    [InlineData(30, 3)]   // 3+0
    [InlineData(44, 8)]   // 4+4
    [InlineData(0,  22)]  // 0 → Le Mat
    public void ReduceBase22_ReturnsExpected(int input, int expected)
        => ReferentielCalculationEngine.ReduceBase22(input).Should().Be(expected);

    // ─── ReduceBase9WithMasterNumbers ─────────────────────────────────────────

    [Theory]
    [InlineData(2024, 8)]   // 2+0+2+4=8
    [InlineData(2025, 9)]   // 2+0+2+5=9
    [InlineData(2026, 1)]   // 2+0+2+6=10 → 1+0=1
    [InlineData(2009, 11)]  // 2+0+0+9=11 → maître nombre conservé
    [InlineData(1994, 5)]   // 1+9+9+4=23 → 2+3=5
    public void ReduceBase9_CurrentYear_ReturnsExpected(int year, int expected)
    {
        var sumDigits = ReferentielCalculationEngine.SumDigits(year);
        ReferentielCalculationEngine.ReduceBase9WithMasterNumbers(sumDigits).Should().Be(expected);
    }

    [Theory]
    [InlineData(11, 11)]  // maître nombre 11 conservé
    [InlineData(22, 22)]  // maître nombre 22 conservé
    [InlineData(9,  9)]
    [InlineData(1,  1)]
    [InlineData(18, 9)]   // 1+8
    public void ReduceBase9WithMasterNumbers_ReturnsExpected(int input, int expected)
        => ReferentielCalculationEngine.ReduceBase9WithMasterNumbers(input).Should().Be(expected);

    // ─── ConvertToMinorArcaneNumber ────────────────────────────────────────────

    [Theory]
    [InlineData(33, 33, "As",      "Épées")]   // As d'Épée
    [InlineData(1,   1, "Roi",     "Bâtons")]
    [InlineData(14, 14, "10",      "Bâtons")]
    [InlineData(15, 15, "Roi",     "Coupes")]
    [InlineData(28, 28, "10",      "Coupes")]
    [InlineData(29, 29, "Roi",     "Épées")]
    [InlineData(42, 42, "10",      "Épées")]
    [InlineData(43, 43, "Roi",     "Deniers")]
    [InlineData(56, 56, "10",      "Deniers")]
    [InlineData(5,   5, "As",      "Bâtons")]
    [InlineData(57, 12, "8",       "Bâtons")]   // 57 > 56 → 5+7=12 → rang 12 → 12-4=8 de Bâtons
    public void ConvertToMinorArcaneNumber_ReturnsExpected(int raw, int expectedNumero, string expectedCarte, string expectedFamille)
    {
        var numero = ReferentielCalculationEngine.ConvertToMinorArcaneNumber(raw);
        var arcane = new global::HolisticProfile.Core.Models.MinorArcane(numero);

        numero.Should().Be(expectedNumero);
        arcane.Carte.Should().Be(expectedCarte);
        arcane.Famille.Should().Be(expectedFamille);
    }

    // ─── Cas limites M7 (soustraction) ────────────────────────────────────────

    [Fact]
    public void Calculate_WhenM3EqualsM2_M7Is22()
    {
        // ANNEE dont somme = MOIS
        // Mois = 3 (mars), ANNEE dont somme chiffres réduit base 22 = 3
        // ex: 03/03/2001 → M3 = 2+0+0+1=3, M2=3 → M7 = 3-3 = 0 → +22 = 22
        var profile = _engine.Calculate(new DateTime(2001, 3, 3), 2026);

        profile[7].Major!.Numero.Should().Be(22, "M7 = 0 → Le Mat (22)");
    }

    [Fact]
    public void Calculate_WhenM3LessThanM2_M7IsPositive()
    {
        // M3 < M2 → résultat négatif → +22
        // ex: 01/12/2001 → M2=12, M3=2+0+0+1=3 → M7 = 3-12 = -9 → -9+22 = 13
        var profile = _engine.Calculate(new DateTime(2001, 12, 1), 2026);

        profile[7].Major!.Numero.Should().Be(13, "M7 = 3-12 = -9 → -9+22 = 13");
    }

    // ─── Maison 8 dynamique ────────────────────────────────────────────────────

    [Fact]
    public void Calculate_SameBirthDate_DifferentYear_M8Differs()
    {
        var profile2024 = _engine.Calculate(new DateTime(1971, 6, 24), 2024);
        var profile2026 = _engine.Calculate(new DateTime(1971, 6, 24), 2026);

        profile2024[8].Major!.Numero.Should().Be(20, "M8(2024) = M6(12) + AU(8) = 20");
        profile2026[8].Major!.Numero.Should().Be(13, "M8(2026) = M6(12) + AU(1) = 13");
    }

    // ─── Preuve par 9 ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1971, 6, 24)]
    [InlineData(1987, 3, 15)]
    [InlineData(1964, 7,  2)]
    [InlineData(2001, 3,  3)]
    public void Calculate_M5Base9_EqualsDoubleM4Base9(int year, int month, int day)
    {
        // Vérification interne Colleuil : M5 en base 9 = double de M4 en base 9
        var profile = _engine.Calculate(new DateTime(year, month, day), 2026);

        var m4Base9 = ReferentielCalculationEngine.ReduceBase9WithMasterNumbers(
            ReferentielCalculationEngine.SumDigits(profile[4].Major!.Numero));
        var m5Base9 = ReferentielCalculationEngine.ReduceBase9WithMasterNumbers(
            ReferentielCalculationEngine.SumDigits(profile[5].Major!.Numero));

        var doubleMod9 = ((m4Base9 * 2 - 1) % 9) + 1;

        m5Base9.Should().Be(doubleMod9, $"preuve par 9 pour {day:00}/{month:00}/{year}");
    }

    // ─── Validation ───────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_FutureDate_ThrowsArgumentException()
    {
        var act = () => _engine.Calculate(DateTime.Today.AddDays(1), 2026);

        act.Should().Throw<ArgumentException>()
           .WithParameterName("birthDate");
    }

    [Fact]
    public void Calculate_Returns14Houses()
    {
        var profile = _engine.Calculate(new DateTime(1987, 3, 15), 2026);

        profile.Houses.Should().HaveCount(14);
    }
}
