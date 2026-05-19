# Main Menu — Résumé technique

## Objectif

Le Main Menu constitue le point d’entrée principal de l’application. Il permet à l’utilisateur d’accéder aux différentes fonctionnalités du jeu, notamment le démarrage d’une partie, la consultation des scores, la configuration des options et la fermeture de l’application.

## Organisation du projet

Un dossier dédié nommé `MainMenu` a été créé afin de regrouper tous les éléments liés au menu. Cette séparation permet de maintenir une architecture modulaire et indépendante des autres scènes du jeu.

Le dossier contient trois sous-dossiers principaux :

- `MainMenu` : scène principale du menu  
- `OptionsMenu` : scène de configuration audio  
- `ScoreBoardMenu` : scène d’affichage des scores  

Chaque scène possède ses scripts associés lorsque nécessaire.

## Structure et navigation

Le menu principal repose sur une scène contenant :

- Un fond visuel  
- Un logo  
- Quatre boutons principaux :  
  - Start  
  - Scoreboard  
  - Options  
  - Quit  

Chaque bouton est associé à une détection d’événement (clic), déclenchant une action définie dans le script de la scène.

## Fonctionnalités

### Start
- Action : change la scène active  
- Résultat : redirige vers la map principale du jeu via le `SceneLoadingManager`. Le système affiche d'abord un écran de chargement (pour masquer les potentiels blocages réseaux) avant de procéder au chargement complet.
- **Note US 8.1** : Au démarrage, le jeu appelle l'API de synchronisation (`GET /api/player/profile`) pour charger l'inventaire, les statistiques et les configurations du joueur identifié.

### Scoreboard
- Action : change la scène vers le tableau des scores  

**État actuel :**
- Contient des labels statiques  

**Évolution prévue :**
- Connexion à une base de données pour afficher les scores des joueurs  

**Navigation :**
- Bouton `Return` permettant de revenir au menu principal  

### Options
- Action : redirige vers la scène des options audio  

**Contenu :**
- Trois sliders :
  - Volume général (Master)  
  - Musique  
  - Effets sonores (SFX)  

**Navigation :**
- Bouton `Return` pour revenir au menu principal  

### Quit
- Action : ferme l’application  

## Implémentation audio (Options)

### Rôle du script

Le script `AudioOptions` permet de gérer dynamiquement le volume des différents canaux audio du jeu via des sliders.

### Fonctionnement

Trois sliders sont exposés dans l’éditeur :
- `masterSlider`  
- `musicSlider`  
- `sfxSlider`  

Le script récupère les indices des bus audio correspondants :
- Master  
- Music  
- SFX  

### Initialisation

À l’exécution
- Le volume actuel de chaque bus est récupéré  
- Converti de décibels vers une valeur linéaire  
- Appliqué aux sliders pour refléter l’état audio actuel  

### Interaction utilisateur

Chaque slider possède une fonction appelée lors d’un changement de valeur :

- Conversion de la valeur linéaire en décibels (pour un aspect plus logique de la diminution lorsqu'on bouge le slider)
- Mise à jour du volume du bus correspondant via `AudioServer`  
- Affichage d’un message de debug dans la console  

### Résumé simplifié

Ce script :
- Synchronise les sliders avec le volume actuel  
- Permet à l’utilisateur de modifier le volume en temps réel  
- Applique les modifications directement au système audio du moteur  