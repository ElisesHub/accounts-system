# Account System Frontend

## Présentation

Portfolio Frontend est l’application web exposée aux utilisateurs du système de gestion des comptes. Elle affiche l’interface utilisateur, gère l’authentification avec Auth0 et communique avec l’Accounts Application API via des requêtes HTTP.

Le projet suit les principes de la Clean Architecture et du Domain-Driven Design en séparant clairement les responsabilités liées à la présentation, à l’application, au domaine et à l’infrastructure.

Le frontend ne se connecte pas directement à la base de données. Les données des comptes sont récupérées via l’Accounts Application API.

## Architecture

```text
Portfolio Frontend
        ↓ HTTP
Accounts Application API
        ↓ HTTP
Accounts Database API
        ↓
Base de données MySQL
```

## Responsabilités

Ce projet est responsable des éléments suivants :

* Afficher l’interface utilisateur frontend
* Gérer l’authentification avec Auth0
* Appeler l’Accounts Application API via un client HTTP typé
* Afficher les données des comptes retournées par le backend
* Gérer les erreurs visibles par l’utilisateur et les pages liées aux codes de statut
* Maintenir une séparation claire entre les responsabilités de présentation, d’application, de domaine et d’infrastructure

## Structure du projet

```text
PortfolioFe
├── Application
├── Domain
├── Infrastructure
└── Presentation
```

Responsabilités typiques :

* `Presentation` — Razor Pages, contrôleurs MVC, vues, routage et préoccupations liées à l’interface utilisateur
* `Application` — services applicatifs, interfaces et logique des cas d’usage
* `Domain` — modèles de domaine et règles métier
* `Infrastructure` — clients HTTP externes, configuration et intégrations de sécurité

## Technologies

* .NET / ASP.NET Core
* Razor Pages
* Contrôleurs et vues MVC
* Clean Architecture
* Domain-Driven Design
* Authentification Auth0
* `HttpClient` typé
* Docker / Docker Compose

## Authentification

Le frontend utilise Auth0 pour l’authentification de l’application web.

Les valeurs de configuration suivantes sont requises :

```text
Auth0:Domain
Auth0:ClientId
Auth0:ClientSecret
```

Si l’une de ces valeurs est absente, l’application échoue au démarrage.

## Configuration et secrets

Ce dépôt ne contient aucun secret d’exécution ni aucune valeur de configuration propre à un environnement.

Pour le développement local, les secrets doivent être gérés avec les user secrets .NET. Ces valeurs sont stockées en dehors du dépôt et ne sont pas versionnées dans Git.

Lorsque l’application est exécutée dans le cadre du système complet de gestion des comptes, la configuration d’exécution est fournie par un dépôt de déploiement séparé via Docker Compose.

Le frontend prend également en charge les secrets montés dans les conteneurs via `/run/secrets`, lorsqu’ils sont fournis par l’environnement d’exécution. Cette option est facultative et principalement destinée aux déploiements conteneurisés.

## User secrets requis en local

Les user secrets .NET suivants sont requis pour le développement local :

```text
Auth0:Domain=
Auth0:ClientSecret=
Auth0:ClientId=
AccountsApplicationApiKey=
```

Initialiser les user secrets depuis le répertoire du projet frontend :

```bash
dotnet user-secrets init
```

Définir les valeurs requises :

```bash
dotnet user-secrets set "Auth0:Domain" "your-auth0-domain"
dotnet user-secrets set "Auth0:ClientSecret" "your-auth0-client-secret"
dotnet user-secrets set "Auth0:ClientId" "your-auth0-client-id"
dotnet user-secrets set "AccountsApplicationApiKey" "your-accounts-application-api-key"
```

## Configuration de l’API externe

Le frontend communique avec l’Accounts Application API via un client HTTP typé.

L’URL de base de l’Accounts Application API est configurée avec :

```text
ExternalApi:BaseUrl
```

Lors de l’exécution via le dépôt de déploiement séparé, cette valeur est injectée par Docker Compose et pointe généralement vers le nom du service Accounts Application API dans le réseau Docker.

Exemple :

```text
http://accountsapplicationapi:8080
```

`ExternalApi:BaseUrl` doit être une URI absolue valide.

## Configuration de la clé API

Le frontend a besoin d’une clé API pour communiquer avec l’Accounts Application API.

Valeur de configuration requise :

```text
AccountsApplicationApiKey
```

Cette valeur doit être fournie via les user secrets .NET en développement local, ou injectée par la configuration de déploiement lors de l’exécution avec Docker Compose.

## Exécution en local

Restaurer les dépendances :

```bash
dotnet restore
```

Lancer le projet frontend :

```bash
dotnet run
```

Si l’exécution se fait depuis la racine de la solution, fournir le chemin du projet :

```bash
dotnet run --project path/to/PortfolioFe
```

L’Accounts Application API doit également être démarrée et accessible via la valeur configurée dans `ExternalApi:BaseUrl`.

## Exécution avec Docker Compose

Ce projet est conçu pour être exécuté dans le cadre du système complet de gestion des comptes, via un dépôt de déploiement séparé.

Le dépôt de déploiement contient le fichier `docker-compose.yaml` utilisé pour démarrer ensemble le frontend, l’Accounts Application API, l’Accounts Database API et la base de données MySQL.

Depuis le dépôt de déploiement, exécuter :

```bash
docker compose up
```

Le dépôt de déploiement est responsable de la configuration d’exécution : URLs des services, clés API, paramètres Auth0, variables d’environnement et secrets Docker.

Ce dépôt frontend contient uniquement le code source de l’application. L’orchestration d’exécution, le câblage des services, les variables d’environnement et les secrets sont gérés en dehors de ce dépôt.

## Flux de requête

Une requête typique liée aux comptes suit le flux suivant :

```text
Utilisateur
  ↓
Portfolio Frontend
  ↓
IAccountsService
  ↓
IExternalAccountsClient
  ↓
Accounts Application API
```

Le frontend enregistre le service applicatif de comptes :

```csharp
builder.Services.AddScoped<IAccountsService, AccountsService>();
```

Il enregistre également un client HTTP typé pour communiquer avec l’Accounts Application API :

```csharp
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>();
```

Cela permet de découpler le frontend des détails d’implémentation de l’intégration HTTP avec le service en aval.

## Gestion des erreurs

Dans les environnements hors développement, l’application utilise un gestionnaire d’exceptions centralisé :

```text
/Error
```

Les réponses liées aux codes de statut, comme 404, 403 et 500, sont gérées via :

```text
/Error/StatusCode?code={statusCode}
```

Le pipeline de requête inclut :

```text
Fichiers statiques
Routage
Authentification
Autorisation
Razor Pages
Routes des contrôleurs MVC
```

## Validation de la configuration

L’application valide la configuration requise au démarrage.

Le démarrage échoue si :

* `Auth0:Domain` est absent
* `Auth0:ClientId` est absent
* `Auth0:ClientSecret` est absent
* `AccountsApplicationApiKey` est absent
* `ExternalApi:BaseUrl` est absent
* `ExternalApi:BaseUrl` n’est pas une URI absolue valide

Cela permet de détecter les problèmes de configuration avant que l’application ne commence à traiter des requêtes.

## Dépannage

### Le frontend ne parvient pas à récupérer les comptes

Vérifiez que :

* L’Accounts Application API est en cours d’exécution
* `ExternalApi:BaseUrl` pointe vers le bon service
* La clé API requise est configurée
* Le frontend peut joindre l’Accounts Application API via HTTP
* L’Accounts Application API peut joindre ses propres services en aval

### L’application échoue au démarrage

Vérifiez que toutes les valeurs de configuration requises sont présentes :

```text
Auth0:Domain
Auth0:ClientId
Auth0:ClientSecret
AccountsApplicationApiKey
ExternalApi:BaseUrl
```

Vérifiez également que `ExternalApi:BaseUrl` est une URI absolue valide.

### La connexion Auth0 ne fonctionne pas

Vérifiez que :

* Le domaine Auth0 est correct
* Le client ID et le client secret sont corrects
* L’URL de callback est correctement configurée dans Auth0
* L’URL de logout est correctement configurée dans Auth0
* L’application s’exécute bien sur l’hôte et le port attendus

## Notes de sécurité

Les secrets ne sont pas stockés dans ce dépôt.

Ne versionnez pas :

* Les client secrets Auth0
* Les clés API
* Les identifiants propres à un environnement
* Les valeurs de configuration de production
* Les fichiers de user secrets locaux
* Les fichiers de secrets générés

Pour le développement local, utilisez les user secrets .NET.

Pour l’exécution conteneurisée, les valeurs requises sont injectées par la configuration de déploiement via Docker Compose.

## Avertissement

Ce projet est un prototype simple créé uniquement à des fins de démonstration. Il est fourni « en l’état », sans aucune garantie.

L’auteur n’est pas responsable des problèmes pouvant résulter de l’utilisation, de la modification, du déploiement ou de la distribution de ce projet, y compris les pertes de données, les problèmes de sécurité ou les interruptions de service.

Ce projet n’est pas destiné à être utilisé tel quel dans un environnement de production. Avant tout déploiement public ou commercial, il convient de passer en revue la configuration de sécurité, la gestion des secrets, la configuration de la base de données, le flux d’authentification, la gestion des erreurs, les logs et les paramètres d’infrastructure.