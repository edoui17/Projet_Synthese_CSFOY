# Futures User Stories - IslandSurvivor

## US 1 : Ajustement du volume (Global et SFX) dans le menu

**Description :**
En tant que joueur, je veux pouvoir ajuster le volume Global et le volume SFX dans les options du menu, afin de personnaliser mon expérience sonore et de ne pas avoir à modifier les paramètres à chaque session.

**Critères d'acceptation :**
- **Scénario 1 : Ajustement des volumes en jeu**
  Étant donné que le joueur est dans le menu principal ou le menu pause
  Quand il modifie les curseurs (sliders) pour le volume Global ou SFX
  Alors le volume en jeu est mis à jour immédiatement en fonction de la nouvelle valeur.

- **Scénario 2 : Sauvegarde des paramètres audio**
  Étant donné que le joueur modifie ses paramètres de son
  Quand il quitte le jeu et le relance lors d'une session ultérieure
  Alors ses paramètres de volume Global et SFX sont sauvegardés et restaurés automatiquement.

**Planning (Fibonacci) :** 3
**Priorité :** Élevée
**Risque :** Faible

**Tâches :**
- [Développement de l'UI Audio], Ajout des curseurs pour Global et SFX dans la scène du menu et liaison avec l'AudioManager, Priorité: 1, Activity Type: Développement, Effort: 2h
- [Sauvegarde locale des paramètres], Création d'un système de sauvegarde/chargement local (ex: via ConfigFile) pour persister les paramètres audio entre les sessions, Priorité: 2, Activity Type: Développement, Effort: 3h
- [Tests UI et Audio], Vérification de l'application correcte des volumes sur les différents bus audio et test de la persistance, Priorité: 1, Activity Type: Testing, Effort: 1h

---

## US 2 : Ajout de décorations générées procéduralement sur la map

**Description :**
En tant que joueur, je veux voir des éléments décoratifs variés sur la carte (buissons, cailloux, ruines), afin de rendre l'environnement plus vivant sans gêner l'apparition des ressources et ennemis.

**Critères d'acceptation :**
- **Scénario 1 : Génération de petites décorations sans collision**
  Étant donné que la carte se génère
  Quand le joueur explore l'île
  Alors il rencontre des petits buissons et cailloux sur lesquels il peut marcher librement sans aucune collision.

- **Scénario 2 : Génération de grosses décorations avec collision**
  Étant donné que la carte se génère
  Quand une ruine ou une grosse structure est placée
  Alors le joueur est bloqué par la structure grâce à une collision bien définie.

- **Scénario 3 : Prévention du chevauchement procédural**
  Étant donné la génération procédurale de l'île
  Quand le système place les décorations, les ressources et les ennemis
  Alors les décorations sont placées à des endroits vides garantissant qu'elles ne chevauchent pas les entités auto-générées.

**Planning (Fibonacci) :** 5
**Priorité :** Moyenne
**Risque :** Moyen (Complexité liée à la génération procédurale et la grille d'occupation)

**Tâches :**
- [Design des Assets], Recherche ou création d'assets visuels pour les buissons, petits cailloux et ruines, Priorité: 2, Activity Type: Design, Effort: 3h
- [Intégration et configuration des scènes de décoration], Création des scènes avec et sans collision (Area/Body) dans Godot, Priorité: 2, Activity Type: Développement, Effort: 2h
- [Logique de placement procédural], Mise à jour de l'algorithme de génération de la carte pour placer les décorations tout en vérifiant l'occupation de l'espace (pas de chevauchement), Priorité: 1, Activity Type: Développement, Effort: 4h
- [Testing du placement], Vérification qu'il n'y a aucun chevauchement et que les collisions fonctionnent correctement sur les grosses structures, Priorité: 2, Activity Type: Testing, Effort: 1h

---

## US 3 : Ajout d'une tour de garde avec archer

**Description :**
En tant que joueur, je veux rencontrer des tours de garde abritant des archers qui me tirent dessus de plus loin, afin d'ajouter un défi stratégique et d'obtenir de grandes récompenses en détruisant ces structures.

**Critères d'acceptation :**
- **Scénario 1 : Détection et attaque de la tour**
  Étant donné que le joueur approche d'une tour de garde
  Quand il entre dans son champ de vision étendu (plus grand que celui d'un archer normal)
  Alors la tour commence à tirer des flèches à la même fréquence qu'un archer standard.

- **Scénario 2 : Destruction de la tour et mort de l'archer**
  Étant donné que le joueur attaque la tour de garde
  Quand les points de vie de la tour atteignent zéro
  Alors la structure de la tour est détruite (avec une collision bloquante persistante ou remplacée par des ruines) et l'archer est vaincu simultanément.

- **Scénario 3 : Loot de récompenses de destruction**
  Étant donné que la tour de garde est détruite
  Quand la structure s'effondre
  Alors elle donne plus d'XP qu'un ennemi standard et libère des ressources (bois en grande quantité, et potentiellement de la pierre).

**Planning (Fibonacci) :** 8
**Priorité :** Élevée
**Risque :** Moyen

**Tâches :**
- [Création du modèle Tour + Archer], Modélisation ou assemblage des scènes pour combiner la structure de la tour et la logique de l'archer, Priorité: 1, Activity Type: Développement, Effort: 3h
- [Logique de ciblage et portée accrue], Ajustement du rayon de détection et implémentation du tir de projectile depuis la tour vers le joueur, Priorité: 1, Activity Type: Développement, Effort: 3h
- [Gestion de la destruction et du loot], Configuration des dégâts reçus par la tour, de la mort synchronisée de l'archer, et de la génération des loots (bois/pierre et XP accrus), Priorité: 1, Activity Type: Développement, Effort: 3h
- [Balancement de la difficulté], Ajustement des points de vie de la tour, de la distance de tir et des taux d'apparition sur l'île, Priorité: 2, Activity Type: Design, Effort: 2h

---

## US 4 : Résidus visuels au sol après récolte ou mort

**Description :**
En tant que joueur, je veux voir des restes visuels au sol (bois, pierre, or, sang, débris) après avoir détruit une ressource ou vaincu une entité, afin de conserver une trace persistante et immersive de mes actions pendant un temps limité.

**Critères d'acceptation :**
- **Scénario 1 : Apparition des résidus au sol**
  Étant donné que le joueur détruit une ressource ou tue un animal passif
  Quand l'entité meurt
  Alors un résidu visuel correspondant (ex: sang de mouton, résidus de bois/pierre) apparaît au sol, sans aucune collision.

- **Scénario 2 : Disparition progressive des résidus**
  Étant donné qu'un résidu visuel est présent sur la carte
  Quand un délai de 30 à 45 secondes s'écoule
  Alors le résidu disparaît progressivement (effet de fade out) pour optimiser les performances.

- **Scénario 3 : Débris permanents pour les structures**
  Étant donné qu'une structure majeure (comme une tour de garde) est détruite
  Quand elle tombe
  Alors ses débris restent de façon permanente sur la carte et conservent une collision bloquante.

**Planning (Fibonacci) :** 5
**Priorité :** Moyenne
**Risque :** Faible

**Tâches :**
- [Création des sprites de résidus], Recherche ou dessin des éléments visuels pour les restes de chaque type de ressource et les taches de sang, Priorité: 3, Activity Type: Design, Effort: 2h
- [Système d'instanciation de résidus], Développement d'un composant/système qui écoute la mort des entités et instancie la scène visuelle appropriée au sol, Priorité: 2, Activity Type: Développement, Effort: 3h
- [Logique de Fade Out (Tween)], Implémentation d'un Tween sur le résidu visuel pour gérer le délai (30-45s) puis la disparition en transparence (`Modulate.A`), Priorité: 2, Activity Type: Développement, Effort: 2h

---

## US 5 : Particules lors de la frappe de ressources

**Description :**
En tant que joueur, je veux voir des particules voler pendant quelques secondes lorsque je frappe une ressource, afin de rendre l'impact physique plus dynamique et satisfaisant.

**Critères d'acceptation :**
- **Scénario 1 : Émission de particules à l'impact**
  Étant donné que le joueur frappe une ressource
  Quand l'arme touche la ressource
  Alors des particules visuelles (ex: éclats) volent autour de l'impact pendant quelques secondes avant de disparaître.

- **Scénario 2 : Couleurs adaptées à la ressource**
  Étant donné que l'effet de particules est généré
  Quand il apparaît
  Alors ses couleurs correspondent au type de la ressource touchée (ex: brun pour le bois, gris pour la roche, jaune pour l'or).

**Planning (Fibonacci) :** 3
**Priorité :** Moyenne
**Risque :** Faible

**Tâches :**
- [Configuration de CPUParticles2D], Création d'une scène de particules réutilisable avec des paramètres physiques (gravité, dispersion, durée de vie), Priorité: 2, Activity Type: Développement, Effort: 2h
- [Adaptation dynamique des couleurs], Ajout d'un script permettant de modifier la couleur (`ColorRamp` ou `Color`) du système de particules en fonction du tag/type de la ressource frappée, Priorité: 2, Activity Type: Développement, Effort: 2h
- [Intégration au système de combat], Déclenchement de l'instanciation des particules dans la méthode de réception de dégâts (`TakeDamage`) des ressources, Priorité: 1, Activity Type: Développement, Effort: 1h

---

## US 6 : Ajout de nouveaux types d'animaux (Passifs et Agressifs)

**Description :**
En tant que joueur, je veux rencontrer une faune plus diversifiée sur l'île (ex: vache, poule, sanglier, ours, loup), afin d'avoir plus de variété dans l'exploration et de faire face à de nouveaux défis naturels.

**Critères d'acceptation :**
- **Scénario 1 : Comportement des animaux agressifs**
  Étant donné que le joueur approche d'un animal agressif (ex: loup, ours, sanglier)
  Quand il entre dans sa zone de détection
  Alors l'animal l'attaque, et à sa mort, il octroie de l'XP, du score, et drop de la viande.

- **Scénario 2 : Comportement des animaux passifs**
  Étant donné que le joueur approche ou attaque un animal passif (ex: vache, poule)
  Quand l'animal est attaqué
  Alors il tente de fuir et, à sa mort, il ne donne ni XP ni score, mais drop de la viande.

- **Scénario 3 : Drop spécifique de viande**
  Étant donné qu'un animal de ce nouveau bestiaire meurt
  Quand ses points de vie atteignent zéro
  Alors la ressource instanciée en guise de loot est systématiquement de la viande.

**Planning (Fibonacci) :** 8
**Priorité :** Moyenne
**Risque :** Moyen (Multiplication des états d'IA et création d'assets visuels/animations)

**Tâches :**
- [Design des Assets Animaux], Recherche ou création des spritesheets pour les différents animaux (vache, poule, ours, sanglier, loup) avec animations d'Idle, Mouvement, et Attaque si applicable, Priorité: 3, Activity Type: Design, Effort: 6h
- [Intégration des animaux passifs], Création des scènes et rattachement au contrôleur d'IA passif existant (`PassiveController`), configuration des loots (viande), Priorité: 2, Activity Type: Développement, Effort: 3h
- [Intégration des animaux agressifs], Création des scènes et rattachement au contrôleur d'IA agressif existant (`AgressorController`), configuration des dégâts, de l'XP, du score et des loots (viande), Priorité: 2, Activity Type: Développement, Effort: 4h
- [Balancement et Spawn], Mise à jour du système de spawn procédural pour inclure ces nouvelles entités selon des probabilités adaptées, Priorité: 2, Activity Type: Développement, Effort: 2h
