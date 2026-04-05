# HolisticProfile — Spécification Projet

## Projet : Système Expert Multi-Référentiel de Connaissance de Soi

> Document de synthèse à utiliser comme brief projet dans Claude Code.
> Généré à partir des échanges de conception entre le porteur de projet et Claude.

---

## 1. Vision du projet

### Objectif
Créer un **logiciel desktop local (client lourd C#)** d'analyse multi-référentielle de la personne, à visée d'accompagnement thérapeutique. L'outil croise plusieurs systèmes symboliques de connaissance de soi (astrologie, Human Design, numérologie Dan Millman, référentiel de naissance, Gene Keys, Ba Zi, etc.) pour produire une synthèse personnalisée unique.

### Philosophie
- **Point de départ d'une thérapie** : se connaître pour se transformer et trouver les leviers d'action
- **Aucun logiciel sur le marché** ne fait d'analyse croisée multi-systèmes — c'est un produit unique
- L'intelligence analytique vient du **praticien** (règles de corrélation, observations cliniques) ; le LLM est la couche de communication qui rend l'analyse accessible en langage naturel

### Propriétaire du projet
- Développeur C# expérimenté
- Praticien formé à plusieurs systèmes de connaissance de soi
- L'outil est conçu pour un usage professionnel personnel (cabinet thérapeutique)

---

## 2. Exigences de confidentialité

### Principe fondamental
**Confidentialité totale des données patients** — aucune donnée identifiante ne doit quitter la machine.

### Architecture hybride local/cloud

```
┌──────────────────────────────────────────────────────┐
│                    COUCHE LOCALE                      │
│              (100% sur la machine)                    │
│                                                      │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐ │
│  │  Données    │  │  Moteurs de  │  │  LLM Local  │ │
│  │  Patients   │  │  Calcul      │  │  (Synthèse) │ │
│  │  (SQLite/   │  │  (C#)        │  │  (Ollama)   │ │
│  │   LiteDB)   │  │              │  │             │ │
│  └─────────────┘  └──────────────┘  └─────────────┘ │
│         │                │                  │        │
│         └────────────────┼──────────────────┘        │
│                          │                           │
│              Synthèse personnalisée                  │
│              (identité + interprétations)             │
└──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────┐
│               COUCHE DISTANTE (optionnelle)           │
│            (RAG / API Claude — anonymisée)            │
│                                                      │
│  Requêtes anonymes uniquement :                      │
│  "Que signifie Saturne en Maison 7 ?"               │
│  "Décris le profil 3/5 en Human Design"              │
│  → Aucune donnée identifiante ne transite            │
└──────────────────────────────────────────────────────┘
```

### Stratégie d'anonymisation des appels distants

Si la couche distante (RAG cloud ou API) est utilisée :

1. **Cache exhaustif** (stratégie recommandée) :
   - Interroger le RAG sur TOUTES les combinaisons possibles en un batch initial
   - Stocker les réponses en cache local
   - Plus aucune connexion nécessaire en usage quotidien
   - Combinaisons finies : ~120 planètes/signes + ~120 planètes/maisons + 12 profils HD + 5 types + 37 chemins Millman + 384 portes/lignes = quelques milliers d'entrées

2. **Requêtes bruitées** (si requêtes ponctuelles nécessaires) :
   - Pour chaque vraie requête, envoyer N fausses requêtes plausibles (ratio 1:5)
   - Décalage temporel aléatoire (30 sec à 10 min entre requêtes)
   - Mélanger les requêtes de plusieurs patients dans la file
   - Pré-calculer les thèmes en avance (pas en temps réel pendant la consultation)

3. **Masquage réseau** :
   - VPN tournant ou proxy
   - Alternance de clés API
   - Possibilité de couper totalement le réseau (tout fonctionne en local)

---

## 3. Architecture technique

### Stack technologique

| Composant | Technologie | Rôle |
|-----------|-------------|------|
| **Langage** | C# / .NET 8+ | Développement principal |
| **Interface** | WPF ou MAUI ou Avalonia | Client lourd desktop |
| **BDD patients** | SQLite ou LiteDB | Données dynamiques (dossiers patients) |
| **Base de connaissances** | Fichiers Markdown | Savoirs stables (interprétations) |
| **LLM local** | Ollama (Mistral / Llama 3 / Qwen 2.5) | Rédaction de synthèses en langage naturel |
| **Embeddings locaux** | nomic-embed-text via Ollama | Pour RAG local (phase ultérieure) |
| **Orchestration RAG** | Semantic Kernel (Microsoft) | Intégration C# native (phase ultérieure) |
| **Éphémérides** | Swiss Ephemeris | Calculs astronomiques (astro, HD, Gene Keys) |

### Matériel minimum
- PC avec 16 Go de RAM
- GPU avec 8+ Go VRAM (idéal, pas obligatoire)
- Stockage SSD pour les modèles (~5-10 Go par modèle LLM)

### Communication avec Ollama

```csharp
// Appel HTTP local — aucune donnée ne sort de la machine
var response = await httpClient.PostAsJsonAsync(
    "http://localhost:11434/api/generate",
    new { model = "mistral", prompt = contexteComplet }
);
```

---

## 4. Structure du projet

### Organisation des fichiers

```
HolisticProfile/
├── src/
│   ├── HolisticProfile.Core/          # Bibliothèque principale
│   │   ├── Models/
│   │   │   ├── Patient.cs              # Modèle patient
│   │   │   ├── BirthData.cs            # Données de naissance
│   │   │   ├── AstroChart.cs           # Thème astrologique
│   │   │   ├── HumanDesignChart.cs     # Charte HD
│   │   │   ├── MillmanProfile.cs       # Profil numérologique
│   │   │   ├── ReferentielChart.cs     # Référentiel de naissance
│   │   │   ├── GeneKeysProfile.cs      # Profil Gene Keys
│   │   │   ├── BaZiChart.cs            # Quatre Piliers
│   │   │   └── IntegratedProfile.cs    # Profil intégré multi-systèmes
│   │   │
│   │   ├── Engines/
│   │   │   ├── AstrologyEngine.cs      # Calcul thème natal
│   │   │   ├── HumanDesignEngine.cs    # Calcul charte HD
│   │   │   ├── MillmanEngine.cs        # Calcul chemin de vie Millman
│   │   │   ├── ReferentielEngine.cs    # Calcul référentiel de naissance
│   │   │   ├── GeneKeysEngine.cs       # Calcul profil hologénétique
│   │   │   └── BaZiEngine.cs           # Calcul 4 piliers
│   │   │
│   │   ├── Correlation/
│   │   │   ├── ThemeDetector.cs        # Détection thèmes récurrents inter-systèmes
│   │   │   ├── CorrelationTable.cs     # Tables de correspondance croisée
│   │   │   ├── TensionAnalyzer.cs      # Tensions/contradictions entre systèmes
│   │   │   └── ThematicMap.cs          # Carte thématique structurée
│   │   │
│   │   ├── Knowledge/
│   │   │   ├── KnowledgeLoader.cs      # Chargement fichiers Markdown
│   │   │   ├── ContextBuilder.cs       # Construction du contexte pour le LLM
│   │   │   └── CacheManager.cs         # Gestion du cache de réponses
│   │   │
│   │   └── LLM/
│   │       ├── OllamaClient.cs         # Client HTTP pour Ollama local
│   │       ├── PromptBuilder.cs        # Construction des prompts
│   │       └── CloudRAGClient.cs       # Client pour requêtes anonymisées (optionnel)
│   │
│   ├── HolisticProfile.Data/           # Couche données
│   │   ├── PatientRepository.cs
│   │   ├── SessionRepository.cs
│   │   └── DbContext.cs
│   │
│   └── HolisticProfile.UI/            # Interface utilisateur (WPF/MAUI/Avalonia)
│       ├── Views/
│       │   ├── PatientFormView         # Saisie données patient
│       │   ├── ProfileDashboardView    # Vue synthétique multi-systèmes
│       │   ├── SystemDetailView        # Vue détaillée par système
│       │   ├── CorrelationView         # Vue des recoupements
│       │   ├── SessionNotesView        # Notes de séance
│       │   └── ChatView               # Dialogue avec le LLM
│       └── ViewModels/
│
├── knowledge_base/                     # BASE DE CONNAISSANCES (Markdown)
│   ├── astrology/
│   │   ├── planets_in_signs/
│   │   │   ├── sun_aries.md
│   │   │   ├── sun_taurus.md
│   │   │   └── ... (120 fichiers : 10 planètes × 12 signes)
│   │   ├── planets_in_houses/
│   │   │   └── ... (120 fichiers : 10 planètes × 12 maisons)
│   │   ├── aspects/
│   │   │   └── ... (conjonction, opposition, trigone, carré, sextile)
│   │   └── synthesis_rules.md
│   │
│   ├── human_design/
│   │   ├── types/
│   │   │   ├── manifestor.md
│   │   │   ├── generator.md
│   │   │   ├── manifesting_generator.md
│   │   │   ├── projector.md
│   │   │   └── reflector.md
│   │   ├── authorities/
│   │   ├── profiles/                   # 12 profils (1/3, 1/4, ... 6/2, 6/3)
│   │   ├── centers/                    # 9 centres (définis/ouverts)
│   │   ├── gates/                      # 64 portes × 6 lignes
│   │   └── channels/                   # 36 canaux
│   │
│   ├── millman/
│   │   ├── paths/
│   │   │   ├── path_19_10_1.md
│   │   │   ├── path_20_2.md
│   │   │   ├── path_33_6.md            # 😉
│   │   │   └── ... (~37 chemins de vie)
│   │   └── synthesis.md
│   │
│   ├── referentiel/
│   │   ├── arcanes/                    # 22 arcanes majeurs
│   │   ├── positions/                  # 14 positions/maisons
│   │   └── combinations/
│   │
│   ├── gene_keys/
│   │   ├── keys/                       # 64 clés (ombre/don/siddhi)
│   │   ├── sequences/                  # Activation, Vénus, Perle
│   │   └── lines/                      # 6 lignes par clé
│   │
│   ├── bazi/
│   │   ├── day_masters/                # 10 Maîtres du Jour
│   │   ├── elements_interactions/      # 5 éléments × interactions
│   │   ├── twelve_officers/
│   │   └── luck_pillars/
│   │
│   ├── correlation/                    # TABLES DE CORRESPONDANCE CROISÉE
│   │   ├── hd_gates_to_planets.md      # Ex: Porte 28 ↔ Pluton
│   │   ├── millman_to_arcanes.md       # Ex: 33/6 ↔ Arcane VI
│   │   ├── gene_keys_to_hd.md
│   │   ├── bazi_to_astro.md
│   │   └── thematic_clusters.md        # Regroupements thématiques trans-systèmes
│   │
│   └── clinical_notes/                 # OBSERVATIONS CLINIQUES PERSONNELLES
│       ├── patterns_observed.md
│       └── case_studies.md
│
├── data/
│   ├── ephemeris/                      # Fichiers Swiss Ephemeris (locaux)
│   ├── iching_mapping.json             # Table degrés zodiacaux → portes I-Ching
│   ├── sexagenary_calendar.json        # Calendrier sexagésimal (Ba Zi)
│   └── patients.db                     # Base SQLite patients
│
├── cache/
│   └── rag_responses/                  # Cache des réponses RAG pré-fetchées
│
└── tests/
    ├── MillmanEngineTests.cs
    ├── AstrologyEngineTests.cs
    └── ...
```

---

## 5. Systèmes de connaissance à intégrer

### Phase 1 — MVP (Numérologie Dan Millman)
**Priorité : valider toute la chaîne de bout en bout**

- **Calcul** : Addition et réduction des chiffres de la date de naissance
- **Corpus** : ~37 chemins de vie (un seul livre source : "The Life You Were Born to Live")
- **Complexité** : Faible — calcul trivial, base de connaissances contenue
- **Données nécessaires** : Date de naissance uniquement

### Phase 2 — Référentiel de naissance
- **Calcul** : 14 positions d'arcanes majeurs du Tarot de Marseille, obtenues par réductions théosophiques de la date de naissance
- **Corpus** : 22 arcanes majeurs × 14 positions + combinaisons
- **Données nécessaires** : Date de naissance
- **Note** : Le Référentiel prend son sens en dialogue avec la personne (le vécu). L'outil fournit la grille, le sens vient de la séance.

### Phase 3 — Astrologie occidentale
- **Calcul** : Positions planétaires via Swiss Ephemeris, calcul des maisons (Placidus ou Koch), aspects
- **Bibliothèques** : Swiss Ephemeris (fichiers éphémérides locaux)
- **Corpus** : Planètes en signes (120), planètes en maisons (120), aspects (dizaines de combinaisons)
- **Données nécessaires** : Date, heure ET lieu de naissance

### Phase 4 — Human Design
- **Calcul** : Deux thèmes astrologiques (naissance + ~88 jours solaires avant) → conversion en portes/lignes du I-Ching
- **Table de correspondance** : Degrés zodiacaux → 64 portes × 6 lignes
- **Dérivation** : Type, Stratégie, Autorité, Profil, Centres définis/ouverts, Canaux
- **Données nécessaires** : Date, heure ET lieu de naissance

### Phase 5 — Gene Keys (Clés Génétiques)
- **Calcul** : Quasi-identique au HD (mêmes positions planétaires → 64 clés)
- **Enrichissement** : Spectre Ombre → Don → Siddhi pour chaque clé
- **Séquences** : Activation (mission), Vénus (relations), Perle (prospérité)
- **Corpus** : 64 clés × 3 fréquences × 6 lignes + descriptions des séquences
- **Données nécessaires** : Date, heure ET lieu de naissance
- **Note** : Coût de développement faible si le moteur HD est déjà fait

### Phase 6 — Ba Zi (Quatre Piliers de la Destinée)
- **Calcul** : Conversion date/heure de naissance via calendrier sexagésimal chinois
- **Structure** : 4 piliers (année, mois, jour, heure) × 2 caractères (tronc céleste + branche terrestre)
- **Analyse** : Maître du Jour, 5 éléments, équilibre yin/yang, périodes de chance (Da Yun)
- **Données nécessaires** : Date et heure de naissance

### Phases futures possibles
- **Astrologie védique (Jyotish)** : Zodiaque sidéral, Nakshatras, Dashas
- **Thème de cristal** : À documenter
- **Astrologie maya / Tzolkin** : 260 jours, 20 glyphes × 13 tons
- **Numérologie pythagoricienne/kabbalistique** : Utilise aussi le nom complet
- **Zi Wei Dou Shu** : Astrologie Pourpre chinoise (12 palais, 100+ étoiles)

---

## 6. Le moteur de corrélation (intelligence propriétaire)

### Principe
C'est le cœur de la valeur du logiciel. Le LLM ne fait pas l'analyse croisée — c'est le **moteur de corrélation codé en C#** qui détecte les patterns, et le LLM rédige la synthèse.

### Flux

```
Étape 1 : Calcul brut (tous les systèmes activés)
    ↓
Étape 2 : Moteur de corrélation (C#, règles codées par le praticien)
    → Détecte les thèmes récurrents inter-systèmes
    → Identifie les tensions/contradictions entre systèmes
    → Pondère selon les critères cliniques du praticien
    → Produit une "carte thématique" structurée
    ↓
Étape 3 : Le LLM reçoit la carte déjà analysée
    → Il rédige, met en forme, rend accessible
    → L'intelligence de l'analyse vient du moteur C#
```

### Tables de correspondance croisée (exemples)

```
Porte HD 28     ↔  Pluton        → Transformation profonde
Porte HD 01     ↔  Soleil        → Expression créative
Chemin 33/6     ↔  Arcane VI     → Responsabilité / choix
Saturne         ↔  Portes 38,54,58 HD → Structuration
Gene Key 28     ↔  Porte HD 28   → Même hexagramme, spectre Ombre/Don/Siddhi
Élément Eau BZ  ↔  Lune/Scorpion → Profondeur émotionnelle
```

> **Ces tables constituent la propriété intellectuelle du praticien** — construites à partir de sa formation et de son expérience clinique. Elles restent 100% locales.

---

## 7. Base de connaissances

### Sources légitimes pour alimenter la base
1. **Livres achetés** par le praticien (versions numériques idéales) — usage local privé professionnel
2. **Notes de formation** personnelles du praticien
3. **Observations cliniques** accumulées en pratique
4. **Données factuelles** non protégeables (tables astronomiques, méthodes de calcul, correspondances I-Ching)
5. **Contenus en domaine public** (astrologie traditionnelle, I-Ching, Tarot de Marseille comme système symbolique)

### Format
- Fichiers **Markdown** pour les connaissances stables (interprétations)
- Chaque fichier traite d'un élément atomique (ex: une planète dans un signe, un chemin de vie)
- Enrichissement progressif avec les observations cliniques personnelles

### Stratégie de construction
1. Commencer par le système le mieux maîtrisé
2. Rédiger les fiches à partir de sa propre compréhension (plus pertinent qu'un copier-coller)
3. Le LLM fait un excellent travail de synthèse même avec une base modeste
4. La base s'enrichit au fil de la pratique clinique → le logiciel devient de plus en plus fin

---

## 8. Données patients (BDD)

### Modèle de données

```
Patient
├── Id (GUID)
├── Nom / Prénom (chiffré)
├── DateNaissance
├── HeureNaissance
├── LieuNaissance
├── Profils calculés
│   ├── ThèmeNatal (astro)
│   ├── CharteHD
│   ├── CheminMillman
│   ├── RéférentielNaissance
│   ├── ProfilGeneKeys
│   └── ThèmeBaZi
├── CarteThématique (résultat du moteur de corrélation)
├── Sessions[]
│   ├── Date
│   ├── Notes du praticien
│   ├── Axes de travail identifiés
│   ├── Synthèse LLM générée
│   └── Observations cliniques
└── ÉvolutionSuivi[]
```

### Sécurité
- Base SQLite/LiteDB chiffrée sur disque
- Aucune synchronisation cloud
- Possibilité de fonctionner réseau totalement coupé

---

## 9. Flux utilisateur type

```
1. Le praticien crée un dossier patient
   → Saisie : "Marie Dupont, née le 15 mars 1987 à 14h30 à Lyon"

2. Tous les moteurs calculent en parallèle
   → Astro : Soleil Poissons Maison 10, Lune Scorpion Maison 5...
   → HD : Générateur Pur, Autorité Sacrale, Profil 3/5, Portes 28, 36...
   → Millman : Chemin 34/7
   → Référentiel : Arcane XIII position 1, Arcane VIII position 2...
   → Gene Keys : Clé 28 (Ombre: Sans But → Don: Totalité → Siddhi: Immortalité)
   → Ba Zi : Maître du Jour Feu Yin, élément dominant Eau...

3. Le moteur de corrélation analyse
   → Thème récurrent : "transformation" (Arcane XIII + Pluton + Porte 28 + Gene Key 28)
   → Tension : HD dit "attendre" (Générateur) mais Astro montre une énergie Mars forte
   → Axe prioritaire identifié : rapport au changement / peur de la mort symbolique

4. Le LLM local reçoit la carte thématique + les interprétations pertinentes de la knowledge base
   → Rédige une synthèse en langage naturel
   → Le praticien peut dialoguer avec le LLM pour approfondir ("Détaille la tension Mars/Sacral")

5. En séance, le praticien utilise la synthèse comme support de dialogue
   → Il ajoute ses notes cliniques
   → Les observations enrichissent la base pour les futurs patients
```

---

## 10. Plan de développement

### Phase 0 — Environnement (prérequis)
- [ ] Installer Ollama + télécharger un modèle (ex: `ollama pull mistral`)
- [ ] Installer .NET 8+ SDK
- [ ] Télécharger le modèle d'embeddings : `ollama pull nomic-embed-text`
- [ ] Vérifier : `ollama run mistral "Bonjour"` → doit répondre

### Phase 1 — MVP Millman (valider la chaîne complète)
- [ ] Structure solution C#
- [ ] `MillmanEngine.cs` : calcul du chemin de vie
- [ ] `OllamaClient.cs` : appel HTTP à Ollama local
- [ ] `KnowledgeLoader.cs` : chargement des fichiers Markdown
- [ ] `ContextBuilder.cs` : construction du prompt (profil calculé + interprétation)
- [ ] Interface console ou minimale pour tester
- [ ] Quelques fichiers Markdown de test (2-3 chemins de vie)
- [ ] **Test end-to-end** : date → calcul → contexte → réponse LLM

### Phase 2 — Référentiel de naissance
- [ ] `ReferentielEngine.cs` : calcul des 14 positions
- [ ] Knowledge base : 22 arcanes × 14 positions
- [ ] Intégration dans le flux

### Phase 3 — Interface utilisateur
- [ ] Choisir WPF / MAUI / Avalonia
- [ ] Formulaire de saisie patient
- [ ] Dashboard de visualisation multi-systèmes
- [ ] Vue de dialogue avec le LLM

### Phase 4 — Astrologie + HD + Gene Keys
- [ ] Intégration Swiss Ephemeris (bindings C# ou appel natif)
- [ ] `AstrologyEngine.cs`
- [ ] `HumanDesignEngine.cs` + table degrés → portes I-Ching
- [ ] `GeneKeysEngine.cs` (réutilise le moteur HD)
- [ ] Knowledge bases respectives

### Phase 5 — Ba Zi
- [ ] `BaZiEngine.cs` + calendrier sexagésimal
- [ ] Knowledge base

### Phase 6 — Moteur de corrélation
- [ ] `ThemeDetector.cs` : détection patterns inter-systèmes
- [ ] `CorrelationTable.cs` : tables de correspondance éditables
- [ ] `TensionAnalyzer.cs` : analyse des contradictions
- [ ] `ThematicMap.cs` : génération de la carte thématique

### Phase 7 — RAG avancé (si nécessaire)
- [ ] Intégration Semantic Kernel
- [ ] Embeddings locaux des fichiers Markdown
- [ ] Recherche vectorielle pour les grandes bases de connaissances
- [ ] Cache exhaustif des réponses cloud (optionnel)

### Phase 8 — Enrichissement continu
- [ ] Interface de saisie des notes cliniques
- [ ] Export/backup des données
- [ ] Historique de suivi patient
- [ ] Statistiques sur les corrélations observées

---

## 11. Notes techniques importantes

### Approche pragmatique pour le RAG
Commencer **SANS RAG** : les fenêtres de contexte actuelles (128k tokens pour certains modèles) permettent d'injecter énormément de contenu directement dans le prompt. Le RAG devient utile uniquement quand la base de connaissances dépasse la taille du contexte.

### Éphémérides Swiss Ephemeris
- Fichiers téléchargeables gratuitement, fonctionnent 100% hors ligne
- Couvrent les calculs nécessaires pour : astrologie occidentale, Human Design, Gene Keys
- Binding C# disponible (ou appel en ligne de commande)

### Calendrier sexagésimal (Ba Zi)
- Tables de conversion disponibles, purement algorithmiques
- Pas besoin d'éphémérides astronomiques (basé sur le calendrier, pas les planètes)

### Modèles LLM recommandés
- **Mistral 7B** : bon rapport qualité/performance, tourne sur 16 Go RAM
- **Llama 3 8B** : très bon en instruction following
- **Qwen 2.5 14B** : plus fin, nécessite plus de VRAM
- **Mistral Small / Llama 3 70B** : pour une machine puissante, résultats nettement meilleurs

---

## 12. Exemple de prompt type pour le LLM

```
Tu es un assistant spécialisé en accompagnement thérapeutique multi-référentiel.
Tu croises les systèmes suivants pour produire une synthèse de la personne.

## Profil calculé

### Numérologie Dan Millman
Chemin de vie : 34/7
[contenu du fichier path_34_7.md]

### Human Design
Type : Générateur Pur
Autorité : Sacrale
Profil : 3/5
Centres définis : Sacral, Racine, Rate
Portes principales : 28 (ligne 4), 36 (ligne 2)
[contenu des fichiers pertinents]

### Astrologie
Soleil : Poissons, Maison 10
Lune : Scorpion, Maison 5
[etc.]

### Carte thématique (générée par le moteur de corrélation)
- Thème dominant : transformation / renaissance
  Sources : Arcane XIII + Pluton Maison 8 + Porte 28 + Gene Key 28 Ombre→Don
- Tension identifiée : besoin d'attendre (Générateur) vs énergie Mars/Bélier impulsive
- Axe de travail suggéré : apprivoiser le rythme de la réponse sacrale

## Consigne
Produis une synthèse accessible, chaleureuse, qui met en lumière les fils rouges 
entre les systèmes. Identifie les points de levier pour un travail de transformation. 
Évite le jargon technique sauf si contextualisé.
```

---

*Ce document est conçu pour être utilisé comme référence dans Claude Code. Chaque section peut être pointée individuellement pour guider le développement étape par étape.*