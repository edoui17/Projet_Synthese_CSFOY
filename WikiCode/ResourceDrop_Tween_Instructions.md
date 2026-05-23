# Configuration des Tweens pour la récolte de ressources (ResourceDrop)

Dans la scène `ResourceDrop.cs`, nous avons modifié la logique d'animation pour répondre au besoin suivant :
- *Problème :* Les ressources récoltées disparaissaient trop vite et devenaient minuscules trop rapidement lors de leur vol vers le joueur.
- *Solution apportée en C# :*
  - **Durée de voyage (Travel Duration) :** Augmentée de `0.5s` à `0.8s`. La ressource prendra désormais plus de temps pour atteindre le joueur.
  - **Délai de rétrécissement (Scale Delay) :** Le Tween gérant la propriété `scale` (pour faire rétrécir l'objet jusqu'à 0) ne dure plus que la deuxième moitié du voyage (`0.4s`) et possède un délai (`0.4s`).
  - **Courbe d'animation (Ease/Trans) :** Le rétrécissement utilise `Tween.TransitionType.Expo` (Exponentiel) avec `EaseType.In` afin de rester gros plus longtemps et rétrécir brutalement à la fin de son trajet.

### Actions requises dans l'Éditeur Godot

**Aucune action directe n'est requise dans l'éditeur !**
Toute la logique d'animation de la goutte de ressource (Burst et Trajet vers la cible) est entièrement gérée par du code C# dans `Src/IslandSurvivor/Scenes/Ressources/ResourceDrop.cs`. Les Tweens sont créés et gérés dynamiquement.

Si vous souhaitez ajuster ces valeurs à l'avenir (par exemple, pour les rendre plus rapides ou plus lentes), vous pouvez modifier directement ces constantes dans `ResourceDrop.cs` :
```csharp
float travelDuration = 0.8f;
float shrinkDuration = 0.4f;
float shrinkDelay = travelDuration - shrinkDuration;
```
