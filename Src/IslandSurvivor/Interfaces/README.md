# Classes (Scripts)

Ce dossier contient la logique pure du jeu. Contrairement aux scripts attachés aux nœuds, les fichiers ici sont souvent des classes C# standards.

### Contenu typique :
* **Logic/** : Algorithmes de génération procédurale, calculs de trajectoires, etc.
* **Data/** : Structures de données locales au client de jeu.
* **Interfaces/** : Définition des contrats pour le code (ex: `IDamageable`).

**Lien avec le Core :** Si une classe ici est amenée à être utilisée aussi par l'API ou le site Web, elle devrait être déplacée dans le projet `ProjetJeu.Core`.