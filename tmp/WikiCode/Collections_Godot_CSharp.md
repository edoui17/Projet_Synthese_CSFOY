# Stratégie des Collections (C# vs Godot)

Afin d'optimiser les performances de notre jeu et de garantir l'étanchéité stricte de notre architecture N-Tiers, une distinction précise doit être faite lors de l'utilisation des collections.

## 1. La Règle d'Or : `System.Collections.Generic` par défaut
Le marshalling (la conversion des données) entre le domaine managé de C# (.NET) et le domaine natif en C++ de Godot est **coûteux en termes de performances**.

Par conséquent, **pour 100% de la logique interne, des calculs du Core et de la gestion d'état**, vous devez utiliser les collections natives C# :
- `List<T>`
- `Dictionary<TKey, TValue>`
- `HashSet<T>`
- `IEnumerable<T>`

**Avantages :**
- Typage fort.
- Accès natif complet et rapide à LINQ (`.Where()`, `.Select()`, etc.).
- Aucune perte de performance due au marshalling.
- Le projet `Core` reste totalement agnostique du moteur de jeu.

## 2. L'Exception Moteur : `Godot.Collections`
Les collections spécifiques à Godot (`Godot.Collections.Array<T>`, `Godot.Collections.Dictionary`) ne doivent être utilisées **que dans la couche d'orchestration (`IslandSurvivor`)** et strictement dans les deux cas suivants :

### Cas A : Exposition dans l'Inspecteur
L'éditeur Godot ne sait sérialiser que ses propres collections. Si vous devez exposer un tableau dans l'Inspecteur via l'attribut `[Export]`, il doit s'agir d'une collection Godot.

```csharp
// CORRECT : Exposé dans l'inspecteur
[Export] public Godot.Collections.Array<PackedScene> EnemyScenes { get; set; } = new();

// INCORRECT : Ne sera pas visible dans l'inspecteur
[Export] public System.Collections.Generic.List<PackedScene> EnemyScenes { get; set; } = new();
```

### Cas B : Interopérabilité avec les APIs natives
Si une méthode native du moteur retourne ou requiert spécifiquement une collection Godot (ex: `GetChildren()`, requêtes physiques), vous devez utiliser ou traiter ces collections, mais n'hésitez pas à convertir les données vers des listes C# si un traitement intensif ou mathématique doit s'ensuivre dans la logique métier.

## Résumé
- **Core / Logique métier / Back-end** = `System.Collections.Generic.List<T>`
- **IslandSurvivor / UI / Export Inspecteur** = `Godot.Collections.Array<T>`
