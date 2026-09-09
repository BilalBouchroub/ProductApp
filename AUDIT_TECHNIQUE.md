# ProductApp — Rapport d’audit technique

Date de l’audit : 31 juillet 2026

## Verdict

ProductApp possède une base cohérente et compilable pour une soutenance : Clean Architecture respectée, API REST sécurisée, persistance SQL Server, CQRS/MediatR, validation centralisée et frontend React strict. Les quatre projets de tests passent intégralement.

L’application est démontrable sur le workflow Produit → Production → Expérience → Étude commerciale → Optimisation. Quelques capacités annoncées dans la vision globale restent des extensions et sont listées sans être présentées comme terminées.

## Architecture finale

~~~mermaid
flowchart LR
    UI[React + TypeScript<br/>Router · Query · Axios] -->|REST / JWT| API[ProductApp.Api<br/>Controllers · Policies · ProblemDetails]
    API --> APP[ProductApp.Application<br/>CQRS · DTO · Validators · Ports]
    APP --> DOMAIN[ProductApp.Domain<br/>Entités · Règles · Calculateurs]
    API --> INFRA[ProductApp.Infrastructure<br/>EF Core · Identity · JWT · Repositories]
    INFRA --> APP
    INFRA --> DOMAIN
    INFRA --> SQL[(SQL Server)]
    INFRA --> SAP[ISapDataProvider<br/>Mock SAP]
~~~

Les dépendances de projets ont été vérifiées :

- Domain : aucune référence vers une autre couche et aucun package d’infrastructure ;
- Application : référence uniquement Domain ;
- Infrastructure : références Application et Domain ;
- Api : références Application et Infrastructure ;
- React : communique avec l’API via un client Axios centralisé, avec mode démonstration explicite.

## Diagramme fonctionnel

~~~mermaid
flowchart TD
    AUTH[Authentification et autorisations] --> ADMIN[Administration]
    AUTH --> PROD[Production]
    AUTH --> COM[Commercial]
    ADMIN --> USERS[Utilisateurs · rôles · permissions]
    ADMIN --> AUDIT[Audit · notifications]
    PROD --> PRODUCTS[Produits · versions]
    PRODUCTS --> CHAIN[Étapes · ressources SAP]
    CHAIN --> EXP[Expériences]
    EXP --> READY[Prêt pour étude]
    READY --> STUDY[Étude de marché]
    STUDY --> SCORES[Scores · recommandation]
    SCORES --> OPT[Demande d’optimisation]
    OPT --> PROD
    USERS --> AUDIT
    PRODUCTS --> AUDIT
    STUDY --> AUDIT
~~~

## Résultats de l’audit

| Domaine | État | Éléments vérifiés |
|---|---|---|
| Clean Architecture / SOLID | Conforme | Sens des dépendances, interfaces côté Application, règles pures côté Domain |
| CQRS | Conforme | 42 commandes, 18 requêtes, handlers MediatR et validateurs de commandes |
| Repository Pattern | Conforme | Contrats Application, implémentations EF Core Infrastructure |
| EF Core / SQL Server | Conforme | Provider SQL Server, migration initiale appliquée, configurations séparées |
| Modèle SQL | Conforme | 24 tables, 67 clés étrangères, 115 index, décimaux et contraintes |
| JWT / Identity | Conforme | access token, refresh rotation/révocation, logout, reset/changement de mot de passe |
| Autorisation | Conforme | rôles, permissions, policies, fallback authentifié, rate limiting login |
| API REST | Conforme au périmètre | 19 controllers, 62 actions HTTP, pagination/recherche/tri selon les ressources |
| Validation / erreurs | Conforme | FluentValidation, pipeline MediatR, ProblemDetails centralisé |
| DTO / mapping | Conforme | entités EF non exposées par les controllers, mappings centralisés |
| Logs / notifications | Conforme | audit automatique des entités et notifications de workflow |
| Calculs métier | Conforme | coûts, ressources, expériences, scores commercial/financier/risque/global |
| Frontend React | Conforme | TypeScript strict, routes par rôle, Axios, TanStack Query, composants réutilisables |
| Responsive / accessibilité | Conforme au contrôle source | breakpoints, labels, erreurs annoncées, focus visible, piège de focus Modal/Drawer |
| Performance | Amélioré | routes lazy, mocks chargés à la demande, cache Query, AsNoTracking/AsSplitQuery |
| Tests | Conforme backend | 95 tests exécutés, 95 réussis, 0 ignoré |

## Corrections et compléments réalisés

### Backend

- remplacement du provider PostgreSQL résiduel par EF Core SQL Server ;
- création d’une migration SQL Server propre et application sur la base locale ;
- ajout du manifeste local dotnet-ef 10.0.10 pour restaurer les outils de migration ;
- correction des cascades multiples via DeleteBehavior.NoAction sur les relations d’audit ;
- enrichissement du contrat Produit avec les données techniques réellement utilisées par React ;
- suppression du N+1 frontend lors du chargement des produits et optimisation des graphes EF avec AsSplitQuery ;
- audit automatique CreatedAt, UpdatedAt, acteurs et logs d’entités ;
- notifications de création produit, publication, expérience et validation commerciale ;
- finalisation du workflow OptimizationRequest : domaine, CQRS, validation, repository, endpoint et notification ;
- validation explicite de toutes les commandes CQRS ;
- rate limiting des endpoints sensibles, en-têtes de sécurité, Swagger limité au développement ;
- suppression du controller commercial dupliqué et harmonisation des routes d’études.

### Frontend

- client Axios central, JWT et traitement central des erreurs HTTP ;
- services API Produits, Expériences, Utilisateurs, Études, Notifications, Audit et Optimisations ;
- bascule API/mocks par VITE_USE_MOCKS, sans charger le repository mock en mode API ;
- invalidation TanStack Query après mutations ;
- routeur organisé par groupes de rôles et chargement différé des pages ;
- correction des champs Produit pour utiliser les vraies réponses API ;
- focus clavier et fermeture Escape des Modal/Drawer, erreurs de champs annoncées ;
- suppression de fichiers morts et réduction du bundle principal à environ 485 kB brut.

## Statistiques

- 160 fichiers C# source, environ 12 476 lignes ;
- 198 fichiers TypeScript/TSX source, environ 2 632 lignes ;
- 24 DbSet et 24 configurations d’entités ;
- 19 controllers et 62 actions REST ;
- 42 commandes CQRS et 18 requêtes CQRS ;
- 27 routes métier React, plus login, unauthorized et not-found ;
- 95 tests backend réussis.

## Validation exécutée

- dotnet restore backend/ProductApp.sln : réussi ;
- dotnet build backend/ProductApp.sln : réussi, 0 warning, 0 erreur ;
- dotnet test backend/ProductApp.sln : 95/95 réussis ;
- dotnet format avec vérification : réussi ;
- npm install : réussi ;
- npm run lint : réussi ;
- npm run build : réussi ;
- migration SQL Server : appliquée ;
- smoke test : health 200, login administrateur JWT réussi, endpoint Produits authentifié réussi.

## Éléments restant éventuellement à faire

Ces éléments ne bloquent pas la démonstration du socle actuel, mais ne doivent pas être annoncés comme finalisés :

1. fournir un service e-mail/SMS réel pour Forgot Password ; le notifier actuel est volontairement un adaptateur de développement sans exposition du token ;
2. remplacer les dashboards et le constructeur de chaîne encore alimentés par les mocks lorsque leurs contrats API détaillés seront stabilisés ;
3. ajouter les ordres/confirmations de production et le module de simulation what-if annoncés dans la vision produit ;
4. implémenter les exports serveur PDF/Excel ; l’export commercial actuel reste simulé côté navigateur ;
5. implémenter le provider SAP réel après fourniture d’une URL OData ou du SAP .NET Connector et des secrets ;
6. ajouter une suite de tests frontend (Vitest/Testing Library) et un audit automatisé axe/Lighthouse ;
7. déployer les clés Data Protection dans un stockage partagé et la clé JWT dans un coffre de secrets ;
8. traiter l’avis npm React Router dès qu’une version corrigée est publiée. L’avis actuel vise le mode RSC, non utilisé par cette SPA ; le downgrade proposé par npm réintroduit d’autres avis.

## Préparation de soutenance

Pour une démonstration reproductible, configurer la chaîne SQL Server, injecter Jwt__SigningKey par secret, appliquer la migration, démarrer l’API puis le frontend avec VITE_API_BASE_URL. Utiliser VITE_USE_MOCKS=true uniquement pour le scénario de démonstration hors connexion.
