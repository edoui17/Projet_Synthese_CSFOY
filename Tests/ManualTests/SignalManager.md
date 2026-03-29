# Tests Manuels - SignalManager (US 5.3)

Afin de valider l'intégrité et le bon fonctionnement du `SignalManager` dans l'environnement de jeu (Godot), les tests manuels suivants doivent être exécutés. Ces tests permettent de confirmer que les scénarios d'acceptation de la *User Story 5.3* sont respectés.

---

## Prérequis de test
* Avoir implémenté temporairement au moins **un (1)** signal concret dans le `Core` (ex: `OnScoreChanged`) et l'avoir délégué dans l'Autoload `SignalManager.cs`.
* Avoir une scène de jeu (Godot) comportant un émetteur de signal (ex: Un bouton "Gagner des points") et un ou plusieurs receveurs (ex: `Label` affichant le score).

---

## Scénario 1 : Mise à jour instantanée des informations

**Objectif :** S'assurer qu'il n'y a aucun délai perceptible entre l'émission du signal et la réaction des systèmes abonnés.

**Procédure :**
1. Lancer le jeu depuis l'éditeur Godot.
2. Déclencher l'événement (ex: Cliquer sur le bouton "Gagner des points").
3. Observer les interfaces réceptrices (ex: Les Labels de score, la barre de santé).

**Résultat attendu :**
* Les interfaces se mettent à jour **immédiatement**.
* Aucun délai, ralentissement, ou décalage (lag) n'est perçu par le joueur.

---

## Scénario 2 : Stabilité lors des transitions (Pattern WeakReference)

**Objectif :** Valider que la suppression d'un objet (Node) de la mémoire ne fait pas planter le système lorsqu'un signal est émis (fuite de mémoire ou référence nulle).

**Procédure :**
1. Lancer le jeu avec une interface abonnée au `SignalManager` (ex: L'ATH du joueur).
2. Forcer la destruction de cet objet (ex: Appeler `QueueFree()` sur le `CanvasLayer` de l'ATH via un bouton "Fermer Menu" ou en changeant de Scène).
3. Une fois l'objet détruit (vérifier l'arbre des nœuds *Remote*), émettre le signal (ex: Cliquer sur "Gagner des points").

**Résultat attendu :**
* Le jeu **ne plante pas**.
* Aucune erreur (`NullReferenceException` ou `ObjectDisposedException`) n'est affichée dans la console de Godot.
* L'objet a bien été nettoyé par le Garbage Collector de .NET.

---

## Scénario 3 : Cohérence des données partagées

**Objectif :** Vérifier que tous les systèmes abonnés au même signal reçoivent la donnée exacte simultanément.

**Procédure :**
1. Instancier **plusieurs** interfaces différentes s'abonnant au même signal (ex: Un ATH en haut de l'écran, un menu de pause, un pop-up de texte flottant).
2. Déclencher un changement de valeur (ex: `SignalManager.Instance.EmitScoreChanged(this, 999);`).
3. Mettre le jeu en pause ou observer attentivement les affichages.

**Résultat attendu :**
* Toutes les interfaces affichent la valeur **exacte** (999) transmise.
* La mise à jour est simultanée sur l'ensemble des écrans.