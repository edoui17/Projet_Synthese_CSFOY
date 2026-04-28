# Système de Statistiques (StatManager)

## Introduction
Le système de statistiques d'IslandSurvivor suit rigoureusement l'architecture N-Tier. Il est divisé en deux parties :
1. **La Logique (Core) :** Gère les mathématiques (limites, bonus, calculs) de manière agnostique vis-à-vis du moteur.
2. **L'Intégration (Godot Client) :** Gère la configuration initiale via des `Ressources` et expose les signaux visuels via le Node `StatManager`.

## Fonctionnement Global (Bridge Pattern)

L'idée centrale est de ne jamais modifier les valeurs de base directement dans le moteur Godot. Le Node `StatManager` sert de "pont" (Bridge) entre Godot et la logique pure du C#.

1. **La Ressource (`EntityStats`) :** Contient les valeurs de *départ* de l'entité (Vie Max, Attaque, Chance, Vitesse). Elle est strictement en **Lecture Seule** pendant l'exécution du jeu.
2. **Le Node (`StatManager`) :** Attaché à l'entité, il lit la Ressource au `_Ready()` et l'injecte dans le Core.
3. **Le Core (`StatTracker`) :** Maintient un dictionnaire (`Dictionary<StatType, Stat>`). Il calcule les valeurs effectives et s'assure qu'elles ne descendent pas sous `0` ou ne dépassent pas la valeur maximale.
4. **La Communication :** Quand une statistique change dans le Core, un `StatChangedEvent` est publié via l'**EventBus**. Le `StatManager` de Godot, via injection de l'EventBus, s'abonne à cet événement et ré-émet un `[Signal]` natif pour que l'éditeur visuel de Godot puisse y connecter des barres de vie ou des effets sonores sans couplage direct avec le C#.

## Comment l'utiliser dans Godot ?

### 1. Créer une nouvelle configuration de statistiques
- Allez dans l'éditeur Godot.
- Faites `Clic Droit > Créer une nouvelle Ressource`.
- Cherchez `EntityStats`.
- Remplissez les valeurs de base (ex: MaxHealth = 150, Attack = 25).
- Sauvegardez le fichier `.tres`.

### 2. Ajouter les statistiques à une Entité
- Ouvrez la scène de votre entité (ex: `Player.tscn` ou `Goblin.tscn`).
- Ajoutez un Node enfant de type `StatManager`.
- Dans l'inspecteur, glissez-déposez la ressource `EntityStats` (créée à l'étape 1) dans le champ `Base Stats Resource`.

### 3. Modifier la santé (Prendre des dégâts ou se soigner)
Depuis un script (ex: `Player.cs` ou `Enemy.cs`) :
```csharp
// Pour infliger 20 points de dégâts
GetNode<StatManager>("StatManager").ModifyCurrentValue(StatType.Health, -20f);

// Pour soigner de 10 points
GetNode<StatManager>("StatManager").ModifyCurrentValue(StatType.Health, 10f);
```

### 4. Ajouter un bonus permanent
Lorsqu'un niveau est gagné ou qu'un objet permanent est équipé :
```csharp
// Ajoute +5 d'attaque de façon permanente.
// Le système va augmenter l'EffectiveMaxValue et ajuster la valeur actuelle en conséquence.
GetNode<StatManager>("StatManager").AddPermanentBonus(StatType.Attack, 5f);
```

### 5. Connecter l'Interface Utilisateur (Barre de Vie)
Vous pouvez utiliser l'onglet "Node" de Godot pour connecter le signal `StatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)`.

Depuis le code (fortement recommandé) :
```csharp
public override void _Ready()
{
    StatManager statManager = GetNode<StatManager>("StatManager");
    statManager.StatChanged += OnStatChanged;
}

private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
{
    if ((StatType)p_statType == StatType.Health)
    {
        // Mettre à jour la barre de vie
        m_healthBar.MaxValue = p_effectiveMaxValue;
        m_healthBar.Value = p_currentValue;
    }
}
```

## Ajout de nouvelles statistiques (Pour les développeurs)
Si vous devez ajouter une nouvelle statistique au jeu (ex: `Defense` ou `Mana`) :

1. Ajoutez l'entrée dans l'enum `/Src/Core/Managers/Stats/StatType.cs`.
2. Ajoutez la propriété `[Export]` correspondante dans le fichier ressource `/Src/IslandSurvivor/Resources/EntityStats.cs`.
3. Ajoutez l'entrée dans l'initialisation du dictionnaire à l'intérieur de `_Ready()` dans `/Src/IslandSurvivor/Nodes/StatManager.cs`.

## Persistance (Méta-Progression)
Les statistiques de méta-progression (améliorations permanentes) sont gérées séparément via le [Persistence System](./Persistence_System.md). Elles sont stockées en base de données et chargées au démarrage du jeu pour être appliquées comme bonus permanents via `AddPermanentBonus`.

**Synchronisation (US 8.1) :**
- **Au lancement** : Appel à `/api/player/profile` pour récupérer les stats persistantes.
- **En jeu** : Sauvegarde via `/api/stats/upsert` ou lors d'une synchronisation globale (`/api/player/sync`).