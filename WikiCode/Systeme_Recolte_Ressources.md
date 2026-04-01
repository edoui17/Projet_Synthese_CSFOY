# Système de Récolte de Ressources (US 4.1)

Ce document explique comment les ressources (Arbres, Roches, Or) sont implémentées du côté de Godot et comment elles interagissent avec le joueur.

## 1. Architecture des Nœuds (Godot)

Chaque ressource dans le jeu hérite d'un `Area2D` et possède un script C# (`Gold.cs`, `Rock.cs`, etc.). L'objectif est de détecter lorsque le joueur tente de briser la ressource pour en extraire des matériaux.

### Les Propriétés Exportées (`[Export]`)
Pour respecter l'architecture N-Tier, le script Godot définit les informations de base de l'objet :
- `EntityId` : L'identifiant unique (ex: `"gold_01"`).
- `MaterialName` : Le nom affiché (ex: `"Or"`).
- `MaterialType` : Le type de matériau (ex: `"Gold"`).
- `IconPath` : Le chemin vers l'icône dans Godot (ex: `"res://Assets/.../Gold Stone 5.png"`). Ce chemin texte sera utilisé par l'interface utilisateur plus tard.

## 2. Interaction Physique (Le groupe "Tool")

Plutôt que d'utiliser un bouton "Interagir" direct, nous avons opté (suite aux retours de l'équipe) pour un système de collision par "Outil" ou "Attaque", similaire aux ennemis.

### La mécanique `AreaEntered`
1. Le nœud de la ressource écoute le signal `AreaEntered`.
2. Lorsqu'une zone (ex: la hache ou l'épée du joueur) entre en collision avec la ressource, le code vérifie si cette zone appartient au groupe `"Tool"`.
3. **Prévention du Multi-Hit (Anti-Spam)** : Godot a un comportement où une seule attaque physique peut déclencher plusieurs collisions dans la même frame. Pour éviter de détruire la ressource 5 fois d'un coup, nous vérifions si le `Timer` de la ressource est arrêté (`Timer.IsStopped()`). Si oui, le Timer démarre, et la ressource est détruite.

```csharp
private void OnAreaEntered(Area2D p_area)
{
    if (p_area.IsInGroup("Tool"))
    {
        if (Timer == null || Timer.IsStopped())
        {
            Timer?.Start();
            DestroyResource();
        }
    }
}
```

## 3. Destruction et Signal Global

Une fois la ressource validée comme étant "détruite" :
1. Une quantité aléatoire est générée.
2. Un objet purement C# `ResourceItem` est instancié avec les données de la ressource (`IconPath`, `Name`, etc.).
3. Un signal global est émis via le **SignalManager** (`SignalManager.Instance.EmitMaterialDestroyed(...)`).
4. La ressource s'efface de la scène avec `QueueFree()`.

Cette approche garantit que la logique de récolte reste très proche du moteur Godot (collisions), mais que l'envoi des données respecte la stricte séparation (le Core).
