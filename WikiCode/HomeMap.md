## La HomeMap

La HomeMap est composer de plusieur couche. Chaque couche est une certaine partie du terrain.
La couche qui est un peut differente c'est celle du batiment qui a un node2D qui est le batiment car elle est composer d'une hit box
et aussi d'une zone d'interaction.

Voici les differentes couche dans le node2D BaseMapIsland:

1.WaterTileMap: c'est le fond bleu a l'arriere de tout
2.FoamWater: se sont les animations d'eau autour de la map
3.GroundTileMap: est la couche de gazon sur l'eau
4.ShadowTileMap: c'est la couche d'ombrage sous les elevations
5.ElevationTileMap: est la couche avec les falaises et les plateaux
6.ElementsTileMap: cette couche est celle des arbres,buisson et autre decoration sur le GroundTileMap
7.ElementsForElevationTileMap: cette couche est celle des arbres,buisson et autre decoration sur le ElevationTileMap
8.cloudTileMap: cette cocuche est les nuages

Dans le BuildingNode qui est le batiment:

1.Building: est l'image
2.Collision: est la hit box du batiment
3.InteractionArea: est la zone qui va permettere aux joueur d'interagire pour le personnage
