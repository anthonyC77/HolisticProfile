using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace HolisticProfile.Infrastructure.Places;

/// <summary>
/// Table locale des lieux de naissance, chargée depuis un fichier JSON (data/places/cities.json).
///
/// La recherche est tolérante : casse, accents, tirets et apostrophes sont ignorés,
/// et « st » est assimilé à « saint » (« st etienne » → « Saint-Étienne »).
///
/// Ordre des résultats : correspondance exacte, puis début de nom, puis début d'un mot,
/// puis contenu, puis région/pays.
/// </summary>
public class JsonPlaceRepository : IPlaceRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling         = JsonCommentHandling.Skip,
        AllowTrailingCommas         = true,
    };

    private readonly string _filePath;
    private readonly Lazy<Task<IReadOnlyList<Place>>> _places;

    public JsonPlaceRepository(string filePath)
    {
        _filePath = Path.GetFullPath(filePath);
        _places   = new Lazy<Task<IReadOnlyList<Place>>>(LoadAsync);
    }

    /// <summary>Nombre de lieux dans la table (déclenche le chargement).</summary>
    public async Task<int> CountAsync() => (await _places.Value).Count;

    public async Task<IReadOnlyList<Place>> SearchAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        if (maxResults < 1) maxResults = 1;

        var needle = NormalizeForSearch(query);
        if (needle.Length == 0) return [];

        var places = await _places.Value;

        return places
            .Select(place => (place, rank: Rank(place, needle)))
            .Where(x => x.rank < int.MaxValue)
            .OrderBy(x => x.rank)
            .ThenBy(x => x.place.Name.Length)
            .ThenBy(x => x.place.Name, StringComparer.CurrentCulture)
            .Take(maxResults)
            .Select(x => x.place)
            .ToList();
    }

    // ─── Classement ───────────────────────────────────────────────────────────

    private static int Rank(Place place, string needle)
    {
        var name = NormalizeForSearch(place.Name);

        if (name == needle) return 0;
        if (name.StartsWith(needle, StringComparison.Ordinal)) return 1;

        // Début d'un mot du nom : « havre » → « le havre »
        if (name.Split(' ').Any(word => word.StartsWith(needle, StringComparison.Ordinal))) return 2;

        if (name.Contains(needle, StringComparison.Ordinal)) return 3;

        var region  = NormalizeForSearch(place.Region ?? string.Empty);
        var country = NormalizeForSearch(place.Country);

        if (region.Contains(needle, StringComparison.Ordinal) ||
            country.Contains(needle, StringComparison.Ordinal)) return 4;

        return int.MaxValue;
    }

    // ─── Normalisation ────────────────────────────────────────────────────────

    /// <summary>
    /// Minuscules, sans accents, séparateurs unifiés en espaces, « st » → « saint ».
    /// </summary>
    internal static string NormalizeForSearch(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var sb         = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            else                         sb.Append(' ');
        }

        var words = sb.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w == "st" ? "saint" : w);

        return string.Join(' ', words);
    }

    // ─── Chargement ───────────────────────────────────────────────────────────

    private async Task<IReadOnlyList<Place>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException(
                $"Table des lieux introuvable : {_filePath}", _filePath);

        await using var stream = File.OpenRead(_filePath);

        var places = await JsonSerializer.DeserializeAsync<List<Place>>(stream, JsonOptions);

        return places ?? [];
    }
}
