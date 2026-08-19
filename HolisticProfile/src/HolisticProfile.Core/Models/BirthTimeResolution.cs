namespace HolisticProfile.Core.Models;

/// <summary>Nature de la correspondance entre une heure locale et le temps universel.</summary>
public enum BirthTimeKind
{
    /// <summary>Cas normal : une seule interprétation possible.</summary>
    Unique,

    /// <summary>
    /// Heure vécue deux fois (retour à l'heure d'hiver) : deux décalages sont possibles.
    /// Le praticien doit trancher — l'ascendant peut changer de plusieurs degrés.
    /// </summary>
    Ambiguous,

    /// <summary>
    /// Heure inexistante (passage à l'heure d'été) : l'horloge a sauté cette tranche.
    /// L'heure saisie est très probablement erronée.
    /// </summary>
    Skipped,
}

/// <summary>
/// Résultat de la résolution d'une heure locale de naissance vers un décalage UTC,
/// en tenant compte des règles historiques du fuseau (heure d'été, réformes horaires).
/// </summary>
/// <param name="UtcOffset">Décalage retenu.</param>
/// <param name="Kind">Nature de la correspondance.</param>
/// <param name="AlternativeUtcOffset">
/// Second décalage possible lorsque <see cref="Kind"/> vaut <see cref="BirthTimeKind.Ambiguous"/>,
/// null sinon.
/// </param>
public record BirthTimeResolution(
    TimeSpan  UtcOffset,
    BirthTimeKind Kind = BirthTimeKind.Unique,
    TimeSpan? AlternativeUtcOffset = null)
{
    /// <summary>Vrai si la saisie mérite une confirmation du praticien.</summary>
    public bool NeedsConfirmation => Kind != BirthTimeKind.Unique;
}
