# Accounts Data API

Le service orienté base de données du [système de gestion des comptes](../../README.fr.md). C’est le seul composant qui se connecte à MySQL ; l’Orchestration API l’appelle en HTTP avec une clé API.

```text
Orchestration API  →  Accounts Data API  →  MySQL
```

## Responsabilités

- Exposer les endpoints liés aux comptes à l’Orchestration API
- Authentifier les requêtes à l’aide d’une clé API
- Exécuter les requêtes sur la base MySQL `AccountsDb`
- Retourner les erreurs de validation dans un format cohérent
- Gérer les exceptions inattendues via un gestionnaire d’exceptions global
- Fournir un health check tenant compte de l’état de la base de données

## Structure du projet

```text
src/Accounts.DataApi/
├── AccountsAPI/
│   ├── Domain/           Modèles métier des comptes et règles métier
│   ├── Application/      Interfaces et services applicatifs
│   ├── Infrastructure/   Repositories MySQL, validation de la clé API, configuration
│   └── Presentation/     Contrôleurs, authentification, gestion des erreurs, modèles de requête/réponse
└── AccountsAPI.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

Les requêtes suivent le flux Contrôleur → `IAccountsService` → `IAccountsRepository` → MySQL, ce qui sépare la logique des contrôleurs, la logique applicative et l’accès aux données.

## Technologies

ASP.NET Core Web API (.NET 10), Clean Architecture, Domain-Driven Design, MySQL avec MySqlConnector, authentification par clé API, OpenAPI/Swagger, health checks, xUnit v3, Moq, FluentAssertions.

## Authentification

Tous les contrôleurs requièrent la politique `RequireApiKey` :

```csharp
app.MapControllers().RequireAuthorization("RequireApiKey");
```

Les requêtes doivent transmettre la clé dans l’en-tête `x-api-key`. Une clé absente ou invalide est rejetée avant d’atteindre le contrôleur. `GET /health` n’est pas authentifié afin que Docker puisse vérifier l’état du conteneur.

## Configuration

| Clé | Rôle |
|---|---|
| `ConnectionStrings:AccountsDb` | Chaîne de connexion MySQL |
| `AccountsApiKey` | Clé attendue de la part de l’Orchestration API |

L’API échoue au démarrage si l’une de ces valeurs est absente.

**En développement local**, elles sont gérées avec les user secrets .NET :

```bash
cd src/Accounts.DataApi/AccountsAPI
dotnet user-secrets set "ConnectionStrings:AccountsDb" "Server=localhost;Port=3306;Database=AccountsDb;Uid=accounts_user;Pwd=yourpassword;"
dotnet user-secrets set "AccountsApiKey" "your-accounts-api-key"
```

**Dans Docker**, les deux valeurs sont des secrets Docker montés dans `/run/secrets` et lus par le provider de configuration key-per-file. Le secret contenant la chaîne de connexion est monté sous le nom cible `ConnectionStrings:AccountsDb`, ce qui permet au code de le lire exactement comme en local :

```csharp
builder.Configuration.GetConnectionString("AccountsDb")
```

Voir le [README racine](../../README.fr.md) pour la mise en place du dossier `secrets/`.

## Base de données

L’API attend le schéma `AccountsDb`, y compris ses procédures stockées. Dans Docker, il est créé à partir de `mySQLDb/accountsdb_dump_v1.sql` lors de la première création du volume de base de données.

## Endpoints

| Méthode | Chemin | Auth | Description |
|---|---|---|---|
| GET | `/health` | Aucune | Vérifie que l’API peut joindre la base de données |
| GET | `/api/accounts` | Clé API | Retourne tous les comptes |

Swagger UI est activé uniquement dans l’environnement Development.

## Réponses d’erreur

Les erreurs de validation retournent :

```json
{
  "code": "ValidationError",
  "message": "One or more validation errors occurred.",
  "fieldErrors": { "fieldName": ["Validation error message"] },
  "traceId": "request-trace-id"
}
```

Les exceptions inattendues sont gérées par `GlobalExceptionHandler` et retournées dans le même format.

## Exécution en local

Avec une instance MySQL accessible via la chaîne de connexion de vos user secrets :

```bash
dotnet run --project src/Accounts.DataApi/AccountsAPI
```

Pour lancer le système complet, utilisez Docker Compose depuis la racine du dépôt. Voir le [README racine](../../README.fr.md).

## Tests

```bash
dotnet test src/Accounts.DataApi/AccountsAPI.Tests
```

Les tests unitaires couvrent les services applicatifs et la validation de la clé API. Les tests d’intégration exécutent le repository sur une vraie base MySQL.

## Dépannage

**`PROCEDURE AccountsDb.GetAccounts does not exist`** : la base n’a pas été initialisée à partir du dump. Recréez le volume depuis la racine du dépôt avec `docker compose down -v`, puis `docker compose up --build`.

**401 Unauthorized** : vérifiez que l’en-tête `x-api-key` correspond à la valeur configurée dans `AccountsApiKey`.

**Le health check échoue** : vérifiez l’hôte de la chaîne de connexion (`db` dans Docker, `localhost` en local), les identifiants, et que l’initialisation de MySQL est terminée.