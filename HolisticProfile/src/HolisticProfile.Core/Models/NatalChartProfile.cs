namespace HolisticProfile.Core.Models;

/// <summary>
/// Thème natal complet, calculé par le moteur astrologique.
/// Contient les positions planétaires, les maisons et les aspects.
/// </summary>
public record NatalChartProfile(
    NatalChartInput              Input,
    IReadOnlyList<PlanetPosition> Planets,
    IReadOnlyList<HousePosition>  Houses,
    IReadOnlyList<AstroAspect>    Aspects)
{
    /// <summary>Position d'une planète donnée, ou null si elle n'est pas dans le thème.</summary>
    public PlanetPosition? GetPlanet(Planet planet)
        => Planets.FirstOrDefault(p => p.Planet == planet);

    /// <summary>Ascendant (cuspide de la Maison 1).</summary>
    public HousePosition Ascendant => Houses[0];

    /// <summary>Milieu-du-Ciel (cuspide de la Maison 10).</summary>
    public HousePosition MidHeaven => Houses[9];
}
