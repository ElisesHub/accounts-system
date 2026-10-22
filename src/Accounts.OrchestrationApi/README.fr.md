# Accounts Application API

## Présentation

Accounts Application API est l’API exposée côté application pour le système de gestion des comptes. Elle se situe entre le Portfolio Frontend et l’Accounts Database API.

Le service reçoit les requêtes HTTP envoyées par le frontend, valide l’accès à l’aide d’une clé API, puis transmet les requêtes liées aux comptes à l’Accounts Database API via HTTP.

L’application ne se connecte pas directement à la base de données. L’accès aux données est pris en charge par l’Accounts Database API.

## Architecture

```text
Portfolio Frontend
        ↓ HTTP + clé API
Accounts Application API
        ↓ HTTP + clé API
Accounts Database API
        ↓
Base de données MySQL
```

## Responsabilités

Ce projet est responsable des éléments suivants :

* Exposer au frontend des endpoints HTTP liés aux comptes
* Authentifier les requêtes à l’aide d’une clé API
* Appeler l’Accounts Database API via un client HTTP typé
* Retourner les données des comptes au frontend
* Gérer les erreurs de validation avec un format de réponse cohérent
* Gérer les exceptions inattendues via un gestionnaire d’exceptions global
* Fournir un endpoint de health check
* Maintenir une séparation claire entre le frontend et les préoccupations liées à l’accès aux données

## Structure du projet

```text
AccountsApplicationAPI
├── Application
├── Domain
├── Infrastructure
└── Presentation
```

Responsabilités typiques :

* `Domain` — modèles métier liés aux comptes et règles métier principales
* `Application` — orchestration des cas d’usage, interfaces de services et services applicatifs
* `Infrastructure` — clients HTTP pour les API en aval, validation des clés API, configuration et intégrations externes
* `Presentation` — contrôleurs API, configuration de l’authentification, politiques d’autorisation, réponses d’erreur, gestion des exceptions et modèles de requête/réponse

## Technologies

* .NET / ASP.NET Core Web API
* Clean Architecture
* Domain-Driven Design
* Authentification par clé API
* `HttpClient` typé
* Swagger / OpenAPI
* Health checks
* Docker / Docker Compose

## Authentification

L’Accounts Application API utilise une authentification par clé API.

Les requêtes doivent inclure la clé API dans l’en-tête HTTP suivant :

```text
x-api-key
```

Tous les contrôleurs mappés requièrent la politique d’autorisation `RequireApiKey`.

```csharp
app.MapControllers().RequireAuthorization("RequireApiKey");
```

Si la clé API est absente ou invalide, la requête est rejetée avant d’atteindre l’action du contrôleur.

## Configuration

L’Accounts Application API utilise la configuration provenant de `appsettings.json`, `appsettings.Development.json`, des variables d’environnement, des user secrets .NET et, de manière optionnelle, des fichiers de secrets montés dans le conteneur.

L’URL de base de l’Accounts Database API n’est pas considérée comme un secret. En développement local, elle est configurée dans `appsettings.Development.json`.

Exemple :

```json
{
  "AccountsDbApi": {
    "BaseUrl": "http://localhost:5253"
  }
}
```

Lors d’une exécution avec Docker Compose, cette même valeur est fournie par le dépôt de déploiement sous forme de variable d’environnement.

Dans Docker Compose, les clés de configuration .NET imbriquées sont généralement représentées avec des doubles underscores :

```yaml
environment:
  AccountsDbApi__BaseUrl: "http://accountsapi:8080"
```

La valeur doit être une URI absolue valide.

## Secrets

Ce dépôt ne contient aucun secret d’exécution.

Pour le développement local, les secrets doivent être gérés avec les user secrets .NET. Ces valeurs sont stockées en dehors du dépôt et ne sont pas versionnées dans Git.

Lorsque l’application est exécutée dans le cadre du système complet de gestion des comptes, les valeurs sensibles sont fournies par un dépôt de déploiement séparé via Docker Compose.

L’application prend également en charge les secrets montés dans les conteneurs via `/run/secrets`, lorsqu’ils sont fournis par l’environnement d’exécution. Cette option est facultative et principalement destinée aux déploiements conteneurisés.

## User secrets requis en local

Les user secrets .NET suivants sont requis pour le développement local :

```text
AccountsApplicationApiKey=
AccountsApiKey=
```

Initialiser les user secrets depuis le répertoire du projet Accounts Application API :

```bash
dotnet user-secrets init
```

Définir les valeurs requises :

```bash
dotnet user-secrets set "AccountsApplicationApiKey" "your-accounts-application-api-key"
dotnet user-secrets set "AccountsApiKey" "your-accounts-api-key"
```

Ne versionnez jamais de vraies clés API ni d’identifiants propres à un environnement.

## Configuration des clés API

Ce service utilise deux valeurs de clé API :

```text
AccountsApplicationApiKey
AccountsApiKey
```

`AccountsApplicationApiKey` est utilisée pour valider les requêtes entrantes provenant du frontend.

`AccountsApiKey` est utilisée lors des requêtes sortantes vers l’Accounts Database API.

Les deux valeurs sont obligatoires. Si l’une des deux clés est absente, l’application échoue au démarrage.

## Configuration de l’API en aval

L’Accounts Application API communique avec l’Accounts Database API via un client HTTP typé.

L’URL de base de l’API en aval est configurée avec :

```text
AccountsDbApi:BaseUrl
```

En développement local, cette valeur est définie dans `appsettings.Development.json` :

```json
{
  "AccountsDbApi": {
    "BaseUrl": "http://localhost:5253"
  }
}
```

Lors de l’exécution via le dépôt de déploiement séparé, cette valeur est injectée par Docker Compose sous forme de variable d’environnement :

```text
AccountsDbApi__BaseUrl
```

Exemple de valeur Docker Compose :

```text
http://accountsapi:8080
```

`AccountsDbApi:BaseUrl` doit être une URI absolue valide.

## Endpoints de l’API

### Health check

```http
GET /health
```

Retourne l’état de santé du service.

### Comptes

Le service expose des endpoints liés aux comptes via ses contrôleurs.

Endpoint de compte attendu :

```http
GET /api/accounts
```

Cet endpoint récupère les données des comptes en appelant l’Accounts Database API.

## Flux de requête

Une requête typique suit le flux suivant :

```text
Portfolio Frontend
  ↓
validation x-api-key
  ↓
Contrôleur Accounts Application API
  ↓
IAccountsService
  ↓
IExternalAccountsClient
  ↓
Accounts Database API
```

L’application enregistre le service de comptes :

```csharp
builder.Services.AddScoped<IAccountsService, AccountsService>();
```

Elle enregistre également un client HTTP typé pour communiquer avec l’Accounts Database API :

```csharp
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>();
```

Cela permet de découpler la couche applicative des détails de l’intégration HTTP avec le service en aval.

## Gestion des erreurs

L’application utilise un gestionnaire d’exceptions global :

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseExceptionHandler();
```

Les erreurs inattendues sont gérées de manière centralisée et retournées avec un format de réponse d’erreur API cohérent.

## Erreurs de validation

Les réponses liées à un état de modèle invalide sont personnalisées.

Les erreurs de validation retournent une réponse structurée contenant :

```text
Code
Message
FieldErrors
TraceId
```

Exemple de structure de réponse pour une erreur de validation :

```json
{
  "code": "ValidationError",
  "message": "One or more validation errors occurred.",
  "fieldErrors": {
    "fieldName": [
      "Validation error message"
    ]
  },
  "traceId": "request-trace-id"
}
```

## Swagger

Swagger est activé uniquement dans les environnements de développement.

Lorsque l’application s’exécute en environnement de développement, Swagger UI est disponible via l’endpoint Swagger configuré.

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Swagger n’est pas activé dans les environnements hors développement.

## Exécution en local

Restaurer les dépendances :

```bash
dotnet restore
```

Lancer le projet API :

```bash
dotnet run
```

Si l’exécution se fait depuis la racine de la solution, fournir le chemin du projet :

```bash
dotnet run --project path/to/AccountsApplicationAPI
```

L’Accounts Database API doit également être démarrée et accessible via la valeur configurée dans `AccountsDbApi:BaseUrl`.

## Exécution avec Docker Compose

Ce projet est conçu pour être exécuté dans le cadre du système complet de gestion des comptes, via un dépôt de déploiement séparé.

Le dépôt de déploiement contient le fichier `docker-compose.yaml` utilisé pour démarrer ensemble le Portfolio Frontend, l’Accounts Application API, l’Accounts Database API et la base de données MySQL.

Depuis le dépôt de déploiement, exécuter :

```bash
docker compose up
```

Le dépôt de déploiement est responsable de la configuration d’exécution : URLs des services, clés API, variables d’environnement et secrets Docker.

Pour Docker Compose, l’URL de base de l’Accounts Database API est fournie sous forme de variable d’environnement :

```text
AccountsDbApi__BaseUrl
```

Ce dépôt contient uniquement le code source de l’Accounts Application API. L’orchestration d’exécution, le câblage des services, les variables d’environnement et les secrets sont gérés en dehors de ce dépôt.

## Validation de la configuration

L’application valide la configuration requise au démarrage.

Le démarrage échoue si :

* `AccountsApplicationApiKey` est absent
* `AccountsApiKey` est absent
* `AccountsDbApi:BaseUrl` est absent
* `AccountsDbApi:BaseUrl` n’est pas une URI absolue valide

Cela permet de détecter les problèmes de configuration avant que l’application ne commence à traiter des requêtes.

## Dépannage

### L’API échoue au démarrage

Vérifiez que toutes les valeurs de configuration requises sont présentes :

```text
AccountsApplicationApiKey
AccountsApiKey
AccountsDbApi:BaseUrl
```

Vérifiez également que `AccountsDbApi:BaseUrl` est une URI absolue valide.

### Les requêtes retournent une erreur non autorisée

Vérifiez que la requête inclut l’en-tête de clé API requis :

```text
x-api-key
```

Vérifiez également que la clé fournie correspond à la valeur configurée dans `AccountsApplicationApiKey`.

### L’API ne parvient pas à récupérer les comptes

Vérifiez que :

* L’Accounts Database API est en cours d’exécution
* `AccountsDbApi:BaseUrl` pointe vers le bon service Accounts Database API
* La valeur configurée dans `AccountsApiKey` est valide
* L’Accounts Application API peut joindre l’Accounts Database API via HTTP
* L’Accounts Database API peut se connecter à la base MySQL

### Swagger n’est pas disponible

Swagger est activé uniquement dans les environnements de développement.

Vérifiez que l’application s’exécute avec l’environnement de développement :

```text
ASPNETCORE_ENVIRONMENT=Development
```

## Notes de sécurité

Les secrets ne sont pas stockés dans ce dépôt.

Ne versionnez pas :

* Les clés API
* Les identifiants propres à un environnement
* Les valeurs de configuration de production
* Les fichiers de user secrets locaux
* Les fichiers de secrets générés

Pour le développement local, utilisez les user secrets .NET.

Pour l’exécution conteneurisée, les valeurs sensibles requises sont injectées par la configuration de déploiement via Docker Compose.

L’URL de base de l’Accounts Database API est une valeur de configuration, pas un secret.

## Avertissement

Ce projet est un prototype simple créé uniquement à des fins de démonstration. Il est fourni « en l’état », sans aucune garantie.

L’auteur n’est pas responsable des problèmes pouvant résulter de l’utilisation, de la modification, du déploiement ou de la distribution de ce projet, y compris les pertes de données, les problèmes de sécurité ou les interruptions de service.

Ce projet n’est pas destiné à être utilisé tel quel dans un environnement de production. Avant tout déploiement public ou commercial, il convient de passer en revue la configuration de sécurité, la gestion des secrets, le flux d’authentification, la configuration de l’API en aval, la gestion des erreurs, les logs et les paramètres d’infrastructure.