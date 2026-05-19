# Projet Web (Tableau de Bord / UI)

## 📌 Responsabilité Unique
Ce répertoire contient l'application Blazor Web App. Son rôle est de fournir une interface utilisateur déportée (ex: tableaux de bord d'administration, classements en ligne) aux administrateurs ou joueurs.

## ✅ Composants Autorisés
- Composants Razor (`.razor`)
- Fichiers statiques web (`wwwroot/`, CSS, JS)
- Services front-end d'appel HTTP vers l'API.

## 🚫 Dépendances Interdites
- **Accès direct BDD** : Le Web ne doit jamais inclure Entity Framework ou le projet `Infrastructure`. Il doit impérativement consommer les données via le projet `API`.
- **Logique de Jeu** : Aucun composant lié à la boucle de rendu de Godot.

## 💡 Conseils pour l'équipe (et l'IA)
- Ce projet dépend du projet `Core` pour partager les Modèles (DTOs/Domaine) et éviter la duplication des structures de données.
- Privilégiez une architecture par composants pour une réutilisation maximale dans l'interface utilisateur.
