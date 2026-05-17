# Projet Tests (Validation Qualité & CI)

## 📌 Responsabilité Unique
Ce répertoire garantit la stabilité et la non-régression de l'ensemble de l'architecture N-Tiers. Il centralise les tests automatisés (Unitaires et Intégration) pour le Core, l'API et la logique pure du jeu.

## ✅ Composants Autorisés
- Projets de tests XUnit/NUnit (`UnitTests/`, `IntegrationTests/`)
- Mocks, Fakes et Stubs (via Moq ou équivalent).
- Données de tests (Fixtures).

## 🚫 Dépendances Interdites
- Ne pas introduire de logique métier dans les tests. Les tests ne font que vérifier les contrats existants.

## 💡 Conseils pour l'équipe (et l'IA)
- **Couverture exhaustive** : Chaque méthode publique du `Core` doit faire l'objet de tests, y compris les cas limites et les méthodes agissant volontairement dans le vide (no-op).
- **CI/CD** : Ces tests sont critiques car ils conditionnent le déploiement. Ils s'exécutent lors de l'étape de Validation (Stage 1) du pipeline (`IS-EXE-CI-Pipeline.yaml`). Si un test échoue, le jeu n'est pas compilé.
- Pour Godot C#, si un test valide des effets de bord sur l'`EventBus`, il faut simuler le passage d'une frame ou traiter manuellement la queue des événements.
