# Accounts Orchestration API

Le service orienté application du [système de gestion des comptes](../../README.fr.md). Il se situe entre le frontend et l’[Accounts Data API](../Accounts.DataApi/README.fr.md) et n’accède pas directement à la base de données.

```text
Frontend  →  Orchestration API  →  Accounts Data API  →  MySQL
```

## Responsabilités

- Exposer les endpoints liés aux comptes au frontend
- Authentifier les requêtes entrantes à l’aide d’une clé API
- Appeler l’Accounts Data API via un `HttpClient` typé, en transmettant sa propre clé API
- Retourner les données des comptes au frontend
- Retourner les erreurs de validation dans un format cohérent
- Gérer les exceptions inattendues via un gestionnaire d’exceptions global
- Fournir un endpoint de health check

## Structure du projet

```text
src/Accounts.OrchestrationApi/
├── Domain/           Modèles métier des comptes et règles métier
├── Application/      Orchestration des cas d’usage et services applicatifs
├── Infrastructure/   Client HTTP vers la Data API, validation de la clé API, configuration
└── Presentation/     Contrôleurs, authentification, gestion des erreurs, modèles de requête/réponse
```

Les requêtes suivent le flux Contrôleur → `IAccountsService` → `IExternalAccountsClient` → Accounts Data API. Le client typé rend la couche applicative indépendante des détails de l’intégration HTTP :

```csharp
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>();
```

## Technologies

ASP.NET Core Web API (.NET 10), Clean Architecture, Domain-Driven Design, `HttpClient` typé, authentification par clé API, OpenAPI/Swagger, health checks.

## Authentification

Tous les contrôleurs requièrent la politique `RequireApiKey` :

```csharp
app.MapControllers().RequireAuthorization("RequireApiKey");
```

Les requêtes entrantes doivent transmettre la clé dans l’en-tête `x-api-key`. Une clé absente ou invalide est rejetée avant d’atteindre le contrôleur. `GET /health` n’est pas authentifié.

## Configuration

| Clé | Rôle | Secret |
|---|---|---|
| `AccountsApplicationApiKey` | Clé attendue de la part du frontend | Oui |
| `AccountsApiKey` | Clé transmise à l’Accounts Data API | Oui |
| `ExternalDbApi:BaseUrl` | URL de base de l’Accounts Data API, sous forme d’URI absolue | Non |

Le démarrage échoue si l’une de ces valeurs est absente ou si l’URL de base n’est pas une URI absolue valide.

**En développement local**, les clés sont des user secrets .NET et l’URL de base est définie dans `appsettings.Development.json` :

```bash
cd src/Accounts.OrchestrationApi
dotnet user-secrets set "AccountsApplicationApiKey" "your-accounts-application-api-key"
dotnet user-secrets set "AccountsApiKey" "your-accounts-api-key"
```

```json
{
  "ExternalDbApi": {
    "BaseUrl": "http://localhost:5253"
  }
}
```

**Dans Docker**, les clés sont des secrets Docker montés dans `/run/secrets`, et l’URL de base est définie dans `docker-compose.yaml` à l’aide du nom de service Compose :

```yaml
ExternalDbApi__BaseUrl: "http://data-api:8080"
```

Le double underscore correspond à la clé imbriquée `ExternalDbApi:BaseUrl`.

## Endpoints

| Méthode | Chemin | Auth | Description |
|---|---|---|---|
| GET | `/health` | Aucune | État de santé du service |
| GET | `/api/accounts` | Clé API | Retourne tous les comptes depuis la Data API |

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

Avec l’Accounts Data API accessible à l’URL de base configurée :

```bash
dotnet run --project src/Accounts.OrchestrationApi
```

Pour lancer le système complet, utilisez Docker Compose depuis la racine du dépôt. Voir le [README racine](../../README.fr.md).

## Dépannage

**Le démarrage échoue** : vérifiez que les trois valeurs de configuration sont définies et que l’URL de base est une URI absolue, par exemple `http://data-api:8080`.

**401 Unauthorized** : vérifiez que l’en-tête `x-api-key` correspond à `AccountsApplicationApiKey`.

**Les comptes ne peuvent pas être récupérés** : vérifiez que la Data API est démarrée, que `ExternalDbApi:BaseUrl` pointe vers elle et que `AccountsApiKey` correspond à la clé attendue par la Data API. Une erreur 500 provient généralement de la Data API : consultez ses logs avec `docker compose logs data-api`.

**Swagger n’est pas disponible** : il ne s’active qu’avec `ASPNETCORE_ENVIRONMENT=Development`.