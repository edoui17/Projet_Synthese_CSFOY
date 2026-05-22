# Architecture du Système de Combat

## Vue d'Ensemble
Le système de combat de IslandSurvivor utilise une mécanique d'attaque à zone d'effet (AoE) unifiée. Une seule action (attaquer via un clic gauche de la souris) permet au joueur d'infliger des dégâts aux ennemis et de récolter des ressources simultanément, à condition qu'ils se trouvent dans la hitbox de l'arme du joueur. Le bouton d'attaque peut être maintenu enfoncé pour déclencher des attaques consécutives dès l'expiration de leur temps de recharge (cooldown). La durée du temps de recharge diminue dynamiquement en fonction de la statistique de `Vitesse` (`Speed`) du joueur, selon la formule : `Cooldown = BaseCooldown / (1 + Speed * 0.05)`.

## Interfaces Clés
*   **`IDamageable`** (`Src/Core/Interfaces/Stats/IDamageable.cs`) : Définit le contrat pour toute entité pouvant subir des dégâts.
    *   `void TakeDamage(int p_amount, Core.Domain.DamageContext p_context);`
    *   Cette interface est implémentée par les PNJ agressifs (ex: `Soldier.cs`), les PNJ passifs (ex: `Sheep.cs`), et les ressources (ex: `Rock.cs`, `Gold.cs`, `ConiferTree.cs`, `AutomnTree.cs`). Le `DamageContext` permet au système de transmettre la référence de l'attaquant en plus des dégâts calculés et d'instantanés de statistiques spécifiques (comme la Chance), sans forcer la victime à lire le `StatManager` d'une autre entité.

## Flux de Résolution des Dégâts (Joueur attaquant un Ennemi/Ressource)
1.  **Déclenchement de l'Attaque** : Le joueur appuie ou maintient le bouton gauche de la souris. Si le drapeau `m_canAttack` est vrai (temps de recharge terminé), `Player.cs` entre dans l'état `Attacking` et verrouille le mouvement.
2.  **Acquisition de la Cible** : Le script s'appuie sur des signaux basés sur des événements (`AreaEntered`, `BodyEntered`) provenant de `m_weaponAreaRight` / `m_weaponAreaLeft` (`Area2D`) du joueur.
3.  **Filtrage et Déduplication** : Le script vérifie si chaque objet en chevauchement (ou son nœud parent) implémente `IDamageable`. Il utilise un `HashSet<IDamageable>` pour garantir qu'une entité avec plusieurs collisionneurs en chevauchement ne reçoive des dégâts qu'une seule fois par exécution d'attaque.
4.  **Calcul des Dégâts** : Le montant des dégâts est calculé en se basant sur le `BaseAttackValue` et récupéré depuis le `StatManager` du joueur (`StatType.Attack`).
5.  **Application des Dégâts** : `TakeDamage(amount, new DamageContext(...))` est invoqué sur chaque cible valide dans le `HashSet`.

## Flux de Résolution des Dégâts (Ennemi attaquant le Joueur)
1.  **Déclencheur de Hitbox** : L'ennemi agressif (ex: `Soldier` ou `Archer`) possède une `Area2D` nommée `HitboxArea` utilisée pour détecter le joueur.
2.  **Acquisition de la Cible** : Lorsqu'un corps (Body) entre dans la `HitboxArea`, le signal `BodyEntered` se déclenche.
3.  **Vérification & Application des Dégâts** : Le script vérifie si le corps en collision fait partie du groupe "Player" et implémente `IDamageable`. Si c'est vrai, l'ennemi appelle immédiatement `TakeDamage(amount, new DamageContext(...))` sur le joueur. Le `BaseAttackValue` est configuré pour s'adapter correctement à partir du Niveau 1 pour ces entités. Le `StatManager` du joueur déduit ensuite la santé correspondante.

## Intégrations des Sous-systèmes
*   **StatManager** : Depuis la refonte majeure des statistiques, chaque entité possède un `StatManager` isolé utilisant un `EventBus` local. L'attaquant détermine ses dégâts sortants (BaseAttackValue + Multiplicateur d'Attaque). Le défenseur déduit la santé et utilise le signal `LocalStatChanged` pour déclencher sa séquence de mort si la santé atteint 0.
*   **InventorySystem** : Lorsqu'une ressource (ou un ennemi avec du butin) atteint 0 de santé, elle reçoit le `DamageContext` contenant le bonus de Chance (`Luck`) de l'attaquant, instancie un `ResourceItem`, et diffuse sa destruction via `SignalManager.Instance.EmitMaterialDestroyed()`. L'`InventoryNode` global écoute ce signal et incrémente l'inventaire du joueur.
*   **ScoreManager** : Lorsqu'un ennemi agressif (comme `Soldier`) meurt, il notifie le `ScoreTracker` du Core via `ServiceRegistry.Instance.ScoreTracker.AddScore(int)` pour incrémenter le score du joueur.

## Système de Combat des Ennemis (Mêlée)

Le système de combat des ennemis dans IslandSurvivor suit l'architecture N-Tier, séparant la logique métier de la représentation client Godot.

### Logique Core (Src/Core)
- **Interfaces :** `IAttackable`, `IDamageable` dictent l'interaction fondamentale pour infliger et recevoir des dégâts.
- **Contrôleurs :** `IAgressorController` gère la machine à états pour les entités agressives. Il suit les états (`IDLE`, `CHASE`, `ATTACK`, `DEAD`) et gère les temps de recharge d'attaque et les durées purement en logique C#, sans avoir conscience des frames (images) Godot.

### Client Godot (Src/IslandSurvivor)
- **Détection des Coups (Hit Detection) :** Les attaques de mêlée utilisent des nœuds `Area2D` dédiés (`HitboxArea` sur les ennemis, `WeaponAttack` sur les joueurs).
- **Suivi des Cibles (Target Tracking) :** Les entités utilisent les signaux `BodyEntered` et `BodyExited` pour maintenir un `HashSet<IDamageable>` des cibles actuellement en chevauchement. Cette approche est plus fiable que d'interroger `GetOverlappingBodies()` en plein milieu d'une animation.
- **Visuels :** Le `_PhysicsProcess` interroge l'état actuel du contrôleur. Si l'état est `ATTACK`, le mouvement est arrêté, et l'`AnimatedSprite2D` passe à l'animation "Attack".
- **Intégration des Statistiques :** Les calculs de dégâts et les modifications de santé sont traités via le `StatManager` attaché, garantissant que toutes les statistiques de l'entité sont centralisées et pilotées par les ressources `EntityStats`.