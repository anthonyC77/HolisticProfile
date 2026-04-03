using HolisticProfile.Console;
using HolisticProfile.Core.Interfaces;
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
System.Console.WriteLine("=== HolisticProfile — Numérologie Dan Millman ===");
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

System.Console.WriteLine();
System.Console.WriteLine("Calcul en cours...");

var synthesisService = services.GetRequiredService<ISynthesisService>();

try
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));

    var result = await synthesisService.RunAsync(birthDate, cts.Token);

    System.Console.WriteLine();
    System.Console.WriteLine($"Chemin de vie Millman : {result.Profile.MillmanLifePath}");
    System.Console.WriteLine(new string('─', 60));
    System.Console.WriteLine();
    System.Console.WriteLine(result.Text);
    System.Console.WriteLine();
    System.Console.WriteLine(new string('─', 60));
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
