
# Accounts System

Ce projet exécute un système de comptes multi-conteneurs avec Docker Compose. Le système est un prototype simple.

Le système contient :

- `portfoliofe` — application frontend
- `accountsapplicationapi` — API applicative
- `accountsapi` — API comptes/base de données
- `accountsdb` — base de données MySQL 8.0

## Prérequis

Avant d’exécuter le projet, installez :

- Docker
- Docker Compose
- Git

Vous aurez également besoin de secrets d’application et de variables d’environnement valides.

## Cloner le repository

```bash
git clone <your-repository-url>
cd <your-repository-folder>
````

## Variables d’environnement

Ce projet nécessite un fichier `.env` dans le même répertoire que `docker-compose.yaml`.

Créez un fichier `.env` en copiant le fichier d’exemple :

```bash
cp .env.example .env
```

Modifiez ensuite `.env` et remplacez les valeurs de remplacement.

Exemple de fichier `.env` :

```env
AUTH0_DOMAIN=your-auth0-domain.example.com
MYSQL_PASSWORD=change-me
MYSQL_ROOT_PASSWORD=change-me-root
```

### Variables d’environnement requises

| Variable              | Description                                    |
| --------------------- | ---------------------------------------------- |
| `AUTH0_DOMAIN`        | Votre domaine Auth0                            |
| `MYSQL_PASSWORD`      | Mot de passe de l’utilisateur applicatif MySQL |
| `MYSQL_ROOT_PASSWORD` | Mot de passe root du conteneur MySQL           |

Ne commitez pas votre vrai fichier `.env` dans Git.

## Secrets

Ce projet utilise les secrets Docker Compose. Les fichiers de secrets requis doivent exister localement dans un dossier `secrets`.

Créez le dossier `secrets` s’il n’existe pas déjà :

```bash
mkdir secrets
```

Créez ensuite les fichiers suivants :

```text
secrets/AccountsApiKey.txt
secrets/AccountsApplicationApiKey.txt
secrets/Auth0ClientId.txt
secrets/Auth0ClientSecret.txt
```

Chaque fichier doit contenir uniquement la valeur du secret.

Exemple :

```text
my-secret-value-here
```

N’entourez pas la valeur de guillemets.

### Fichiers de secrets requis

| File                                    | Description                                                  |
| --------------------------------------- | ------------------------------------------------------------ |
| `secrets/AccountsApiKey.txt`            | Clé API utilisée par `accountsapi`                           |
| `secrets/AccountsApplicationApiKey.txt` | Clé API utilisée par `accountsapplicationapi` et le frontend |
| `secrets/Auth0ClientId.txt`             | ID client Auth0                                              |
| `secrets/Auth0ClientSecret.txt`         | Secret client Auth0                                          |

Ne commitez pas de vrais fichiers de secrets dans Git.

## Fichiers de secrets d’exemple

Le repository inclut des fichiers d’exemple :

```text
secrets.example/AccountsApiKey.txt
secrets.example/AccountsApplicationApiKey.txt
secrets.example/Auth0ClientId.txt
secrets.example/Auth0ClientSecret.txt
```

Copiez-les vers les noms de fichiers requis :

```bash
cp secrets.example/AccountsApiKey.txt secrets/AccountsApiKey.txt
cp secrets.example/AccountsApplicationApiKey.txt secrets/AccountsApplicationApiKey.txt
cp secrets.example/Auth0ClientId.txt secrets/Auth0ClientId.txt
cp secrets.example/Auth0ClientSecret.txt secrets/Auth0ClientSecret.txt
```

Remplacez ensuite les valeurs fictives par de vraies valeurs.

## Initialisation de la base de données

Le conteneur MySQL utilise ce fichier SQL dump au premier démarrage :

```text
./mySQLDb/accountsdb_dump_v1.sql
```

Ce fichier est monté dans le conteneur MySQL à cet emplacement :

```text
/docker-entrypoint-initdb.d/accountsdb_dump_v1.sql
```

MySQL exécute les scripts d’initialisation uniquement lorsque le volume de base de données est créé pour la première fois.

Si le volume de base de données existe déjà, le SQL dump ne sera pas réimporté automatiquement.

Pour réinitialiser la base de données et réexécuter le script d’initialisation SQL :

```bash
docker compose down -v
docker compose up -d
```

Attention : `docker compose down -v` supprime le volume MySQL et toutes les données de la base de données.

## Exécuter l’application

Après avoir créé le fichier `.env` et les fichiers de secrets requis, exécutez :

```bash
docker compose up -d
```

Cela démarre tous les services en mode détaché.

Pour voir les logs :

```bash
docker compose logs -f
```

Pour voir les logs d’un service spécifique :

```bash
docker compose logs -f accountsapi
docker compose logs -f accountsapplicationapi
docker compose logs -f portfoliofe
docker compose logs -f accountsdb
```

## Accéder à l’application

Le frontend est exposé sur le port `1000`.

Ouvrez :

```text
http://localhost:1000
```

Les services internes communiquent avec les noms de services Docker Compose :

```text
portfoliofe -> accountsapplicationapi
accountsapplicationapi -> accountsapi
accountsapi -> accountsdb
```

Les URLs internes sont :

```text
http://accountsapplicationapi:8080
http://accountsapi:8080
accountsdb:3306
```

## Arrêter l’application

Pour arrêter les conteneurs :

```bash
docker compose down
```

Pour arrêter les conteneurs et supprimer le volume de base de données :

```bash
docker compose down -v
```

Utilisez `-v` avec précaution, car cela supprime les données de la base de données.

## Télécharger les images Docker publiques

Cette configuration Compose de production utilise des images Docker publiques.

Pour télécharger les dernières images :

```bash
docker compose pull
```

Démarrez ensuite ou recréez les conteneurs :

```bash
docker compose up -d
```

Si les conteneurs sont déjà en cours d’exécution et que vous voulez les recréer avec les images téléchargées :

```bash
docker compose up -d --force-recreate
```

## Health checks

Le service MySQL inclut un healthcheck. Le service `accountsapi` attend que MySQL devienne healthy avant de démarrer.

```yaml
depends_on:
  accountsdb:
    condition: service_healthy
```

## Erreurs courantes

### La variable d’environnement n’est pas définie

Exemple :

```text
The "AUTH0_DOMAIN" variable is not set. Defaulting to a blank string.
The "MYSQL_PASSWORD" variable is not set. Defaulting to a blank string.
MYSQL_ROOT_PASSWORD is required
```

Correction : créez un fichier `.env` dans le même répertoire que `docker-compose.yml` et fournissez les valeurs :

```env
AUTH0_DOMAIN=your-auth0-domain.example.com
MYSQL_PASSWORD=change-me
MYSQL_ROOT_PASSWORD=change-me-root
```

### Fichier de secret introuvable

Exemple :

```text
secrets/AccountsApiKey.txt not found
```

Correction : créez le fichier manquant dans le répertoire `secrets`.

```bash
mkdir secrets
touch secrets/AccountsApiKey.txt
```

Ajoutez ensuite la vraie valeur du secret dans le fichier.

### Les données MySQL ne se réinitialisent pas

Si les changements du SQL dump n’apparaissent pas, le volume MySQL existe probablement déjà.

Réinitialisez le volume de base de données :

```bash
docker compose down -v
docker compose up -d
```

Avertissement : cela supprime les données existantes de la base de données.

## `.gitignore` suggéré

Utilisez un `.gitignore` comme celui-ci pour éviter de commiter de vrais secrets :

```gitignore
.env

secrets/AccountsApiKey.txt
secrets/AccountsApplicationApiKey.txt
secrets/Auth0ClientId.txt
secrets/Auth0ClientSecret.txt

mysql-data/
```

## Avertissement

Ce projet est un prototype simple créé à des fins de démonstration uniquement. Il est fourni « tel quel », sans garantie d’aucune sorte.

L’auteur n’est pas responsable des problèmes pouvant résulter de l’utilisation, de la modification, du déploiement ou de la distribution de ce projet, y compris les pertes de données, les problèmes de sécurité ou les interruptions de service.

Ce projet n’est pas destiné à être utilisé tel quel dans un environnement de production. Avant tout déploiement public ou commercial, vérifiez la configuration de sécurité, la gestion des secrets, la configuration de la base de données, le flux d’authentification, la gestion des erreurs, les logs et les paramètres d’infrastructure.