using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

/// <summary>
/// Recherche de lieux de naissance dans la table locale des villes.
/// Aucune requête réseau : la table est embarquée avec l'application.
/// </summary>
public interface IPlaceRepository
{
    /// <summary>
    /// Recherche les lieux dont le nom correspond à <paramref name="query"/>.
    /// La comparaison ignore la casse, les accents et les tirets.
    /// Les correspondances exactes puis par préfixe sont remontées en tête.
    /// </summary>
    /// <returns>Liste éventuellement vide, jamais null.</returns>
    Task<IReadOnlyList<Place>> SearchAsync(string query, int maxResults = 10);
}
