# Accounts Orchestration API

Le service orienté application du [système de gestion des comptes](../../README.fr.md). Il expose une API REST au frontend et appelle l’[Accounts Data API](../Accounts.DataApi/README.fr.md) en gRPC. Il n’accède pas directement à la base de données.

```text
Frontend  →  HTTP + clé API  →  Orchestration API  →  gRPC sur TLS + JWT  →  Accounts Data API  →  PostgreSQL
```

## Responsabilités

- Exposer au frontend les endpoints liés aux comptes, sous forme d’API REST
- Authentifier les requêtes entrantes à l’aide d’une clé API
- Appeler l’Accounts Data API via un client gRPC, sur TLS, avec un jeton d’accès (access token) Auth0
- Faire la traduction entre les modèles REST utilisés par le frontend et le contrat gRPC de la Data API
- Gérer les exceptions inattendues via un gestionnaire d’exceptions global
- Fournir un health check de liveness

## Structure du projet

```text
src/Accounts.OrchestrationApi/
├── OrchestrationApi/
│   ├── Domain/           Modèles métier des comptes et règles métier
│   ├── Application/      Orchestration des cas d’usage et services applicatifs
│   ├── Infrastructure/   Client gRPC vers la Data API, fournisseur de jetons Auth0, validation de la clé API, configuration
│   └── Presentation/     Contrôleurs, authentification, gestion des erreurs, modèles de requête/réponse
└── OrchestrationApi.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

Les requêtes suivent le flux contrôleur → service applicatif → client gRPC → Accounts Data API. La couche applicative dépend d’une interface : elle ignore tout de gRPC, de TLS et des jetons.

## Technologies

ASP.NET Core Web API (.NET 10), Clean Architecture, Domain-Driven Design, client gRPC (`Grpc.Net.ClientFactory`), Protocol Buffers, Auth0 client credentials, TLS avec une autorité de certification privée, authentification par clé API, OpenAPI/Swagger, health checks ASP.NET Core, logs JSON structurés, xUnit v3, AwesomeAssertions.

## Authentification

**Requêtes entrantes.** L’API est sécurisée par défaut : sa politique d’autorisation par défaut (fallback policy) exige le schéma clé API sur chaque endpoint. Les requêtes doivent transmettre la clé dans l’en-tête `x-api-key`, et une clé absente ou invalide est rejetée avant d’atteindre le contrôleur. Le health check en est explicitement exempté avec `AllowAnonymous()`.

**Appels à la Data API.** Le client gRPC joint un jeton d’accès Auth0 à chaque appel, sous forme de call credentials. Le jeton est obtenu via le flux client credentials, avec l’application machine-to-machine de l’Orchestration API dans Auth0. La Data API exige le scope `read:accounts`.

## TLS vers la Data API

Le client gRPC se connecte à la Data API en HTTPS. Au lieu du magasin de certificats du système d’exploitation, il ne fait confiance qu’à l’autorité privée qui a émis le certificat de la Data API, chargée depuis le fichier de certificat d’autorité configuré. La validation des certificats n’est jamais désactivée. Si aucun chemin de certificat d’autorité n’est configuré, le client utilise le magasin de certificats du système d’exploitation.

Voir [TLS entre les services](../../README.fr.md#tls-entre-les-services) pour créer les certificats.

## Configuration

Les paramètres obligatoires sont liés à des classes d’options et validés au démarrage. L’API échoue immédiatement si une valeur obligatoire est absente, si l’URL de base de la Data API n’est pas une URL absolue valide, ou si un chemin de certificat d’autorité est défini mais que le fichier n’existe pas.

| Paramètre | Rôle | Secret |
|---|---|---|
| `AccountsApplicationApiKey` | Clé attendue de la part du frontend | Oui |
| Client ID et client secret Auth0 | Identifiants de l’application machine-to-machine utilisée pour obtenir les jetons d’accès | Oui |
| URL de base de la Data API | Adresse de la Data API, par exemple `https://data-api:8081` dans Docker | Non |
| Chemin du certificat d’autorité | Certificat racine utilisé pour faire confiance à la Data API | Non |

**En développement local**, les secrets sont des user secrets .NET, et les valeurs locales non secrètes se trouvent dans `appsettings.Development.json`.

**Dans Docker**, les secrets sont montés dans `/run/secrets`, et les valeurs propres à chaque déploiement, comme l’URL de base et le chemin du certificat d’autorité, sont définies comme variables d’environnement dans `docker-compose.yaml`. Un double underscore dans le nom d’une variable d’environnement correspond à une clé de configuration imbriquée (`Section__Key` → `Section:Key`).

## Endpoints

| Méthode | Chemin | Auth | Description |
|---|---|---|---|
| GET | `/health/live` | Aucune | Le processus tourne ; aucune vérification des dépendances |
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

Les exceptions inattendues, y compris les appels échoués à la Data API, sont gérées par `GlobalExceptionHandler`, journalisées une seule fois avec l’identifiant de trace W3C, et retournées dans le même format.

## Logs

En dehors de l’environnement Development, l’API écrit des logs JSON sur la console, avec les identifiants de trace et de span. En Development, elle utilise le format console simple.

## Exécution en local

Avec l’Accounts Data API accessible à l’URL de base configurée :

```bash
dotnet run --project src/Accounts.OrchestrationApi/OrchestrationApi
```

Pour lancer le système complet, utilisez Docker Compose depuis la racine du dépôt. Voir le [README racine](../../README.fr.md).

## Tests

```bash
dotnet test src/Accounts.OrchestrationApi/OrchestrationApi.Tests
```

Les tests unitaires couvrent la validation de la clé API. Les tests d’intégration exécutent l’API en mémoire avec une configuration de test et vérifient, par exemple, que le health check est accessible sans authentification.

## Dépannage

**Le démarrage échoue** : vérifiez que chaque paramètre obligatoire est défini, que l’URL de base de la Data API est une URL absolue comme `https://data-api:8081`, et que le fichier de certificat d’autorité existe au chemin configuré.

**401 Unauthorized** : vérifiez que l’en-tête `x-api-key` correspond à `AccountsApplicationApiKey`.

**Erreurs de certificat ou SSL lors des appels à la Data API** : le certificat d’autorité est manquant, ou provient d’une autre autorité que celle qui a émis le certificat de la Data API.

**`Unauthenticated` ou `PermissionDenied` renvoyé par la Data API** : les identifiants Auth0 sont incorrects, ou l’application machine-to-machine n’a pas reçu le scope `read:accounts`.

**Les comptes ne peuvent pas être récupérés** : vérifiez que la Data API est démarrée et que l’URL de base pointe vers elle. Une erreur 500 provient généralement de la Data API : consultez ses logs avec `docker compose logs data-api`.

**Swagger n’est pas disponible** : il ne s’active qu’avec `ASPNETCORE_ENVIRONMENT=Development`.