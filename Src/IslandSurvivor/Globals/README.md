# Globals

Ce dossier contient les scripts utilisés comme **Autoloads** (Singletons) dans Godot. 

### Contenu :
* **GameManager.cs** : État général de la partie (en pause, score actuel, etc.).
* **Settings.cs** : Préférences de l'utilisateur (volume, résolution).
* **Events.cs** : Bus d'événements global pour la communication entre scènes éloignées.

> **Qualité :** Utilisez les Singletons avec parcimonie pour éviter de créer un code trop couplé. Si une donnée peut être passée de parent à enfant, privilégiez cette méthode.