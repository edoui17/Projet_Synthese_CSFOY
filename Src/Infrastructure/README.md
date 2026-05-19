# Projet Infrastructure (Persistance & Accès aux Données)

## 📌 Responsabilité Unique
Ce répertoire gère exclusivement l'implémentation technique de l'accès aux données, des requêtes SQL et de la persistance via Entity Framework Core. C'est l'interface avec notre base de données.

## ✅ Composants Autorisés
- Contexte de base de données (`AppDbContext.cs`)
- Implémentations concrètes des Repositories (`Repositories/`)
- Fichiers de migration Entity Framework (`Migrations/`)
- Entités spécifiques à la base de données (si un mapping séparé du `Core` est jugé nécessaire).

## 🚫 Dépendances Interdites
- **Interface Utilisateur & Jeu** : Aucune référence directe à `IslandSurvivor` ou `Web`.
- **Logique Métier** : Ne pas inclure de règles de calcul métier dans les Repositories, ces derniers ne doivent faire que du CRUD ou des requêtes optimisées.

## 💡 Conseils pour l'équipe (et l'IA)
- Utilisez l'Injection de Dépendances pour fournir les Repositories au reste de l'application via les interfaces définies dans `Core`.
- Soyez attentifs aux performances : favorisez les requêtes asynchrones et surveillez le comportement de tracking d'EF Core.
