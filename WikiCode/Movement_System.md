# Système de Mouvement (Movement System)

**Tags:** `Gameplay`, `Mouvement`, `Physique`

## Description
Le `MovementController` est un composant réutilisable conçu pour uniformiser la logique de déplacement physique de toutes les entités du jeu (Joueur et PNJ).
Il encapsule les calculs de vitesse en prenant en compte les statistiques des entités (ex: la statistique de Vitesse via le `StatManager`) et exécute le mouvement en utilisant les capacités natives du moteur Godot (`MoveAndSlide`).

## Fonctionnement
Le système est centré sur le nœud `MovementController` qui doit être ajouté en tant qu'enfant d'un `CharacterBody2D`.

### Propriétés exportées
- **Stats** (`StatManager?`) : Référence optionnelle au gestionnaire de statistiques de l'entité. S'il est fourni, la statistique `BaseSpeedValue` est utilisée comme vitesse par défaut, et la statistique dynamique `Speed` est extraite et appliquée sous forme de pourcentage d'augmentation à cette vitesse de base (ex: 1 point de Speed = +5% de vitesse).
- **DashCooldown** (`float`) : Le délai de recharge du Dash. Par défaut à `3.0`.
- **DashDuration** (`float`) : La durée en secondes du déplacement de Dash. Par défaut à `0.2`.
- **DashSpeedMultiplier** (`float`) : Le multiplicateur de vitesse appliqué lors du Dash. Par défaut à `3.0`.

### Propriétés publiques
- **IsStunned** (`bool`) : Indique si l'entité est actuellement étourdie (ne peut pas bouger ni dasher).
- **IsDashing** (`bool`) : Indique si l'entité est actuellement en plein Dash.

### Méthodes principales
- **Move(Vector2 p_direction, float? p_customBaseSpeed = null)** :
  - Ne s'exécute pas (et applique une vélocité de zéro) si l'entité est étourdie (`IsStunned`).
  - Ne s'exécute pas si l'entité est en plein `Dash`.
  - Calcule la vitesse finale en ajoutant le pourcentage d'augmentation au `p_customBaseSpeed` (ou `Stats.BaseSpeedValue`, avec une valeur par défaut de `300f`).
  - Applique la vélocité calculée (`p_direction * finalSpeed`) au `CharacterBody2D` parent.
  - Appelle `MoveAndSlide()` pour exécuter le déplacement et gérer les collisions avec l'environnement.

- **TryDash(Vector2 p_direction)** :
  - Renvoie `true` et déclenche le mode Dash si le cooldown est écoulé et que l'entité n'est pas déjà en train de dasher ni étourdie (`IsStunned`), sinon renvoie `false`.
  - Le Dash bloque le mouvement standard (`Move`) pendant `DashDuration`, et déplace l'entité rapidement avec `MoveAndSlide` multiplié par `DashSpeedMultiplier`.

- **ApplyStun(float p_duration)** :
  - Applique l'état d'étourdissement (`IsStunned = true`) pendant la durée spécifiée en secondes, bloquant ainsi le déplacement normal ou le dash.

## Intégration
### Configuration d'une nouvelle entité
1. Créez votre scène avec un nœud racine `CharacterBody2D`.
2. Ajoutez une instance du nœud `MovementController.tscn` comme enfant.
3. Si votre entité utilise un `StatManager`, reliez-le à la propriété **Stats** du `MovementController` dans l'éditeur.
4. Dans le script de l'entité, récupérez la référence du contrôleur :
```csharp
private MovementController m_movementController;

public override void _Ready()
{
    m_movementController = GetNodeOrNull<MovementController>("MovementController");
}
```
5. Appelez la méthode `Move()` dans la boucle physique (ex: `_PhysicsProcess`) avec la direction désirée :
```csharp
public override void _PhysicsProcess(double p_delta)
{
    Vector2 direction = GetInputDirection();
    m_movementController?.Move(direction);
}
```

## Décision d'Architecture
Bien que le projet suive une architecture en couches stricte avec la logique métier dans le `Core`, le système de mouvement reste encapsulé dans le projet client (`IslandSurvivor`). Cette décision a été prise pour les raisons suivantes :
- **Performance et API Natives** : Le mouvement dépend fortement du moteur physique de Godot et de la méthode `MoveAndSlide()` qui gère nativement les collisions, les pentes, et la friction de l'environnement. Reproduire cette logique de physique 2D dans le Core serait très coûteux en performance et redondant.
- **Séparation des préoccupations** : Le calcul de l'intention du mouvement (inputs du joueur, IA du PNJ) est séparé de l'exécution physique. Le Core (si nécessaire) peut décider "où" aller, mais le `MovementController` de Godot détermine concrètement "comment" s'y rendre dans le monde physique.
