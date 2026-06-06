# Configuration Audio & Persistance Godot

L'interface de gestion du volume a été connectée au système N-Tier via `IAudioSettingsManager`.

## Modèles et Services
- `AudioSettingsData` : Contient `MasterVolume`, `MusicVolume`, `SfxVolume` avec des valeurs par défaut à `1.0f`.
- `AudioSettingsManager` : Service C# (couche `Core`) qui s'occupe de la sérialisation JSON via `ISaveService` dans `audio_settings.json`.

## Godot Setup
*Aucune manipulation manuelle dans l'éditeur n'est nécessaire.*

Cependant, il est bon de vérifier les points suivants :
1. Dans le script `AudioManager.cs` (`Globals/AudioManager.cs`), au lancement de l'application (`_Ready()`), les valeurs stockées sont automatiquement appliquées aux bus `Master`, `Music`, et `SFX` de `AudioServer`.
2. L'interface `AudioOptions.cs` lit initialement le volume du bus pour définir la valeur des sliders (comme c'était déjà le cas).
3. Les signaux `value_changed` de chaque HSlider (`masterSlider`, `musicSlider`, `sfxSlider`) se déclenchent bien et appellent `_on_master_soudn_h_slider_value_changed` (ou équivalents). La méthode `SaveSettings()` récupère ensuite les nouvelles valeurs de ces sliders et met à jour le fichier `audio_settings.json` instantanément.

## Tests et vérifications (QA)
- Lancer le jeu, modifier le volume de la musique dans le menu des options.
- Vérifier si un fichier `audio_settings.json` est apparu dans le dossier `Save/` (qui se trouve à la racine du `.sln` ou via les réglages OS de Godot).
- Quitter et relancer le jeu, et vérifier que la musique/master/sfx reprennent exactement la valeur enregistrée.
