# ProductApp —

## Présentation du projet

ProductApp est une application web de suivi de production et d'aide
à la décision commerciale, intégrée aux ressources SAP.

Le cas d'usage initial est une chaîne de production de biscuits,
mais l'application doit rester généralisable à d'autres produits industriels.

## Objectifs principaux

L'application doit permettre de :

1. gérer les produits industriels ;
2. définir les étapes de production ;
3. associer des ressources SAP aux étapes ;
4. calculer le coût de revient ;
5. calculer la marge estimée ;
6. générer un score de faisabilité commerciale ;
7. effectuer des simulations what-if ;
8. suivre les ordres et l'avancement de production ;
9. exporter des rapports PDF et Excel ;
10. gérer les utilisateurs, les rôles et la traçabilité.

## Architecture générale

Le projet utilise une Clean Architecture.

### Backend

- `backend/src/ProductApp.Domain`
- `backend/src/ProductApp.Application`
- `backend/src/ProductApp.Infrastructure`
- `backend/src/ProductApp.Api`

### Tests

- `backend/tests/ProductApp.Domain.Tests`
- `backend/tests/ProductApp.Application.Tests`
- `backend/tests/ProductApp.Infrastructure.Tests`
- `backend/tests/ProductApp.Api.Tests`

### Frontend

Le frontend se trouve dans `frontend`.

Technologies :

- React ;
- TypeScript ;
- Vite ;
- React Router ;
- Axios ;
- Tailwind CSS ou Bootstrap ;
- Recharts pour les graphiques.

## Règles de dépendance

Respecter strictement les dépendances suivantes :

- Domain ne dépend d'aucun autre projet.
- Application dépend uniquement de Domain.
- Infrastructure dépend de Application et Domain.
- Api dépend de Application et Infrastructure.
- Le frontend communique uniquement avec l'API REST.

Ne jamais référencer Infrastructure depuis Domain ou Application.

## Couche Domain

Domain contient :

- les entités métier ;
- les enums ;
- les value objects ;
- les exceptions métier ;
- les événements de domaine ;
- les règles métier pures.

Domain ne doit contenir aucune dépendance vers :

- Entity Framework Core ;
- ASP.NET Core ;
- SAP ;
- MediatR ;
- le frontend.

## Couche Application

Application contient :

- les commandes et requêtes CQRS ;
- les handlers MediatR ;
- les DTO ;
- les validateurs FluentValidation ;
- les interfaces de repository ;
- les interfaces SAP ;
- les services de calcul métier ;
- les interfaces de reporting ;
- les mappings.

Organiser les fonctionnalités par module et non uniquement par type technique.

Exemple :

ProductApp.Application/
  Products/
    Commands/
    Queries/
    DTOs/
  ProductionSteps/
  Resources/
  MarketAnalysis/
  SapIntegration/

## Couche Infrastructure

Infrastructure contient :

- Entity Framework Core ;
- ApplicationDbContext ;
- les configurations des entités ;
- les migrations ;
- les repositories ;
- l'authentification JWT ;
- l'intégration SAP ;
- les exports PDF et Excel ;
- Serilog ;
- les tâches de synchronisation SAP.

## Intégration SAP

Utiliser l'interface :

`ISapDataProvider`

Prévoir deux implémentations :

1. `MockSapDataProvider` pour le développement ;
2. une implémentation réelle avec OData ou SAP .NET Connector.

Ne jamais mettre directement la logique SAP dans les controllers.

Les secrets SAP ne doivent jamais être écrits dans le code source.

## Entités principales

Prévoir au minimum :

- Product ;
- ProductVersion ;
- ProductionStep ;
- StepResource ;
- SapMaterial ;
- Equipment ;
- ProductionOrder ;
- ProductionConfirmation ;
- MarketAnalysis ;
- WhatIfSimulation ;
- Notification ;
- AuditLog ;
- ApplicationUser.

## Rôles

Les rôles principaux sont :

- ProductionManager ;
- CommercialManager ;
- Administrator.

Appliquer les autorisations sur les endpoints sensibles.

## Backend conventions

- Utiliser C# avec nullable reference types activés.
- Utiliser async/await pour les opérations d'entrée-sortie.
- Accepter un CancellationToken dans les handlers et services asynchrones.
- Utiliser des DTO pour les entrées et sorties API.
- Ne jamais exposer directement les entités EF Core depuis les controllers.
- Utiliser FluentValidation pour valider les commandes.
- Utiliser MediatR pour les cas d'utilisation.
- Utiliser les codes HTTP appropriés.
- Centraliser la gestion des exceptions.
- Ajouter des logs utiles sans exposer de secrets.
- Ajouter des commentaires uniquement lorsque la logique est complexe.

## Frontend conventions

- Utiliser uniquement TypeScript.
- Ne pas utiliser le type `any` sans justification.
- Séparer les pages, composants, hooks, services API et types.
- Mettre les appels HTTP dans `frontend/src/api`.
- Utiliser des composants réutilisables.
- Gérer les états de chargement, erreur et absence de données.
- Protéger les routes selon le rôle utilisateur.
- Ne jamais stocker de mot de passe.
- Utiliser des variables d'environnement pour l'URL de l'API.

## Base de données

Utiliser SQL Server avec Entity Framework Core.

Les migrations doivent être placées dans ProductApp.Infrastructure.

Prévoir :

- les clés étrangères ;
- les contraintes ;
- les index utiles ;
- l'audit des créations et modifications ;
- les types décimaux adaptés aux montants et quantités.

## Tests

Ajouter des tests pour :

- le calcul du coût de revient ;
- le calcul de marge ;
- le score de faisabilité ;
- les validations ;
- les handlers Application ;
- les endpoints critiques ;
- le mock SAP.

Utiliser :

- xUnit ;
- Moq ;
- FluentAssertions si installé.

## Sécurité

- Utiliser JWT pour l'authentification.
- Utiliser des rôles ou politiques d'autorisation.
- Ne jamais écrire de secret dans Git.
- Ne jamais enregistrer les mots de passe en clair.
- Ne jamais afficher les identifiants SAP dans les logs.
- Valider toutes les données venant de l'utilisateur.
- Configurer CORS uniquement pour les origines nécessaires.

## Méthode de travail obligatoire

Avant toute modification importante :

1. analyser la structure existante ;
2. expliquer brièvement le plan ;
3. identifier les fichiers concernés ;
4. ne modifier que les fichiers nécessaires.

Après les modifications :

1. exécuter `dotnet restore` si nécessaire ;
2. exécuter `dotnet build backend/ProductApp.sln` ;
3. exécuter `dotnet test backend/ProductApp.sln` ;
4. exécuter `npm run build --prefix frontend` ;
5. afficher les erreurs restantes ;
6. résumer les fichiers modifiés.

Ne pas prétendre qu'une commande a réussi si elle n'a pas réellement été exécutée.

## Restrictions

- Ne pas modifier plusieurs modules sans nécessité.
- Ne pas supprimer du code existant sans explication.
- Ne pas remplacer toute l'architecture pour corriger un petit problème.
- Ne pas installer de package inutile.
- Ne pas créer des fichiers vides sans contenu compilable.
- Ne pas implémenter l'accès SAP réel sans informations de connexion.
- Utiliser MockSapDataProvider tant que SAP réel n'est pas disponible.
