namespace HolisticProfile.Core.Models;

/// <summary>
/// Aspect entre deux planètes dans le thème natal.
/// </summary>
/// <param name="Planet1">Première planète.</param>
/// <param name="Planet2">Deuxième planète.</param>
/// <param name="Type">Type d'aspect (conjonction, carré, trigone…).</param>
/// <param name="Orb">Écart à l'exactitude en degrés (toujours positif).</param>
/// <param name="IsApplying">Vrai si l'aspect est encore en formation (planète rapide se rapproche de l'exactitude).</param>
public record AstroAspect(
    Planet     Planet1,
    Planet     Planet2,
    AspectType Type,
    double     Orb,
    bool       IsApplying = false)
{
    public override string ToString()
        => $"{Planet1.ToFrench()} {Type.Symbol()} {Planet2.ToFrench()} ({Type.ToFrench()}, orbe {Orb:F1}°{(IsApplying ? " ↗" : "")})";
}
