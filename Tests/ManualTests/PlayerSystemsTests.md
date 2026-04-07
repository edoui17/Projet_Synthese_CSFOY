# Devis de Tests Manuels : Mouvement et Interaction

Ce document détaille les scénarios de tests manuels pour valider le système de mouvement et d'interaction du joueur.

## 1. Mouvement du Personnage

| ID | Titre | Description | Résultat Attendu |
| :--- | :--- | :--- | :--- |
| M-01 | WASD / Flèches | Appuyer sur W, A, S, D ou les flèches. | Le personnage se déplace dans la direction correspondante. |
| M-02 | Accélération | Maintenir une touche directionnelle. | Le personnage atteint progressivement sa vitesse maximale. |
| M-03 | Friction | Relâcher la touche de mouvement. | Le personnage s'arrête de manière fluide (ne s'arrête pas instantanément). |
| M-04 | Animation Run | Se déplacer. | L'animation "RUN" se déclenche. |
| M-05 | Animation Idle | S'arrêter. | L'animation "IDLE" se déclenche. |
| M-06 | Stat Speed | Modifier la vitesse dans `PlayerStats.tres`. | La vitesse maximale du joueur change en conséquence. |

## 2. Système d'Interaction

| ID | Titre | Description | Résultat Attendu |
| :--- | :--- | :--- | :--- |
| I-01 | Détection Proximité | S'approcher d'un bâtiment interactif. | L'indicateur "Press E to Interact" apparaît au-dessus du joueur. |
| I-02 | Perte de Focus | S'éloigner d'un objet après détection. | L'indicateur visuel disparaît. |
| I-03 | Déclenchement | Appuyer sur 'E' près d'un objet. | L'action se déclenche (voir console) et le mouvement est bloqué durant l'action. |
| I-04 | Priorité (Distance) | Se placer entre deux objets interactifs. | L'indicateur affiche le prompt de l'objet le plus proche. |
| I-05 | Animation Interaction | Déclencher une interaction. | L'animation "INTERACT" se joue sur le personnage. |
| I-06 | Mono-interaction | Appuyer sur 'E' rapidement plusieurs fois. | Une seule interaction se déclenche à la fois ; le système attend la fin de l'action. |

## 3. Physique et Collisions

| ID | Titre | Description | Résultat Attendu |
| :--- | :--- | :--- | :--- |
| P-01 | Mur / Obstacle | Essayer de traverser un arbre ou un bâtiment. | Le personnage est bloqué physiquement par les colliders. |
| P-02 | Interaction Layer | Vérifier si l'interaction fonctionne à travers un mur. | L'interaction doit être possible tant que le joueur est dans l'Area2D, sauf si spécifié autrement par le level design. |
