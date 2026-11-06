# Accounts Data API

Le service orienté base de données du [système de gestion des comptes](../../README.fr.md). Il sert gRPC à l’Orchestration API et c’est le seul composant qui se connecte à PostgreSQL.

```text
Orchestration API  →  gRPC sur TLS + JWT  →  Accounts Data API  →  PostgreSQL 18
```

## Responsabilités

- Servir les contrats gRPC `accounts.v1.AccountsService` et `statements.v1.StatementsService`
- Authentifier les appelants à l’aide de jetons d’accès JWT émis par Auth0 et exiger le scope `read:accounts`
- Exécuter des requêtes en lecture seule sur PostgreSQL avec le rôle `accounts_app`
- Traduire les résultats attendus en codes de statut gRPC à un seul endroit
- Fournir des health checks de liveness et de readiness

## Structure du projet

```text
src/Accounts.DataApi/
├── AccountsAPI/
│   ├── Domain/           Modèles métier des comptes et règles métier
│   ├── Application/      Interfaces et services applicatifs
│   ├── Infrastructure/   Repositories PostgreSQL et configuration
│   └── Presentation/     Implémentations des services gRPC et traduction des erreurs
└── AccountsAPI.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

Les requêtes suivent le flux service gRPC → service applicatif → repository → PostgreSQL. Les services gRPC ne contiennent aucune logique d’accès aux données, et les repositories ignorent tout de gRPC.

## Technologies

ASP.NET Core gRPC (.NET 10, `Grpc.AspNetCore`), Protocol Buffers, Clean Architecture, Domain-Driven Design, PostgreSQL 18 avec Npgsql, authentification JWT bearer (Auth0), TLS, health checks ASP.NET Core, logs JSON structurés, xUnit v3, Testcontainers, NSubstitute, AwesomeAssertions.

## Services gRPC

| Service | RPC | Type |
|---|---|---|
| `accounts.v1.AccountsService` | `GetAccount` | Unaire |
| `accounts.v1.AccountsService` | `GetAccounts` | Unaire |
| `statements.v1.StatementsService` | `GetTransactions` | Streaming serveur |

Les contrats se trouvent dans [`protos/`](../../protos), à la racine du dépôt.

`GetTransactions` envoie un message par transaction, par ordre chronologique, en lisant les lignes depuis PostgreSQL et en les écrivant dans la réponse une par une. Si le client se déconnecte ou si sa deadline expire, l’annulation remonte jusqu’à la requête en base. Le service vérifie que le compte existe avant de lancer le streaming : un compte inexistant renvoie donc `NotFound` plutôt qu’un flux vide.

## Endpoints et ports

| Port | Protocole | Sert |
|---|---|---|
| 8081 | HTTPS, HTTP/2 | gRPC |
| 8080 | HTTP/1.1 | Health checks |

| Chemin | Auth | Vérifie |
|---|---|---|
| `/health/live` | Aucune | Le processus tourne ; aucune vérification des dépendances |
| `/health/ready` | Aucune | La base de données est joignable |

## Authentification et autorisation

L’API est sécurisée par défaut : une politique d’autorisation par défaut (fallback policy) exige un appelant authentifié sur chaque endpoint. Chaque service gRPC exige en plus le scope `read:accounts`, afin qu’un futur service d’écriture puisse exiger son propre scope. Les health checks en sont explicitement exemptés avec `AllowAnonymous()`.

Les appelants envoient un jeton d’accès Auth0 dans l’en-tête `Authorization: Bearer`. L’Orchestration API l’obtient via le flux client credentials.

## TLS

Dans Docker, Kestrel sert gRPC sur le port 8081 avec un certificat émis par l’autorité de certification privée du système. Les chemins du certificat et de la clé sont définis par des variables d’environnement Compose et pointent vers des secrets Docker sous `/run/secrets`. Lorsque l’API s’exécute en dehors de Docker, elle utilise à la place le certificat de développement .NET. Voir [TLS entre les services](../../README.fr.md#tls-entre-les-services) pour créer le certificat.

## Configuration

Les paramètres obligatoires sont liés à des classes d’options et validés au démarrage : l’API échoue immédiatement si l’un d’eux est absent ou invalide. Les valeurs identiques partout se trouvent dans `appsettings.json`, celles qui varient selon le déploiement viennent de variables d’environnement, et les secrets viennent de fichiers secrets.

Le mot de passe de la base est la seule partie secrète de la connexion : l’hôte, le nom de la base et le nom d’utilisateur sont de la configuration ordinaire.

**En développement local**, les valeurs secrètes sont gérées avec les user secrets .NET :

```bash
cd src/Accounts.DataApi/AccountsAPI
dotnet user-secrets list
```

**Dans Docker**, les secrets sont montés dans `/run/secrets` et lus par le provider de configuration key-per-file. Voir le [README racine](../../README.fr.md) pour la mise en place du dossier `secrets/`.

## Base de données

L’API se connecte en tant que `accounts_app`, un rôle en lecture seule créé par les scripts d’initialisation de `DbFiles/init/`. Elle utilise du SQL inline dans ses repositories plutôt que des procédures stockées ou des fonctions, si bien que chaque requête se trouve à côté du code qui l’utilise. Voir [Base de données](../../README.fr.md#base-de-données) dans le README racine pour le schéma et les rôles.

## Erreurs

Les résultats attendus, comme un compte introuvable ou un identifiant invalide, sont renvoyés comme résultats et traduits en codes de statut gRPC (`NotFound`, `InvalidArgument`) à un seul endroit. Les erreurs inattendues sont journalisées une seule fois côté serveur avec l’identifiant de trace W3C, et l’appelant reçoit un message d’erreur générique.

## Logs

En dehors de l’environnement Development, l’API écrit des logs JSON sur la console, avec les identifiants de trace et de span. En Development, elle utilise le format console simple.

## Exécution en local

Avec une instance PostgreSQL accessible via vos paramètres de connexion :

```bash
dotnet run --project src/Accounts.DataApi/AccountsAPI
```

Pour lancer le système complet, utilisez Docker Compose depuis la racine du dépôt. Voir le [README racine](../../README.fr.md).

## Tests

```bash
dotnet test src/Accounts.DataApi/AccountsAPI.Tests
```

Docker doit être démarré. Les tests d’intégration utilisent Testcontainers pour lancer un conteneur PostgreSQL 18 jetable, initialisé avec les mêmes scripts de `DbFiles/init/` que Docker Compose. L’API s’exécute ensuite en mémoire, et les tests l’appellent via un vrai client gRPC, y compris le RPC en streaming. Ils vérifient aussi que les health checks sont accessibles sans authentification.

## Dépannage

**`Unauthenticated`** : l’appel ne contient pas de jeton d’accès, ou le jeton est invalide ou a une mauvaise audience.

**`PermissionDenied`** : le jeton est valide mais n’a pas le scope `read:accounts`. Accordez-le à l’application machine-to-machine dans Auth0.

**`/health/ready` échoue** : l’API ne parvient pas à joindre PostgreSQL. Vérifiez l’hôte (`db` dans Docker), le mot de passe d’`accounts_app`, et que l’initialisation de la base est terminée.

**`password authentication failed for user "accounts_app"`** : la base a été initialisée avec un autre mot de passe. Réinitialisez le volume depuis la racine du dépôt avec `docker compose down -v`.