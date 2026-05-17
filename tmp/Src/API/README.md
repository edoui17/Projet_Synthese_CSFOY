# Projet API (Couche d'Exposition)

## 📌 Responsabilité Unique
Ce répertoire contient l'API ASP.NET Core. Son rôle strict est de servir de passerelle d'exposition des données et de point de communication réseau entre les clients (Jeu Godot, Dashboard Web) et la persistance (Base de données).
**L'API n'héberge aucune logique métier complexe ni mécanique de jeu.**

## ✅ Composants Autorisés
- Contrôleurs REST (`Controllers/`)
- Middlewares et filtres d'authentification (`Middleware/`)
- Configuration des services d'injection de dépendances (`Program.cs`)
- DTOs (Data Transfer Objects) spécifiques à l'API si le format d'exposition diffère du modèle Core.

## 🚫 Dépendances Interdites
- **Godot** : Aucune référence aux bibliothèques du moteur de jeu.
- **Logique Métier** : Ne pas recréer d'algorithmes (ex: calcul d'XP) ici, utiliser le projet `Core`.

## 💡 Conseils pour l'équipe (et l'IA)
- Maintenez les contrôleurs aussi fins que possible (*Thin Controllers*). Ils doivent se contenter de recevoir la requête, d'appeler le service approprié dans `Core` ou `Infrastructure`, puis de formater la réponse.
- Vérifiez toujours que la sécurité (CORS, JWT) est appliquée au niveau des endpoints sensibles.
