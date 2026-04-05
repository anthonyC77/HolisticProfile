using HolisticProfile.Core.Engines;
using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Services;
using HolisticProfile.Infrastructure.Cache;
using HolisticProfile.Infrastructure.KnowledgeBase;
using HolisticProfile.Infrastructure.LlmClients;
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

        return services;
    }
}
