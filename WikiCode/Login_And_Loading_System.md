# Système de Connexion et de Chargement (Login & Loading)

## Vue d'Ensemble
Le flux de connexion et de chargement initial d'IslandSurvivor agit comme la porte d'entrée de l'application. Ce système gère l'authentification de l'utilisateur, la validation de la session, la récupération du profil via l'API, ainsi que le mode hors-ligne. Il assure une transition fluide entre l'écran de connexion (`LoginScreen`), l'écran de chargement (`LoadingScreen`), et le menu principal.

## Architecture N-Tier (Accountant vs. Orchestrator)
Ce système respecte la stricte séparation entre le Core (Accountant) et le client Godot (Orchestrator) :

*   **Le Core (C# Pur) :**
    *   **`LoginValidator` :** Valide les entrées locales (longueur, caractères) selon des constantes explicites (`MAX_USERNAME_LENGTH`, etc.) pour prévenir les erreurs de sécurité ou les requêtes invalides.
    *   **`IApiService` :** L'interface chargée d'interagir avec le backend distant. Elle expose des méthodes pour l'authentification (`LoginAsync`) et la récupération des données utilisateur (`GetProfileAsync`). L'implémentation concrète gère les requêtes HTTP.
*   **L'Orchestrator (Godot Client) :**
    *   **`LoginScreen` :** Collecte les données de l'utilisateur, appelle le validateur local, puis délègue la requête de connexion à l'API via le `ServiceRegistry`.
    *   **`LoadingScreen` :** Un calque d'interface (`CanvasLayer`) qui affiche l'état actuel de la synchronisation (spinner, messages de statut) et bloque les interactions de la scène en arrière-plan.
    *   **`GameManager` :** Le contrôleur global (Autoload) qui orchestre le flux complet. Il valide la session, récupère le profil et décide de la scène à charger.

## Flux de Connexion (`LoginScreen`)
1.  **Validation Locale :** L'utilisateur entre ses identifiants. Le `LoginScreen` appelle `LoginValidator.Validate(username, password)`. Si les données sont invalides, une erreur est affichée sans requête réseau.
2.  **Requête Réseau :** Si la validation locale passe, les inputs de l'UI sont verrouillés et `ApiService.LoginAsync` est appelé.
3.  **Résultat :**
    *   Si le serveur retourne un token, celui-ci est stocké via `SessionProvider.StoreToken(token)`.
    *   Le `LoginScreen` cède ensuite le contrôle au `GameManager` en appelant `GameManager.Instance.InitializeGameAsync()`.
    *   En cas d'erreur réseau, un message adapté est affiché.

## Flux d'Initialisation (`GameManager` & `LoadingScreen`)
La méthode `InitializeGameAsync` du `GameManager` est responsable du chargement sécurisé des données du joueur.

1.  **Vérification du Token :** Le `GameManager` vérifie s'il existe un token stocké. Si ce n'est pas le cas, le joueur est redirigé vers le `LoginScreen`.
2.  **Affichage du Chargement :** S'il y a un token, le `GameManager` instancie le `LoadingScreen` et fige la scène active (`ProcessModeEnum.Disabled`).
3.  **Validation de Session :** Il appelle `ApiService.GetProfileAsync()`.
    *   **Succès :** Le profil (`ProfileResponse`) est validé par `ProfileValidator`. S'il est intègre, il est mappé en domaine métier (`ProfileMapper.MapToDomain`) et un `ProfileLoadedEvent` est publié via l'**EventBus**.
    *   **Données Corrompues :** Si les données retournées par le serveur semblent altérées, l'application repousse la réponse (`INTEGRITY ALERT`) et force le basculement en mode hors-ligne pour protéger le client.
    *   **Token Expiré/Invalide :** Le token est effacé et l'utilisateur retourne à la page de connexion.
4.  **Transition :** Une fois l'événement publié, le `GameManager` charge le `MainMenu`.

## Mode Hors-Ligne (Guest Mode)
IslandSurvivor prévoit une stratégie de résilience lorsque les serveurs sont inaccessibles.

1.  **Déclenchement :** L'utilisateur peut cliquer sur le bouton "Hors ligne" sur le `LoginScreen` ou le `LoadingScreen` (après échec de connexion). Cela émet un signal via le `SignalManager` (`OfflineModeRequested`).
2.  **Récupération du Cache :** Le `GameManager` intercepte la requête, passe en `GuestMode`, et interroge `ApiService.GetCachedProfile()`.
3.  **Profil par Défaut :** S'il n'y a pas de profil en cache, le système génère un profil par défaut (`Username = "Guest"`).
4.  **Lancement :** Un `ProfileLoadedEvent` est émis pour initialiser le reste des systèmes, et le joueur est redirigé vers le menu principal.
