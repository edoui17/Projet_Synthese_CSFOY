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
            // Transmet le nœud attaquant pour calculer la statistique "Chance"
            DestroyResource(p_area.GetParent() ?? p_area);
        }
    }
}
```

## 3. Système de Stats Locales, Destruction et Publication

Depuis la mise à jour majeure du système de statistiques, chaque ressource possède son propre `StatManager` (et donc sa propre barre de vie interne isolée).

Une fois la ressource validée comme étant "détruite" (la vie descend à 0) :
1. Le signal local `LocalStatChanged` déclenche `DestroyResource(object p_attacker)`.
2. Le script récupère le `StatManager` de l'`attaquant` (généralement le Joueur) pour extraire sa statistique "Chance" (Luck) et octroyer des matériaux bonus (ex: 5% de bonus par point de stat).
3. Une quantité aléatoire de base est générée puis additionnée au bonus de Chance.
4. Un objet purement C# `ResourceItem` est instancié avec les données de la ressource (`IconPath`, `Name`, etc.).
5. Le Godot `SignalManager` est appelé via `EmitMaterialDestroyed(...)` qui publie à son tour un événement sur l'**EventBus** C# pur, informant les autres systèmes (comme l'inventaire) de la récolte de façon découplée.
6. La ressource s'efface de la scène avec `QueueFree()`.

Cette approche garantit que la logique de récolte reste très proche du moteur Godot (collisions), mais que l'envoi des données respecte la stricte séparation avec le Core via l'EventBus.
