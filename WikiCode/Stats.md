# Système de Statistiques (StatManager)

## Introduction
Le système de statistiques d'IslandSurvivor suit rigoureusement l'architecture N-Tier. Il est divisé en deux parties :
1. **La Logique (Core) :** Gère les mathématiques (limites, bonus, calculs) de manière agnostique vis-à-vis du moteur.
2. **L'Intégration (Godot Client) :** Gère la configuration initiale via des `Ressources` et expose les signaux visuels via le Node `StatManager`.

## Fonctionnement Global (Bridge Pattern)

L'idée centrale est de ne jamais modifier les valeurs de base directement dans le moteur Godot. Le Node `StatManager` sert de "pont" (Bridge) entre Godot et la logique pure du C#. Depuis la refactorisation majeure, **chaque entité possède sa propre instance locale isolée** de `StatTracker` et d'`EventBus`, éliminant le couplage lié à un Singleton global.

1. **Les Ressources Modulaires (`EntityStats`, `CombatEntityStats`, `PlayerStats`) :** Contiennent les valeurs de *départ* de l'entité via un système d'héritage. Par exemple, un arbre utilise `EntityStats` (uniquement la Santé), tandis que le joueur utilise `PlayerStats` (Santé, Attaque, Vitesse, Chance). Ces ressources sont strictement en **Lecture Seule** pendant l'exécution du jeu.
2. **Le Node (`StatManager`) :** Attaché à l'entité, il lit la Ressource au `_Ready()`, instancie un `EventBus` local et un `StatTracker` local, puis y injecte les valeurs.
3. **Le Core (`StatTracker`) :** Maintient un dictionnaire (`Dictionary<StatType, IStat>`) en utilisant `PoolStat` pour la santé et `AttributeStat` pour le reste. Il calcule les valeurs effectives et s'assure qu'elles ne descendent pas sous `0` ou ne dépassent pas la valeur maximale pour les jauges.
4. **La Communication Locale :** Quand une statistique change dans le Core, un `StatChangedEvent` est publié via l'**EventBus Local** de l'entité. Le `StatManager` de Godot écoute cet événement local et ré-émet un `[Signal] LocalStatChanged` natif. Les interfaces utilisateur s'abonnent à ce signal local, garantissant qu'une UI de monstre ne réagit pas aux dégâts pris par le joueur.

## Comment l'utiliser dans Godot ?

### 1. Créer une nouvelle configuration de statistiques
- Allez dans l'éditeur Godot.
- Faites `Clic Droit > Créer une nouvelle Ressource`.
- Cherchez le type approprié : `EntityStats` (Ressources basiques), `CombatEntityStats` (Monstres/PNJ), ou `PlayerStats` (Joueur).
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
Vous pouvez utiliser l'onglet "Node" de Godot pour connecter le signal `LocalStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)`.

Depuis le code (fortement recommandé) :
```csharp
public override void _Ready()
{
    StatManager statManager = GetNode<StatManager>("StatManager");
    statManager.LocalStatChanged += OnStatChanged;
}

private void OnStatChanged(int p_statType, float p_currentValue, float p_effectiveMaxValue)
{
    if ((StatType)p_statType == StatType.Health)
    {
        // Mettre à jour la barre de vie locale
        m_healthBar.MaxValue = p_effectiveMaxValue;
        m_healthBar.Value = p_currentValue;
    }
}
```

### 6. Gérer les destructions et les butins
La méthode `TakeDamage` requiert l'instance de l'attaquant (`object p_attacker`). Ceci est indispensable pour que l'entité détruite (ex: un rocher) puisse extraire le `StatManager` de l'attaquant et calculer les probabilités de butin basé sur sa statistique `Chance` (`Luck`). L'entité détruite écoute sa propre mort via son signal `LocalStatChanged` pour éviter d'insérer de la logique de destruction directement dans la prise de dégâts.

## Ajout de nouvelles statistiques (Pour les développeurs)
Si vous devez ajouter une nouvelle statistique au jeu (ex: `Defense` ou `Mana`) :

1. Ajoutez l'entrée dans l'enum `/Src/Core/Managers/Stats/StatType.cs`.
2. Ajoutez la propriété `[Export]` correspondante dans le bon niveau de ressource (ex: `CombatEntityStats.cs`).
3. Ajoutez l'entrée dans la logique d'initialisation du dictionnaire (et les vérifications de type `is CombatEntityStats`) à l'intérieur de `_Ready()` dans `/Src/IslandSurvivor/Nodes/StatsManager/StatManager.cs`.

## Persistance (Méta-Progression)
Les statistiques de méta-progression (améliorations permanentes) sont gérées séparément via le [Persistence System](./Persistence_System.md). Elles sont stockées en base de données et chargées au démarrage du jeu pour être appliquées comme bonus permanents via `AddPermanentBonus`.

**Synchronisation (US 8.1) :**
- **Au lancement** : Appel à `/api/player/profile` pour récupérer les stats persistantes.
- **En jeu** : Sauvegarde via `/api/stats/upsert` ou lors d'une synchronisation globale (`/api/player/sync`).