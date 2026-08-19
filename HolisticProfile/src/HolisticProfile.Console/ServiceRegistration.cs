using HolisticProfile.Core.Engines;
using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Services;
using HolisticProfile.Infrastructure.Cache;
using HolisticProfile.Infrastructure.Engines;
using HolisticProfile.Infrastructure.KnowledgeBase;
using HolisticProfile.Infrastructure.LlmClients;
using HolisticProfile.Infrastructure.Places;
using HolisticProfile.Infrastructure.Prompt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace HolisticProfile.Console;

public static class ServiceRegistration
{
    public static IServiceCollection AddHolisticProfile(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<OllamaOptions>(config.GetSection("Ollama"));
        services.Configure<KnowledgeBaseOptions>(config.GetSection("KnowledgeBase"));
        services.Configure<SynthesisCacheOptions>(config.GetSection("SynthesisCache"));

        services.AddSingleton<ICalculationEngine, MillmanCalculationEngine>();
        services.AddSingleton<IPromptBuilder, MillmanPromptBuilder>();

        services.AddSingleton<IKnowledgeBaseRepository>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<KnowledgeBaseOptions>>().Value;
            return new FileKnowledgeBaseRepository(opts.BasePath);
        });

        // Client HTTP nommé avec timeout généreux pour les LLM locaux
        services.AddHttpClient("ollama", (sp, http) =>
        {
            var opts = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
            http.BaseAddress = new Uri(opts.BaseUrl);
            http.Timeout     = TimeSpan.FromMinutes(3);
        });

        services.AddTransient<ILlmClient>(sp =>
        {
            var opts    = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new OllamaClient(factory.CreateClient("ollama"), opts.Model);
        });

        services.AddSingleton<ISynthesisCacheRepository>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<SynthesisCacheOptions>>().Value;
            return new FileSynthesisCacheRepository(opts.BasePath);
        });

        services.AddTransient<ISynthesisService, SynthesisService>();

        // --- Référentiel de Naissance ---
        services.Configure<ReferentielKnowledgeBaseOptions>(config.GetSection("ReferentielKnowledgeBase"));
        services.Configure<ReferentielSynthesisCacheOptions>(config.GetSection("ReferentielSynthesisCache"));

        services.AddSingleton<IReferentielCalculationEngine, ReferentielCalculationEngine>();
        services.AddSingleton<IReferentielPromptBuilder, ReferentielPromptBuilder>();

        services.AddSingleton<IReferentielKnowledgeBaseRepository>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<ReferentielKnowledgeBaseOptions>>().Value;
            return new ReferentielKnowledgeBaseRepository(opts.BasePath);
        });

        services.AddSingleton<IReferentielSynthesisCacheRepository>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<ReferentielSynthesisCacheOptions>>().Value;
            return new ReferentielSynthesisCacheRepository(opts.BasePath);
        });

        services.AddTransient<IReferentielSynthesisService, ReferentielSynthesisService>();

        // --- Astrologie ---
        services.Configure<AstrologyKnowledgeBaseOptions>(config.GetSection("AstrologyKnowledgeBase"));
        services.Configure<AstrologySynthesisCacheOptions>(config.GetSection("AstrologySynthesisCache"));

        services.AddSingleton<IAstrologyCalculationEngine>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<AstrologyKnowledgeBaseOptions>>().Value;
            // EphemerisPath optionnel : si vide, Moshier est utilisé (aucun fichier requis)
            var ephPath = config["Astrology:EphemerisPath"];
            return new SwissEphAstrologyCalculationEngine(ephPath);
        });

        services.AddSingleton<IAstrologyPromptBuilder, AstrologyPromptBuilder>();

        services.AddSingleton<IAstrologyKnowledgeBaseRepository>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<AstrologyKnowledgeBaseOptions>>().Value;
            return new AstrologyKnowledgeBaseRepository(opts.BasePath);
        });

        services.AddSingleton<IAstrologySynthesisCacheRepository>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<AstrologySynthesisCacheOptions>>().Value;
            return new AstrologySynthesisCacheRepository(opts.BasePath);
        });

        services.AddTransient<IAstrologySynthesisService, AstrologySynthesisService>();

        // --- Lieux de naissance & fuseaux horaires ---
        services.Configure<PlaceOptions>(config.GetSection("Places"));

        services.AddSingleton<IPlaceRepository>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<PlaceOptions>>().Value;

            // Par défaut : la table livrée avec l'application (copiée à côté de l'exécutable)
            var path = string.IsNullOrWhiteSpace(opts.FilePath)
                ? Path.Combine(AppContext.BaseDirectory, "data", "places", "cities.json")
                : opts.FilePath;

            return new JsonPlaceRepository(path);
        });

        services.AddSingleton<IBirthTimeZoneResolver>(_ => new NodaTimeZoneResolver());

        return services;
    }
}
