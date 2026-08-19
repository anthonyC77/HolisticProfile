using HolisticProfile.Console;
using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;


var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection()
    .AddHolisticProfile(config)
    .BuildServiceProvider();

System.Console.OutputEncoding = System.Text.Encoding.UTF8;
System.Console.WriteLine("=== HolisticProfile ===");
System.Console.WriteLine();
System.Console.WriteLine("1 — Numérologie Dan Millman");
System.Console.WriteLine("2 — Référentiel de Naissance (Colleuil)");
System.Console.WriteLine("3 — Thème Natal Astrologique");
System.Console.WriteLine();
System.Console.Write("Choix (1, 2 ou 3) : ");

var choice = System.Console.ReadLine()?.Trim();
System.Console.WriteLine();

DateTime birthDate;
while (true)
{
    System.Console.Write("Date de naissance (JJ/MM/AAAA) : ");
    var input = System.Console.ReadLine()?.Trim();

    if (DateTime.TryParseExact(input, "dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out birthDate))
        break;

    System.Console.WriteLine("  Format invalide, réessaie (ex: 15/03/1987)");
}

// Données supplémentaires pour l'astrologie
NatalChartInput? natalInput = null;
if (choice == "3")
    natalInput = await ReadNatalChartInputAsync(
        birthDate,
        services.GetRequiredService<IPlaceRepository>(),
        services.GetRequiredService<IBirthTimeZoneResolver>());

System.Console.WriteLine();
System.Console.WriteLine("Calcul en cours...");

var width = 80;
try { width = Math.Clamp(System.Console.WindowWidth - 2, 40, 80); } catch { /* redirigé */ }

try
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));

    if (choice == "3" && natalInput is not null)
    {
        var astroService = services.GetRequiredService<IAstrologySynthesisService>();
        var result = await astroService.RunAsync(natalInput, cts.Token);

        System.Console.WriteLine();
        System.Console.WriteLine($"Thème Natal — {result.Profile.Input}");
        System.Console.WriteLine();
        System.Console.WriteLine($"  ASC : {result.Profile.Ascendant.DegreeInSign:F1}° {result.Profile.Ascendant.Sign.ToFrench()}");
        System.Console.WriteLine($"  MC  : {result.Profile.MidHeaven.DegreeInSign:F1}° {result.Profile.MidHeaven.Sign.ToFrench()}");
        System.Console.WriteLine();
        foreach (var planet in result.Profile.Planets)
            System.Console.WriteLine($"  {planet}");
        System.Console.WriteLine();
        System.Console.WriteLine("  Aspects principaux :");
        foreach (var aspect in result.Profile.Aspects.Take(8))
            System.Console.WriteLine($"    {aspect}");
        System.Console.WriteLine(new string('─', width));
        System.Console.WriteLine();
        PrintWrapped(result.Text, lineWidth: width);
        System.Console.WriteLine();
        System.Console.WriteLine(new string('─', width));
    }
    else if (choice == "2")
    {
        var referentielService = services.GetRequiredService<IReferentielSynthesisService>();
        var result = await referentielService.RunAsync(birthDate, cts.Token);

        System.Console.WriteLine();
        System.Console.WriteLine($"Référentiel de Naissance — {result.Profile.BirthDate:dd/MM/yyyy} (année {result.Profile.CurrentYear})");
        System.Console.WriteLine();
        foreach (var house in result.Profile.Houses)
            System.Console.WriteLine($"  {house}");
        System.Console.WriteLine(new string('─', width));
        System.Console.WriteLine();
        PrintWrapped(result.Text, lineWidth: width);
        System.Console.WriteLine();
        System.Console.WriteLine(new string('─', width));
    }
    else
    {
        var synthesisService = services.GetRequiredService<ISynthesisService>();
        var result = await synthesisService.RunAsync(birthDate, cts.Token);

        System.Console.WriteLine();
        System.Console.WriteLine($"Chemin de vie Millman : {result.Profile.MillmanLifePath}");
        System.Console.WriteLine(new string('─', width));
        System.Console.WriteLine();
        PrintWrapped(result.Text, lineWidth: width);
        System.Console.WriteLine();
        System.Console.WriteLine(new string('─', width));
    }
}
catch (HttpRequestException ex)
{
    System.Console.WriteLine();
    System.Console.WriteLine($"[Erreur] Impossible de joindre Ollama : {ex.Message}");
    System.Console.WriteLine("Vérifie qu'Ollama est lancé : ollama serve");
}
catch (OperationCanceledException)
{
    System.Console.WriteLine();
    System.Console.WriteLine("[Erreur] Timeout — le modèle a mis trop longtemps à répondre.");
}

static void PrintWrapped(string text, int lineWidth)
{
    foreach (var line in text.Split('\n'))
    {
        var trimmed = line.TrimEnd('\r');

        // Lignes vides : telles quelles
        if (trimmed.Length == 0)
        {
            System.Console.WriteLine();
            continue;
        }

        // Titres markdown (### Titre) et items de liste (- item, * item) : tels quels
        // NB : "* item" commence par "* " (étoile + espace)
        //      "**gras**" commence par "**" → doit être wrappé comme du texte normal
        var isHeading  = trimmed.StartsWith('#');
        var isListItem = (trimmed.StartsWith("- ") || trimmed.StartsWith("* "));

        if (isHeading || isListItem)
        {
            System.Console.WriteLine(trimmed);
            continue;
        }

        // Tout le reste (paragraphes, texte en **gras**, etc.) → word-wrap
        WrapLine(trimmed, lineWidth);
    }
}

/// <summary>
/// Saisie des données du thème natal : lieu (table locale de villes) puis heure,
/// le décalage UTC étant déduit du fuseau du lieu et des règles historiques.
/// </summary>
static async Task<NatalChartInput> ReadNatalChartInputAsync(
    DateTime birthDate,
    IPlaceRepository places,
    IBirthTimeZoneResolver zones)
{
    var (latitude, longitude, placeName, timeZoneId) = await ReadPlaceAsync(places, zones);

    while (true)
    {
        var birthTime = ReadBirthTime();

        var birthDateTime = new DateTime(
            birthDate.Year, birthDate.Month, birthDate.Day,
            birthTime.Hour, birthTime.Minute, 0);

        // Lieu saisi à la main sans fuseau : le praticien donne le décalage
        if (timeZoneId is null)
            return new NatalChartInput(birthDateTime, ReadUtcOffset(), latitude, longitude, placeName);

        var resolution = zones.Resolve(birthDateTime, timeZoneId);

        if (resolution.Kind == BirthTimeKind.Skipped)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("  /!\\ Cette heure n'a jamais existé à cet endroit : cette nuit-là,");
            System.Console.WriteLine("      les horloges ont avancé d'une heure (passage à l'heure d'été).");
            System.Console.Write("      Ressaisir l'heure ? (O/n) : ");

            var again = System.Console.ReadLine()?.Trim().ToLowerInvariant();
            if (again is null or "" or "o" or "oui" or "y") continue;
        }
        else if (resolution.Kind == BirthTimeKind.Ambiguous)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("  /!\\ Cette heure a été vécue deux fois cette nuit-là (retour à l'heure d'hiver).");
            System.Console.WriteLine($"      1 — avant le changement d'heure : {FormatOffset(resolution.UtcOffset)}");
            System.Console.WriteLine($"      2 — après le changement d'heure : {FormatOffset(resolution.AlternativeUtcOffset!.Value)}");
            System.Console.Write("      Choix (1 ou 2, défaut 1) : ");

            if (System.Console.ReadLine()?.Trim() == "2")
                resolution = resolution with { UtcOffset = resolution.AlternativeUtcOffset!.Value };
        }

        System.Console.WriteLine($"  → Décalage retenu : {FormatOffset(resolution.UtcOffset)} ({timeZoneId})");

        return new NatalChartInput(birthDateTime, resolution.UtcOffset, latitude, longitude, placeName);
    }
}

/// <summary>Recherche du lieu dans la table locale, avec repli sur une saisie manuelle.</summary>
static async Task<(double Latitude, double Longitude, string Name, string? TimeZoneId)> ReadPlaceAsync(
    IPlaceRepository places,
    IBirthTimeZoneResolver zones)
{
    while (true)
    {
        System.Console.Write("Lieu de naissance (nom de ville, ou * pour saisir les coordonnées) : ");
        var query = System.Console.ReadLine()?.Trim();

        if (query is null || query == "*") return ReadManualPlace(zones);
        if (query.Length == 0) continue;

        IReadOnlyList<Place> matches;
        try
        {
            matches = await places.SearchAsync(query);
        }
        catch (FileNotFoundException ex)
        {
            System.Console.WriteLine($"  [Erreur] {ex.Message}");
            return ReadManualPlace(zones);
        }

        if (matches.Count == 0)
        {
            System.Console.WriteLine("  Aucune ville trouvée — autre orthographe, ou * pour saisir les coordonnées.");
            continue;
        }

        if (matches.Count == 1)
        {
            var only = matches[0];
            System.Console.WriteLine($"  → {only.DisplayName} — {only.Latitude:F4}°, {only.Longitude:F4}°");
            return (only.Latitude, only.Longitude, only.DisplayName, only.TimeZoneId);
        }

        for (int i = 0; i < matches.Count; i++)
            System.Console.WriteLine($"    {i + 1} — {matches[i].DisplayName} ({matches[i].Latitude:F2}°, {matches[i].Longitude:F2}°)");

        System.Console.Write($"  Choix (1-{matches.Count}, Entrée pour chercher autrement) : ");

        if (int.TryParse(System.Console.ReadLine()?.Trim(), out var index) &&
            index >= 1 && index <= matches.Count)
        {
            var chosen = matches[index - 1];
            return (chosen.Latitude, chosen.Longitude, chosen.DisplayName, chosen.TimeZoneId);
        }
    }
}

/// <summary>Saisie directe des coordonnées, pour un lieu absent de la table.</summary>
static (double Latitude, double Longitude, string Name, string? TimeZoneId) ReadManualPlace(
    IBirthTimeZoneResolver zones)
{
    var latitude  = ReadDouble("Latitude (ex: 48.8566 — positif au Nord) : ", -90, 90);
    var longitude = ReadDouble("Longitude (ex: 2.3522 — positif à l'Est) : ", -180, 180);

    System.Console.Write("Nom du lieu (facultatif) : ");
    var name = System.Console.ReadLine()?.Trim() ?? string.Empty;

    while (true)
    {
        System.Console.Write("Fuseau IANA (ex: Europe/Paris — Entrée pour saisir le décalage) : ");
        var zoneId = System.Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(zoneId))
            return (latitude, longitude, name, null);

        try
        {
            // Validation immédiate : un fuseau inconnu ferait échouer le calcul plus loin
            zones.Resolve(new DateTime(2000, 1, 1, 12, 0, 0), zoneId);
            return (latitude, longitude, name, zoneId);
        }
        catch (ArgumentException)
        {
            System.Console.WriteLine("  Fuseau inconnu (attendu : Europe/Paris, America/New_York, Indian/Reunion…)");
        }
    }
}

static TimeOnly ReadBirthTime()
{
    while (true)
    {
        System.Console.Write("Heure de naissance (HH:MM) : ");
        var raw = System.Console.ReadLine()?.Trim();

        if (TimeOnly.TryParseExact(raw, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            return time;

        System.Console.WriteLine("  Format invalide, réessaie (ex: 14:30)");
    }
}

static TimeSpan ReadUtcOffset()
{
    while (true)
    {
        System.Console.Write("Décalage UTC en heures (ex: 1, -5, 5.5) : ");
        var raw = System.Console.ReadLine()?.Trim();

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var hours) &&
            hours is >= -14 and <= 14)
            return TimeSpan.FromHours(hours);

        System.Console.WriteLine("  Nombre attendu entre -14 et 14 (point comme séparateur décimal)");
    }
}

static double ReadDouble(string label, double min, double max)
{
    while (true)
    {
        System.Console.Write(label);
        var raw = System.Console.ReadLine()?.Trim();

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
            value >= min && value <= max)
            return value;

        System.Console.WriteLine($"  Nombre décimal attendu entre {min} et {max} (point comme séparateur)");
    }
}

static string FormatOffset(TimeSpan offset)
    => $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset.Duration():hh\\:mm}";

static void WrapLine(string line, int lineWidth)
{
    var words   = line.Split(' ');
    var current = new System.Text.StringBuilder();

    foreach (var word in words)
    {
        if (current.Length == 0)
        {
            current.Append(word);
        }
        else if (current.Length + 1 + word.Length <= lineWidth)
        {
            current.Append(' ');
            current.Append(word);
        }
        else
        {
            System.Console.WriteLine(current.ToString());
            current.Clear();
            current.Append(word);
        }
    }

    if (current.Length > 0)
        System.Console.WriteLine(current.ToString());
}
