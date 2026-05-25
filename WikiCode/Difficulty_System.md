# Système de Gestion de la Difficulté - IslandSurvivor

Le système de difficulté dynamique de *IslandSurvivor* est conçu pour offrir une progression équilibrée mais tendue. La difficulté est gérée par plusieurs composants collaborant entre le Core (Modèles purs) et IslandSurvivor (Logique Godot), respectant notre architecture N-Tier.

## Modèle Mathématique

Le **Global Threat Score** (Score de Menace Global, $S_G$) est le multiplicateur principal du jeu, plafonné de manière stricte à `6.0x` (+500%). Il est calculé comme le produit de trois facteurs :

$$S_G = \text{Clamp}(M_T \times M_L \times M_{Ile}, 1.0, 6.0)$$

### 1. Multiplicateur de Temps ($M_T$)
Augmentation linéaire sur 30 minutes, passant de 1.0 (à 0 min) à 3.0 (à 30 min).
Formule : $1.0 + (\text{TempsEcoulé} / 1800) \times 2.0$

### 2. Multiplicateur de Niveau ($M_L$)
Afin d'éviter une explosion des valeurs mais maintenir la pertinence du levelling, le système utilise une croissance exponentielle (Compound Growth) : +15% par niveau.
Formule : $1.15^{(Niveau - 1)}$

### 3. Multiplicateur d'Île ($M_{Ile}$)
Basé sur l'`Enum` `IslandDifficulty` :
- **Poor (Pauvre) :** 0.8x
- **Normal :** 1.0x
- **Hard (Difficile) :** 1.5x

---

## Simulation du Point de Rupture (30 Minutes)

Voici une simulation de l'évolution du `GlobalThreatScore` ($S_G$) avec différents niveaux de joueur sur 30 minutes :

| Temps (Min) | Niveau Joueur | Île Pauvre (0.8x) | Île Normale (1.0x) | Île Difficile (1.5x) |
|-------------|---------------|-------------------|--------------------|----------------------|
| **0 min**   | Niveau 1      | 0.80x             | 1.00x              | 1.50x                |
| **15 min**  | Niveau 10     | 2.81x             | 3.51x              | 5.27x                |
| **30 min**  | Niveau 20     | *Plancher (4.27x)* | **Plafond (6.00x)**| **Plafond (6.00x)**  |

> **Analyse du Point de Rupture :** À 30 minutes (Temps x3.0), un joueur niveau 20 subira des dégâts massifs. Sur une île Normale ou Difficile, le plafond strict de `6.0x` (Clamp) empêche la valeur de s'emballer vers l'infini. Cela garantit que le jeu pousse agressivement le joueur à extraire et fuir sans causer un crash mathématique de l'économie ou détruire les variables Float.

---

## Architecture des Composants

### 1. Core : `IDifficultyManager` & `IslandDifficulty`
L'`Enum` `IslandDifficulty` définit les types de biomes, tandis que l'interface abstraite garantit l'indépendance de la base mathématique face à l'implémentation moteur.

### 2. IslandSurvivor : `DifficultyManager` (Autoload)
Le `DifficultyManager` calcule $S_G$ à chaque requête et suit le `TimeElapsed`. Il est réinitialisé par le `LevelController` au chargement de chaque nouvelle île.

### 3. IslandSurvivor : `LevelController`
Composant clé pour l'intégrité de la scène ("Single Source of Truth"). Il lit le `Resource` `IslandConfig` (lié au `.tscn` de la map), extrait le biome, et notifie le `DifficultyManager` d'appliquer le multiplicateur tout en réinitialisant le temps.

### 4. IslandSurvivor : `EnemyStatsHandler`
Un **Composant Décorateur** attaché aux monstres (ex. `AggressiveNpcBase.tscn`). Il n'altère pas la définition de base des Npc.
- Il écoute les ordres du `EnemySpawnZone`.
- Il multiplie les points de vie (HP) et les Dégâts (Attack) de base par le $S_G$.
- S'il est désigné "Élite" (probabilité calculée par le Spawner après 15 min), il applique un ultime bonus fixe (+200% HP, +150% Atk) et teinte le modèle en rougeoyant.

### 5. IslandSurvivor : `EnemySpawnZone`
Ce système génère de manière dynamique les paramètres de la "Vague" :
- **Intervalle de Vague** = `BaseInterval / ThreatScore` (Rapide)
- **Nombre d'Ennemis** = `BaseCount * ThreatScore` (Dense)
- Injecte l'indicateur "Élite" et le niveau actuel du joueur dans les entités au moment de leur instanciation.
