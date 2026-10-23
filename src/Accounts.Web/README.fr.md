# Accounts Web

L’application web destinée aux utilisateurs du [système de gestion des comptes](../../README.fr.md). Elle affiche l’interface, gère la connexion via Auth0 et récupère les données des comptes depuis l’[Orchestration API](../Accounts.OrchestrationApi/README.fr.md). Elle n’accède pas directement aux autres services ni à la base de données.

```text
Navigateur  →  Accounts Web  →  Orchestration API  →  Accounts Data API  →  MySQL
```

## Responsabilités

- Afficher l’interface utilisateur avec Razor Pages et des vues MVC
- Authentifier les utilisateurs via Auth0
- Appeler l’Orchestration API via un `HttpClient` typé, en transmettant une clé API
- Afficher les données des comptes
- Afficher des pages d’erreur et de code de statut destinées aux utilisateurs

## Structure du projet

```text
src/Accounts.Web/
├── Domain/           Modèles métier et règles métier
├── Application/      Services applicatifs et interfaces
├── Infrastructure/   Client HTTP vers l’Orchestration API, configuration, intégrations de sécurité
├── Presentation/     Razor Pages, contrôleurs et vues MVC, routage
└── wwwroot/          Ressources statiques
```

Les requêtes suivent le flux Page ou contrôleur → `IAccountsService` → `IExternalAccountsClient` → Orchestration API :

```csharp
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>();
```

## Technologies

ASP.NET Core (.NET 10), Razor Pages, MVC, Clean Architecture, Domain-Driven Design, Auth0, `HttpClient` typé.

## Configuration

| Clé | Rôle | Secret |
|---|---|---|
| `Auth0:Domain` | Domaine du tenant Auth0 | Non |
| `Auth0:ClientId` | Client ID de l’application Auth0 | Oui |
| `Auth0:ClientSecret` | Client secret de l’application Auth0 | Oui |
| `AccountsApplicationApiKey` | Clé transmise à l’Orchestration API | Oui |
| `ExternalAccountsApi:BaseUrl` | URL de base de l’Orchestration API, sous forme d’URI absolue | Non |

Le démarrage échoue si l’une de ces valeurs est absente ou si l’URL de base n’est pas une URI absolue valide.

**En développement local**, les valeurs non sensibles sont définies dans `appsettings.Development.json` :

```json
{
  "ExternalAccountsApi": {
    "BaseUrl": "http://localhost:5182"
  },
  "Auth0": {
    "Domain": "your-tenant.eu.auth0.com"
  }
}
```

Les secrets sont gérés avec les user secrets .NET :

```bash
cd src/Accounts.Web
dotnet user-secrets set "Auth0:ClientId" "your-auth0-client-id"
dotnet user-secrets set "Auth0:ClientSecret" "your-auth0-client-secret"
dotnet user-secrets set "AccountsApplicationApiKey" "your-accounts-application-api-key"
```

**Dans Docker**, le client ID, le client secret et la clé API sont des secrets Docker montés dans `/run/secrets`. Le domaine et l’URL de base sont définis dans `docker-compose.yaml` :

```yaml
Auth0__Domain: your-tenant.eu.auth0.com
ExternalAccountsApi__BaseUrl: "http://orchestration-api:8080"
```

## Configuration d’Auth0

Dans les paramètres de l’application Auth0, déclarez les URL sur lesquelles l’application s’exécute :

| Paramètre | Docker Compose | `dotnet run` |
|---|---|---|
| Allowed Callback URLs | `http://localhost:1000/callback` | `http://localhost:5236/callback` |
| Allowed Logout URLs | `http://localhost:1000` | `http://localhost:5236` |

## Gestion des erreurs

Hors environnement Development, les exceptions non gérées sont affichées sur `/Error`, et les codes de statut tels que 403, 404 et 500 sur `/Error/StatusCode?code={statusCode}`. Le profil de lancement `httpProd` exécute l’application en local en mode Production, pour vérifier ces pages sans Docker.

## Exécution en local

Avec l’Orchestration API accessible sur `http://localhost:5182` :

```bash
dotnet run --project src/Accounts.Web
```

L’application est disponible sur `http://localhost:5236`. Pour lancer le système complet, utilisez Docker Compose depuis la racine du dépôt ; l’application est alors disponible sur `http://localhost:1000`. Voir le [README racine](../../README.fr.md).

## Dépannage

**Le démarrage échoue** : vérifiez que les cinq valeurs de configuration sont définies et que l’URL de base est une URI absolue.

**Les comptes ne peuvent pas être récupérés** : vérifiez que l’Orchestration API est démarrée, que `ExternalAccountsApi:BaseUrl` pointe vers elle et que `AccountsApplicationApiKey` correspond à la clé attendue. Dans Docker, consultez `docker compose logs orchestration-api` et `docker compose logs data-api` pour identifier où la requête a échoué.

**La connexion Auth0 échoue** : vérifiez le domaine, le client ID et le secret, ainsi que la correspondance entre les URL de callback et de logout déclarées dans Auth0 et l’hôte et le port de l’application.