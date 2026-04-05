# Référentiel de Naissance — Spécification Algorithme

> Document technique pour l'implémentation du `ReferentielEngine.cs`
> Basé sur la méthode de Georges Colleuil

---

## 1. Principe général

Le Référentiel de Naissance est une grille de **14 maisons** (aussi appelées "zones"), chacune occupée par un **arcane du Tarot de Marseille**, calculé à partir de la **date de naissance** uniquement.

- **Maisons 1 à 13** : occupées par des **Arcanes Majeurs** (numérotés de 1 à 22)
- **Maison 14** : occupée par un **Arcane Mineur** (numéroté de 1 à 56)
- **Maison 8** : seule maison **dynamique** (change chaque année)

---

## 2. Règles de réduction

### Réduction en base 22 (Arcanes Majeurs)

Tous les calculs des maisons 1 à 13 se réduisent en **base 22** :

```
Si résultat > 22 → additionner les chiffres du nombre
Répéter jusqu'à obtenir un nombre entre 1 et 22
Si résultat = 0 → utiliser 22 (Le Mat)
```

**Exemples :**
```
30 → 3 + 0 = 3
25 → 2 + 5 = 7
44 → 4 + 4 = 8
23 → 2 + 3 = 5
22 → 22 (valide, Le Mat)
11 → 11 (valide, La Force — maître nombre conservé)
```

### Réduction en base 9 (Année universelle uniquement)

L'année universelle (utilisée pour M8) se réduit en **base 9** :

```
Additionner tous les chiffres de l'année en cours
Répéter jusqu'à obtenir un nombre entre 1 et 9
EXCEPTION : 11 et 22 ne se réduisent PAS (maîtres nombres)
```

**Exemples :**
```
2024 → 2+0+2+4 = 8
2025 → 2+0+2+5 = 9
2026 → 2+0+2+6 = 10 → 1+0 = 1
2009 → 2+0+0+9 = 11 → reste 11 (maître nombre)
```

### Soustraction avec résultat négatif

```
Si résultat < 0 → ajouter 22
Si résultat = 0 → utiliser 22
```

---

## 3. Données d'entrée

```
Date de naissance : JJ/MM/AAAA
Année en cours : AAAA (pour le calcul de M8)
```

### Extraction des composants

```
JOUR   = JJ (nombre entier, ex: 24)
MOIS   = MM (nombre entier, ex: 6)
ANNEE  = AAAA (nombre entier, ex: 1971)
```

---

## 4. Calcul des 14 Maisons

### Maisons de base (données brutes de la date de naissance)

#### Maison 1 — Personnalité
```
Comment la personne est perçue par les autres.
Ce qu'elle peut apporter à autrui.

M1 = JOUR
Si M1 > 22 → réduire en base 22

Exemples :
  Né le 5  → M1 = 5  (Le Pape)
  Né le 15 → M1 = 15 (Le Diable)
  Né le 24 → M1 = 2+4 = 6 (L'Amoureux)
  Né le 31 → M1 = 3+1 = 4 (L'Empereur)
```

#### Maison 2 — Quête / Idéal
```
Ce que la personne cherche, son idéal, son manque fondamental.

M2 = MOIS

Exemples :
  Né en janvier  → M2 = 1  (Le Bateleur)
  Né en mars     → M2 = 3  (L'Impératrice)
  Né en décembre → M2 = 12 (Le Pendu)
```

> Note : Le mois est toujours entre 1 et 12, donc jamais besoin de réduire.

#### Maison 3 — Pensées, désirs, peurs
```
Le monde intérieur : désirs, peurs, pensées profondes.

M3 = somme des chiffres de l'ANNEE, réduite en base 22

Exemples :
  1966 → 1+9+6+6 = 22 (Le Mat)
  1971 → 1+9+7+1 = 18 (La Lune)
  1987 → 1+9+8+7 = 25 → 2+5 = 7 (Le Chariot)
  2001 → 2+0+0+1 = 3  (L'Impératrice)
```

### Maisons dérivées (premier niveau)

#### Maison 4 — Action / Chemin de vie / Mission
```
La mission de vie, le chemin d'action.

M4 = M1 + M2 + M3, réduit en base 22

Alternative (équivalent) :
M4 = somme de TOUS les chiffres de la date de naissance, réduit en base 22
    = chiffres(JJ) + chiffres(MM) + chiffres(AAAA), réduit en base 22

Exemple (24/06/1971) :
  M1=6, M2=6, M3=18
  M4 = 6 + 6 + 18 = 30 → 3+0 = 3 (L'Impératrice)

  Vérification : 2+4+0+6+1+9+7+1 = 30 → 3+0 = 3 ✓
```

#### Maison 5 — Passage obligé
```
Le passage incontournable, l'épreuve de transformation.

M5 = M1 + M2 + M3 + M4, réduit en base 22

IMPORTANT : Conserver le nombre AVANT réduction pour le calcul de M14.

Vérification (preuve par 9) :
  M5 réduit en base 9 doit être égal au double de M4 réduit en base 9.

Exemple (24/06/1971) :
  M5 = 6 + 6 + 18 + 3 = 33 → 3+3 = 6
  Vérification : M4=3, double=6. M5=6. En base 9 : 6 = 6 ✓

  Nombre avant réduction pour M14 : 33
```

#### Maison 6 — Ressources / Talents
```
Les qualités innées, les talents, ce sur quoi s'appuyer.

M6 = M1 + M2, réduit en base 22

Équivalent : JOUR + MOIS (somme des chiffres), réduit en base 22

Exemple (24/06/1971) :
  M6 = 6 + 6 = 12 (Le Pendu)
```

#### Maison 7 — Défis / Obstacles
```
Ce contre quoi on bute, la leçon à apprendre.

M7 = M3 - M2
Si résultat ≤ 0 → ajouter 22

Certaines sources indiquent : M7 = |M1 - M3| ou M7 = M3 - M2.
Vérifier avec le livre de Colleuil (édition de référence).

Exemple (24/06/1971) — avec M7 = M3 - M2 :
  M7 = 18 - 6 = 12 (Le Pendu)

Exemple alternatif — avec M7 = |M1 - M3| :
  M7 = |6 - 18| = 12, négatif → ajouter 22 si nécessaire
  Ici : 6 - 18 = -12 → -12 + 22 = 10 (La Roue de Fortune)
```

> ⚠️ **Attention** : Il existe des variations selon les sources sur le calcul de M7.
> La formule la plus courante trouvée est M7 = M3 - M2 (si négatif, +22).
> Certaines sources indiquent M7 = M1 - M3 (si négatif, +22).
> **À vérifier impérativement avec le livre de Georges Colleuil** (édition officielle).

### Maison dynamique (change chaque année)

#### Maison 8 — Année en cours / Transformations
```
L'énergie de l'année, ce qui est en mouvement.

Année_Universelle = somme des chiffres de l'année en cours, réduite en BASE 9
  SAUF 11 et 22 qui restent tels quels (maîtres nombres)

M8 = M6 + Année_Universelle, réduit en base 22

Exemple (24/06/1971, année 2026) :
  Année_Universelle = 2+0+2+6 = 10 → 1+0 = 1
  M8 = 12 + 1 = 13 (L'Arcane Sans Nom)

Exemple (même personne, année 2024) :
  Année_Universelle = 2+0+2+4 = 8
  M8 = 12 + 8 = 20 (Le Jugement)
```

### Maisons de profondeur

#### Maison 9 — Soi profond / Terrain d'excellence
```
L'Être intérieur, ce en quoi on peut briller.

M9 = M6 + M7, réduit en base 22

Exemple (24/06/1971, avec M7=12) :
  M9 = 12 + 12 = 24 → 2+4 = 6 (L'Amoureux)
```

#### Maison 10 — Échecs / Ombre / Schémas inconscients
```
Ce qui met en échec, l'ombre, les schémas répétitifs négatifs.

M10 = 22 - M9

Si M10 = 0 → utiliser 22

Exemple :
  M10 = 22 - 6 = 16 (La Maison Dieu)
```

#### Maison 11 — Mémoire / Héritage / Projet parental inconscient
```
Ce qu'on porte sans l'avoir choisi, l'héritage familial.

M11 = M7 + M3 + M10, réduit en base 22

Exemple :
  M11 = 12 + 18 + 16 = 46 → 4+6 = 10 (La Roue de Fortune)
```

#### Maison 12 — Guérison / Idéal / Transmission
```
L'énergie de guérison, ce qu'on laissera derrière soi, les valeurs à transmettre.

M12 = M6 + M2 + M4, réduit en base 22

Exemple :
  M12 = 12 + 6 + 3 = 21 (Le Monde)
```

#### Maison 13 — Paradoxes / Problématique fondamentale ("Cœur du Blason")
```
La tension fondamentale, les contradictions internes.

M13 = M12 + M1 + M5 + M3 + M11 + M4 + M5 + M2 + M9
       (Note : M5 est comptée DEUX FOIS — ce n'est pas une erreur)

Réduit en base 22.

Alternative (même formule, regroupée) :
M13 = (M12 + M1 + M5 + M3 + M11) + (M4 + M5 + M2 + M9)

Exemple :
  M13 = 21 + 6 + 6 + 18 + 10 + 3 + 6 + 6 + 6 = 82 → 8+2 = 10
  (La Roue de Fortune)
```

### Maison 14 — Ressource universelle (Arcane Mineur)

```
La ressource complémentaire, la "roue de secours".

M14 = Nombre obtenu AVANT réduction lors du calcul de M5

Ce nombre est converti en Arcane Mineur via la table suivante :

  Bâtons  :  1 à 14  (Roi=1, Reine=2, Cavalier=3, Valet=4, As=5, 2=6, 3=7... 10=14)
  Coupes  : 15 à 28  (Roi=15, Reine=16, Cavalier=17, Valet=18, As=19... 10=28)
  Épées   : 29 à 42  (Roi=29, Reine=30, Cavalier=31, Valet=32, As=33... 10=42)
  Deniers : 43 à 56  (Roi=43, Reine=44, Cavalier=45, Valet=46, As=47... 10=56)

Si le nombre avant réduction > 56 → réduire (additionner les chiffres)

Exemple :
  M5 avant réduction = 33 → Arcane Mineur n°33 = As d'Épée
```

---

## 5. Signification des 14 Maisons

| Maison | Nom court | Signification |
|--------|-----------|---------------|
| **M1** | Personnalité | Image sociale, ce qu'on montre, ce qu'on apporte |
| **M2** | Quête | Ce qu'on cherche, le manque, l'idéal |
| **M3** | Pensées | Monde intérieur, désirs, peurs, pensées profondes |
| **M4** | Action | Mission de vie, chemin d'action, vocation |
| **M5** | Passage obligé | Épreuve incontournable, transformation nécessaire |
| **M6** | Ressources | Talents innés, qualités, ce sur quoi s'appuyer |
| **M7** | Défis | Obstacles, leçon à apprendre, ce contre quoi on bute |
| **M8** | Année | Énergie de l'année en cours, dynamique temporelle |
| **M9** | Soi profond | Être intérieur, terrain d'excellence |
| **M10** | Échecs | Ombre, schémas inconscients, ce qui met en échec |
| **M11** | Mémoire | Héritage familial, mémoire karmique, projet parental |
| **M12** | Guérison | Énergie de guérison, idéal, valeurs à transmettre |
| **M13** | Paradoxes | Problématique fondamentale, tension, contradictions |
| **M14** | Ressource universelle | Arcane mineur de synthèse, "roue de secours" |

---

## 6. Concepts complémentaires pour l'analyse

### Aspects et configurations

- **Miroirs** : Deux arcanes dont la somme fait 22 (ex: 5+17, 8+14, 10+12)
- **Dialectiques** : Deux mêmes arcanes dans des maisons différentes
- **Trous noirs** : Arcane absent du référentiel (jamais présent dans aucune maison)
- **Brisures** : Configurations spécifiques entre certaines maisons
- **Gammes** : Arcanes partageant la même réduction en base 9 (ex: 3, 12, 21 → tous réduisent à 3)
- **Éclipses** : Quand M8 contient 18 (La Lune) ou 19 (Le Soleil)
- **Minimum Affectif Vital (MAV)** : M7 - M4 (si négatif, +22)

### Le Référentiel de Re-Naissance (annuel)

Variante dynamique calculée pour l'année en cours :
```
M1 et M2 : identiques au Référentiel de Naissance
M3 : chiffres de l'année en cours, réduits en base 22
M4 : M1 + M2 + M3 (année en cours), réduit en base 22
Le reste se recalcule en cascade.
```

---

## 7. Table des 22 Arcanes Majeurs

| N° | Arcane | Mots-clés |
|----|--------|-----------|
| 1 | Le Bateleur | Commencement, potentiel, spontanéité, créativité |
| 2 | La Papesse | Intuition, réceptivité, gestation, mystère |
| 3 | L'Impératrice | Expression, créativité, communication, féminin actif |
| 4 | L'Empereur | Structure, autorité, construction, stabilité |
| 5 | Le Pape | Transmission, sagesse, enseignement, pont entre mondes |
| 6 | L'Amoureux | Choix, amour, responsabilité, union des contraires |
| 7 | Le Chariot | Action, conquête, mouvement, triomphe |
| 8 | La Justice | Équilibre, rigueur, clarté, intégrité |
| 9 | L'Hermite | Intériorité, solitude féconde, sagesse, quête de sens |
| 10 | La Roue de Fortune | Cycles, mouvement, libre-arbitre, tournant de vie |
| 11 | La Force | Énergie maîtrisée, courage, puissance intérieure |
| 12 | Le Pendu | Lâcher-prise, suspension, don de soi, inversion du regard |
| 13 | L'Arcane Sans Nom | Transformation, fin et renouveau, mutation profonde |
| 14 | Tempérance | Harmonie, fluidité, guérison, patience, circulation |
| 15 | Le Diable | Désirs, passions, attachements, forces créatrices brutes |
| 16 | La Maison Dieu | Effondrement, libération, ouverture, choc révélateur |
| 17 | L'Étoile | Inspiration, confiance, don, connexion cosmique |
| 18 | La Lune | Profondeur, inconscient, réceptivité, monde onirique |
| 19 | Le Soleil | Rayonnement, joie, chaleur, fraternité, réussite |
| 20 | Le Jugement | Renaissance, vocation, appel, révélation |
| 21 | Le Monde | Accomplissement, totalité, réalisation, aboutissement |
| 22 | Le Mat | Liberté, saut dans l'inconnu, folie sacrée, élan vital |

---

## 8. Pseudo-code pour l'implémentation

```
FONCTION CalculerReferentiel(jour, mois, annee, anneeEnCours)

    // --- Maisons de base ---
    M1 = ReduceBase22(jour)
    M2 = mois  // toujours entre 1 et 12, pas de réduction
    M3 = ReduceBase22(SommeChiffres(annee))
    
    // --- Maisons dérivées ---
    M4 = ReduceBase22(M1 + M2 + M3)
    
    M5_avant_reduction = M1 + M2 + M3 + M4
    M5 = ReduceBase22(M5_avant_reduction)
    
    M6 = ReduceBase22(M1 + M2)
    M7 = M3 - M2
    SI M7 <= 0 ALORS M7 = M7 + 22
    
    // --- Maison dynamique ---
    anneeUniverselle = ReduceBase9_AvecMaitresNombres(SommeChiffres(anneeEnCours))
    M8 = ReduceBase22(M6 + anneeUniverselle)
    
    // --- Maisons de profondeur ---
    M9  = ReduceBase22(M6 + M7)
    M10 = 22 - M9
    SI M10 == 0 ALORS M10 = 22
    
    M11 = ReduceBase22(M7 + M3 + M10)
    M12 = ReduceBase22(M6 + M2 + M4)
    M13 = ReduceBase22(M12 + M1 + M5 + M3 + M11 + M4 + M5 + M2 + M9)
    
    // --- Maison 14 (Arcane Mineur) ---
    M14 = ConvertirArcaneMineur(M5_avant_reduction)
    
    // --- Vérification (preuve par 9) ---
    ASSERT ReduceBase9(M5) == ReduceBase9(M4 * 2)
    
    RETOURNER { M1..M14 }

FIN FONCTION

FONCTION ReduceBase22(n)
    TANT QUE n > 22
        n = SommeChiffres(n)
    SI n == 0 ALORS n = 22
    RETOURNER n
FIN FONCTION

FONCTION ReduceBase9_AvecMaitresNombres(n)
    SI n == 11 OU n == 22 ALORS RETOURNER n
    TANT QUE n > 9
        n = SommeChiffres(n)
    SI n == 0 ALORS n = 9
    RETOURNER n
FIN FONCTION

FONCTION SommeChiffres(n)
    somme = 0
    POUR CHAQUE chiffre c DANS n
        somme = somme + c
    RETOURNER somme
FIN FONCTION

FONCTION ConvertirArcaneMineur(n)
    TANT QUE n > 56
        n = SommeChiffres(n)
    SI n == 0 ALORS n = 56
    
    SI n >= 1  ET n <= 14 ALORS famille = "Bâtons"
    SI n >= 15 ET n <= 28 ALORS famille = "Coupes"
    SI n >= 29 ET n <= 42 ALORS famille = "Épées"
    SI n >= 43 ET n <= 56 ALORS famille = "Deniers"
    
    rang = ((n - 1) MOD 14) + 1
    
    SI rang == 1  ALORS carte = "Roi"
    SI rang == 2  ALORS carte = "Reine"
    SI rang == 3  ALORS carte = "Cavalier"
    SI rang == 4  ALORS carte = "Valet"
    SI rang == 5  ALORS carte = "As"
    SI rang >= 6  ALORS carte = ToString(rang - 4)  // 2 à 10
    
    RETOURNER { numero=n, famille, carte }
FIN FONCTION
```

---

## 9. Points d'attention pour le développement

### Variantes connues

> ⚠️ **Plusieurs sources présentent des variantes dans les formules.**
> Les formules ci-dessus sont la synthèse la plus cohérente trouvée.
> **Il est fortement recommandé de vérifier avec le livre officiel de Georges Colleuil**
> ("Le Référentiel de Naissance — Langage Symbolique Dynamique du Tarot", 6e édition)
> et de tester avec des cas connus du praticien.

### Cas limites à traiter
- Jours > 22 (24, 25, 26, 27, 28, 29, 30, 31)
- Soustractions donnant exactement 0 → utiliser 22
- Soustractions donnant un résultat négatif → ajouter 22
- Nombre avant réduction de M5 > 56 → réduire pour M14
- Années universelles 11 et 22 → ne pas réduire en base 9

### Tests recommandés
- Implémenter d'abord le calcul, puis vérifier sur 5 à 10 dates connues
- Comparer avec le logiciel officiel de l'école de Colleuil si accessible
- Valider avec le praticien sur ses propres cas cliniques

---

*Ce document sert de référence technique pour l'implémentation du moteur de calcul du Référentiel de Naissance dans le projet HolisticProfile.*
