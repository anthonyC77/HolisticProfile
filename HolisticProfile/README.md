# HolisticProfile

Logiciel desktop d'analyse personnalisée multi-référentielle, assisté par un LLM local (Ollama).  
À partir d'une date de naissance, il croise plusieurs systèmes de connaissance de soi et génère une synthèse d'accompagnement thérapeutique — sans envoyer de données personnelles sur internet.

## Référentiels intégrés

| Référentiel | Statut |
|---|---|
| Numérologie Dan Millman | Phase 1 — disponible |
| Référentiel de naissance | Phase 2 — en cours |
| Design Humain | Prévu |
| Astrologie | Prévu |

## Prérequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Ollama](https://ollama.com) installé et démarré en local
- Un modèle chargé, ex : `ollama pull llama3`

## Démarrage rapide

```bash
dotnet run --project src/HolisticProfile.Console
```

La configuration se fait dans `src/HolisticProfile.Console/appsettings.json`.

## Lancer les tests

```bash
dotnet test
```

## Stack technique

.NET 9 · Clean Architecture · Ollama (LLM local) · xUnit · NSubstitute · FluentAssertions
