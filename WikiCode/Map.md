##Documentation Technique — BaseMap & TileMaps

Objectif général

Décrire la structure, le rôle et les contraintes de chaque TileMap composant une map jouable. Cette documentation sert de référence pour la création manuelle, la génération procédurale et la validation automatique.

## Structure hiérarchique

BaseMap
 ├── WaterTileMap
 ├── FoamWaterTileMap
 ├── GroundTileMap
 ├── BridgeTileMap
 ├── ShadowTileMap
 ├── ElevationTileMap
 └── CloudTileMap

Chaque couche possède un rôle visuel et logique distinct.

## WaterTileMap

Rôle

Fond aquatique couvrant toute la zone non-terrestre.

Base visuelle de la map.

Contraintes

Toujours sous toutes les autres couches.

Couvre 100% des zones non-solides.

Jamais au-dessus du sol.

Génération

Première couche générée.

Sert de masque négatif pour le sol.

## FoamWaterTileMap

Rôle

Bordure de mousse autour du sol pour améliorer la lisibilité.

Contraintes

Uniquement sous les tuiles de sol.

Jamais au milieu de l’eau.

Suit la forme exacte du GroundTileMap.

Génération

Calculée après le sol.

Détection de bord (4 directions + diagonales).

## GroundTileMap

Rôle

Surface jouable principale.

Support des collisions et interactions.

Contraintes

Couvre ≥ 65% de la surface totale.

Aucune île isolée (connectivité obligatoire).

Plateformes cohérentes et navigables.

Génération

Basée sur bruit + règles de connectivité.

Sert de base pour Foam, Shadow et Elevation.

## BridgeTileMap

Rôle

Ponts reliant des zones séparées.

Évite les ruptures de navigation.

Contraintes

Relie deux zones de GroundTileMap.

Jamais flottant dans le vide.

Peut être au-dessus de l’eau.

Génération

Optionnel, déclenché si deux zones sont trop éloignées.

##ShadowTileMap

Rôle

Ombres sous les plateformes et élévations.

Améliore la perception de profondeur.

Contraintes

Toujours sous GroundTileMap et ElevationTileMap.

Généré automatiquement selon les tuiles de sol.

Génération

Offset vertical (ex. +1 tuile).

Peut être adouci selon le style.

## ElevationTileMap

Rôle

Zones en hauteur (falaises, plateformes surélevées).

Utilise des stone sides pour les bords verticaux.

Contraintes

Connectée au sol ou plateforme logique.

Stone sides sur tous les bords exposés.

Ombres obligatoires sous les élévations.

Génération

Après le sol.

Peut utiliser un second bruit ou une règle de zones.

## CloudTileMap

Rôle

Éléments décoratifs flottants.

Aucun impact gameplay.

Contraintes

Jamais en collision avec le joueur.

Peut être animé ou parallaxé.

Génération

Placement pseudo-aléatoire avec densité configurable.

## Validation automatique

La map doit respecter les règles suivantes :

Ground coverage ≥ 65%

Aucune île isolée

Foam sous le sol uniquement

Stone sides sur toutes les élévations

Ombres sous les élévations

Bridges connectent deux zones valides