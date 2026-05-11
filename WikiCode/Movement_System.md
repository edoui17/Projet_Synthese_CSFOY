# Système de Mouvement (Movement System)

**Tags:** `Gameplay`, `Mouvement`, `Physique`

## Description
Le `MovementController` est un composant réutilisable conçu pour uniformiser la logique de déplacement physique de toutes les entités du jeu (Joueur et PNJ).
Il encapsule les calculs de vitesse en prenant en compte les statistiques des entités (ex: la statistique de Vitesse via le `StatManager`) et exécute le mouvement en utilisant les capacités natives du moteur Godot (`MoveAndSlide`).

## Fonctionnement
Le système est centré sur le nœud `MovementController` qui doit être ajouté en tant qu'enfant d'un `CharacterBody2D`.

### Propriétés exportées
- **BaseSpeed** (`float`) : La vitesse de base du mouvement si aucune n'est spécifiée, et sur laquelle les bonus de statistiques seront appliqués. Par défaut à `300.0`.
- **Stats** (`StatManager?`) : Référence optionnelle au gestionnaire de statistiques de l'entité. S'il est fourni, la statistique `Speed` est extraite et appliquée sous forme de pourcentage d'augmentation à la vitesse de base (ex: 10 de speed = +10% de vitesse).

### Méthode principale
- **Move(Vector2 p_direction, float? p_customBaseSpeed = null)** :
  - Calcule la vitesse finale en ajoutant le pourcentage d'augmentation au `p_customBaseSpeed` (ou `BaseSpeed` s'il est null).
  - Applique la vélocité calculée (`p_direction * finalSpeed`) au `CharacterBody2D` parent.
  - Appelle `MoveAndSlide()` pour exécuter le déplacement et gérer les collisions avec l'environnement.

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
