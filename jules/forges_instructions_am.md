# Setup de la scène Player - IslandSurvivor

Suite aux modifications dans `Player.cs` pour utiliser une logique d'attaque basée sur des signaux (Option B), voici les instructions pour vérifier ou configurer manuellement les nœuds dans l'éditeur Godot.

## Nœud : WeaponInteractionArea (`Area2D`)

1. Ouvrez `Player.tscn` dans Godot.
2. Localisez le nœud enfant **WeaponInteractionArea** (sous `Player`).
3. Dans l'inspecteur, vérifiez les **Collision** :
    - **Layer** : Activez la couche `4` (Combat). Godot affichera que sa valeur (mask) est `8`.
    - **Mask** : Activez la couche `5` (Ressource / Ennemi). Godot affichera que sa valeur (mask) est `16`.
4. Sélectionnez le nœud **WeaponInteractionArea** et allez dans l'onglet **Node** (à côté d'Inspector) > **Signals**.
5. Si ce n'est pas déjà fait par le code `_Ready()`, vous pouvez lier les signaux manuellement pour vous assurer de la redondance :
    - Double-cliquez sur le signal **`area_entered`**.
    - Connectez-le au script du nœud `Player`, en choisissant la méthode `OnWeaponAreaEntered`.
    - Double-cliquez sur le signal **`body_entered`**.
    - Connectez-le au script du nœud `Player`, en choisissant la méthode `OnWeaponBodyEntered`.

## Nœud : WeaponHitbox (`CollisionShape2D`)

1. Ce nœud est un enfant de `WeaponInteractionArea`.
2. Par défaut, sa propriété **Disabled** doit être **cochée** (Activé / Vrai). Le collider ne doit pas exister en temps normal.

## Nœud : AnimationPlayer

1. Sélectionnez le nœud **AnimationPlayer**.
2. Ouvrez l'animation `ATTACK` dans le panneau d'animation en bas.
3. Vérifiez la piste (track) ciblant la propriété `disabled` de `WeaponHitbox` :
    - Au temps **0.0s** ou de base, elle doit être `true` (désactivée).
    - Vers **0.2s** (lorsque le coup part), il doit y avoir une clé mettant la propriété `disabled` à `false` (activée).
    - Vers **0.4s** (à la fin de la frappe), il doit y avoir une autre clé la remettant à `true` (désactivée).

## Test du fonctionnement

Lorsque l'animation se lance (appui sur espace), la Hitbox va s'activer à `0.2s`. À cet instant précis, le moteur physique de Godot va détecter ce qui chevauche la zone et déclencher les événements `area_entered` ou `body_entered` pour toute entité sur le Masque 5, provoquant ainsi l'application des dégâts.