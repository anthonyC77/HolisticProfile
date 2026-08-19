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
    natalInput = ReadNatalChartInput(birthDate);

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

static NatalChartInput ReadNatalChartInput(DateTime birthDate)
{
    // Heure
    TimeOnly birthTime = default;
    while (true)
    {
        System.Console.Write("Heure de naissance (HH:MM) : ");
        var raw = System.Console.ReadLine()?.Trim();
        if (TimeOnly.TryParseExact(raw, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthTime))
            break;
        System.Console.WriteLine("  Format invalide, réessaie (ex: 14:30)");
    }

    // Décalage UTC
    int utcOffset = 0;
    while (true)
    {
        System.Console.Write("Décalage UTC en heures (ex: 1 pour Paris hiver, 2 pour Paris été) : ");
        var raw = System.Console.ReadLine()?.Trim();
        if (int.TryParse(raw, out utcOffset)) break;
        System.Console.WriteLine("  Entier attendu (ex: 1, -5, 0)");
    }

    // Latitude
    double latitude = 0;
    while (true)
    {
        System.Console.Write("Latitude du lieu de naissance (ex: 48.8566 pour Paris) : ");
        var raw = System.Console.ReadLine()?.Trim();
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude)) break;
        System.Console.WriteLine("  Nombre décimal attendu (utiliser le point comme séparateur)");
    }

    // Longitude
    double longitude = 0;
    while (true)
    {
        System.Console.Write("Longitude du lieu de naissance (ex: 2.3522 pour Paris) : ");
        var raw = System.Console.ReadLine()?.Trim();
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude)) break;
        System.Console.WriteLine("  Nombre décimal attendu (utiliser le point comme séparateur)");
    }

    System.Console.Write("Nom du lieu (facultatif, appuie sur Entrée pour sauter) : ");
    var placeName = System.Console.ReadLine()?.Trim() ?? string.Empty;

    var birthDateTime = new DateTime(
        birthDate.Year, birthDate.Month, birthDate.Day,
        birthTime.Hour, birthTime.Minute, 0);

    return new NatalChartInput(
        birthDateTime,
        TimeSpan.FromHours(utcOffset),
        latitude,
        longitude,
        placeName);
}

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
