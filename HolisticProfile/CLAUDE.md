# HolisticProfile

Logiciel desktop d'analyse personnalisée multi-référentielle, assistée par un LLM local (Ollama).  
Il croise plusieurs systèmes de connaissance de soi pour produire des synthèses d'accompagnement thérapeutique.

---

## Vision du projet

L'objectif est de fournir à un praticien (ou à la personne elle-même) une synthèse cohérente et personnalisée
en croisant plusieurs référentiels :

| Référentiel | Statut |
|---|---|
| Numérologie Dan Millman | **Phase 1 — implémenté** |
| Référentiel de naissance | **Phase 2 — implémenté** |
| Astrologie | **Phase 3 — implémenté** |
| Design Humain | Prévu |
| Croisement huiles essentielles | Prévu |
| Tests psychologiques / structure psy | Prévu |

Le moteur de synthèse repose sur un LLM local via **Ollama** — aucune donnée personnelle ne quitte la machine.

---

## Architecture

Clean Architecture en 3 couches :

```
src/
  HolisticProfile.Core/           — Domaine pur (modèles, interfaces, moteurs, services)
  HolisticProfile.Infrastructure/ — Implémentations techniques (fichiers, Ollama, cache, prompt)
  HolisticProfile.Console/        — Point d'entrée (DI, configuration, orchestration)

tests/
  HolisticProfile.Core.Tests/
  HolisticProfile.Infrastructure.Tests/
```

Stack : .NET 9 · xUnit · NSubstitute · FluentAssertions

---

## Phase 1 — Numérologie Dan Millman

### Principe du système

Dan Millman (auteur de *Le Guerrier Pacifique*) a développé un système de numérologie basé sur la **date de naissance complète**.
Contrairement à d'autres systèmes qui réduisent immédiatement, Millman **conserve tous les niveaux de réduction** :
chaque chiffre intermédiaire porte un sens propre.

### Calcul du chemin de vie

**Étape 1 — Somme brute**  
On additionne **tous les chiffres** de la date au format `JJMMAAAA`.

> Exemple : 14/05/1983 → 1+4+0+5+1+9+8+3 = **31**

**Étape 2 — Première réduction**  
On réduit la somme brute en additionnant ses chiffres.

> 31 → 3+1 = **4** → chemin 2 niveaux : **31/4**

Si le résultat est encore ≥ 10, on réduit une deuxième fois :

> Exemple : 29/11/1975 → 2+9+1+1+1+9+7+5 = **35** → 3+5 = **8** → chemin 2 niveaux : **35/8**  
> Autre exemple : 07/02/1968 → 0+7+0+2+1+9+6+8 = **33** → 3+3 = **6** → chemin 2 niveaux : **33/6**

**Chemin à 3 niveaux** — quand la première réduction est elle-même ≥ 10 :

> Exemple : 28/05/1975 → 2+8+0+5+1+9+7+5 = **37** → 3+7 = **10** → 1+0 = **1** → chemin 3 niveaux : **37/10/1**  
> Exemple classique : 29/11/2 (somme=29, intermédiaire=11, racine=2)

### Notation

| Notation | Signification |
|---|---|
| `34/7` | somme brute 34, racine 7 |
| `29/11/2` | somme brute 29, intermédiaire 11, racine 2 |

**Important :** la notation ne s'inverse jamais. `34/7` n'est pas `7/34`.

### Lecture des chiffres

Chaque chiffre constitutif du chemin a un sens et un poids :

| Chiffre | Thème principal |
|---|---|
| 1 | Créativité, initiative, insécurité créatrice |
| 2 | Coopération, relation, service |
| 3 | Expression, communication, dispersion |
| 4 | Stabilité, processus, rigueur, corps |
| 5 | Liberté, expérience, discipline |
| 6 | Vision, idéalisme, perfectionnisme |
| 7 | Foi, introspection, quête de sens |
| 8 | Pouvoir, abondance, autorité |
| 9 | Intégrité, sagesse, leadership spirituel |

**Nombres maîtres** (11, 22, 33) : intensité doublée, double défi, demandent une gestion consciente.  
Un chiffre répété dans le chemin (ex : double 1 dans le 11) porte un **double poids et un double défi**.

### Pipeline technique

```
DateTime (date de naissance)
    │
    ▼
MillmanCalculationEngine     → MillmanLifePath (sum, intermediateSum?, root)
    │
    ▼
FileKnowledgeBaseRepository  → contenu markdown (knowledge_base/millman/paths/path_XX_YY.md)
    │
    ▼
MillmanPromptBuilder         → prompt structuré pour le LLM
    │
    ▼
OllamaClient                 → appel HTTP local au modèle
    │
    ▼
FileSynthesisCacheRepository → mise en cache (évite les appels LLM redondants)
    │
    ▼
SynthesisResult              → synthèse texte personnalisée
```

### Base de connaissances

Les fiches sont des fichiers Markdown dans `knowledge_base/millman/paths/`.  
Convention de nommage : `path_{clé}.md`

| Fichier | Chemin |
|---|---|
| `path_34_7.md` | 34/7 — Le Processus et la Foi |
| `path_29_11_2.md` | 29/11/2 — L'Énergie Créatrice et la Coopération |
| `path_40_4.md` | 40/4 — À compléter |

Chaque fiche suit une structure standardisée :
- Thème central
- Chiffres constitutifs et leur poids
- Forces
- Défis récurrents
- Axe de travail thérapeutique
- Leviers d'action
- Note clinique

---

## Lieu de naissance et fuseau horaire (thème natal)

Le décalage UTC conditionne directement l'Ascendant : **une heure d'erreur déplace l'ASC d'environ 15°**,
soit souvent une maison entière. Il n'est donc jamais demandé au praticien — il est déduit du lieu.

### Table des lieux

`data/places/cities.json` — 213 lieux (128 en France, préfectures + grandes villes + DOM-TOM,
plus les principales villes du monde). Chaque entrée porte le nom, la région, le pays,
les coordonnées décimales et l'**identifiant IANA du fuseau** (ex : `Europe/Paris`).

Le fichier est copié à côté de l'exécutable (`data/places/cities.json`) et chargé par défaut depuis là.
`Places:FilePath` dans `appsettings.json` permet de pointer une autre table.

La recherche (`JsonPlaceRepository`) ignore casse, accents et tirets, et comprend `st` pour `saint`
(« st etienne » → Saint-Étienne). Ordre : nom exact, début de nom, début d'un mot, contenu, région/pays.

**Ajouter une ville** : une ligne dans le JSON. Les tests `CitiesDataTests` vérifient que chaque fuseau
existe dans la TZDB, que les coordonnées sont valides et qu'il n'y a pas de doublon.

### Résolution du décalage

`NodaTimeZoneResolver` s'appuie sur la base **TZDB embarquée dans NodaTime**, et non sur `TimeZoneInfo` :
Windows ne conserve pas les règles anciennes et donnerait un décalage faux pour la plupart des naissances
avant les années 2000. Sont ainsi correctement restitués :

- l'absence d'heure d'été en France entre 1946 et 1975 (réintroduite le 28 mars 1976) ;
- l'heure du méridien de Paris (+0h09'21") avant le 11 mars 1911 ;
- les règles propres à chaque pays (l'heure d'été US commençait en avril jusqu'en 2006, etc.).

Deux cas limites sont signalés au praticien plutôt que tranchés en silence :

| Cas | Situation | Comportement |
|---|---|---|
| `Ambiguous` | Heure vécue deux fois (retour à l'heure d'hiver) | Les deux décalages sont proposés, le premier par défaut |
| `Skipped` | Heure inexistante (passage à l'heure d'été) | Alerte + proposition de ressaisir l'heure |

Si le lieu n'est pas dans la table, `*` ouvre la saisie manuelle : latitude, longitude,
puis fuseau IANA (validé immédiatement) ou décalage brut.

Le décalage retenu fait partie de la clé de cache (`NatalChartInput.CacheKey`) : deux décalages
donnent deux thèmes différents, qui ne doivent pas se confondre en cache.

---

## Lancer le projet

**Prérequis**
- .NET 9 SDK
- [Ollama](https://ollama.com) installé et démarré localement
- Un modèle chargé dans Ollama (ex : `ollama pull llama3`)

**Configuration** (`appsettings.json`)
```json
{
  "Ollama": { "BaseUrl": "http://localhost:11434", "Model": "llama3" },
  "KnowledgeBase": { "BasePath": "knowledge_base/millman/paths" },
  "SynthesisCache": { "BasePath": "cache/syntheses" }
}
```

**Exécution**
```bash
dotnet run --project src/HolisticProfile.Console
```

**Tests**
```bash
dotnet test
```
