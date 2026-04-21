# AI Usage Journal - EC

### 2025-05-22 - [User Story: Génération Aléatoire d'Arbres] | **Request** | Implement dynamic tree spawning using Resources.LoadAll equivalent in Godot and a central manager. | **AI Contribution** | Defined IRandomSelector in Core, implemented RandomSelector, updated TreeSpawn with DirAccess scanning, and created TreePopulationManager. | **Decision Reasoning** | Followed N-Tier architecture by separating random logic from Godot-specific scene loading. Used DirAccess for dynamic file discovery to avoid hardcoding.
