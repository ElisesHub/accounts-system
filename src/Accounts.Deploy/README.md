 
# Accounts System

This project runs a multi-container accounts system using Docker Compose. The system is an extremely simple prototype.

The system contains:

- `portfoliofe` — frontend application
- `accountsapplicationapi` — application API
- `accountsapi` — accounts/database API
- `accountsdb` — MySQL 8.0 database

## Prerequisites

Before running the project, install:

- Docker
- Docker Compose
- Git

You will also need valid application secrets and environment variables.

## Clone the repository

```bash
git clone <your-repository-url>
cd <your-repository-folder>
````

## Environment variables

This project requires a `.env` file in the same directory as `docker-compose.yaml`.

Create a `.env` file by copying the example file:

```bash
cp .env.example .env
```

Then edit `.env` and replace the placeholder values.

Example `.env` file:

```env
AUTH0_DOMAIN=your-auth0-domain.example.com
MYSQL_PASSWORD=change-me
MYSQL_ROOT_PASSWORD=change-me-root
```

### Required environment variables

| Variable              | Description                             |
| --------------------- | --------------------------------------- |
| `AUTH0_DOMAIN`        | Your Auth0 domain                       |
| `MYSQL_PASSWORD`      | Password for the MySQL application user |
| `MYSQL_ROOT_PASSWORD` | Root password for the MySQL container   |

Do not commit your real `.env` file to Git.

## Secrets

This project uses Docker Compose secrets. The required secret files must exist locally inside a `secrets` folder.

Create the `secrets` folder if it does not already exist:

```bash
mkdir secrets
```

Then create the following files:

```text
secrets/AccountsApiKey.txt
secrets/AccountsApplicationApiKey.txt
secrets/Auth0ClientId.txt
secrets/Auth0ClientSecret.txt
```

Each file should contain only the secret value.

Example:

```text
my-secret-value-here
```

Do not wrap the value in quotes.

### Required secret files

| File                                    | Description                                               |
| --------------------------------------- | --------------------------------------------------------- |
| `secrets/AccountsApiKey.txt`            | API key used by `accountsapi`                             |
| `secrets/AccountsApplicationApiKey.txt` | API key used by `accountsapplicationapi` and the frontend |
| `secrets/Auth0ClientId.txt`             | Auth0 client ID                                           |
| `secrets/Auth0ClientSecret.txt`         | Auth0 client secret                                       |

Do not commit real secret files to Git.

## Example secret files

The repository includes example files:

```text
secrets.example/AccountsApiKey.txt
secrets.example/AccountsApplicationApiKey.txt
secrets.example/Auth0ClientId.txt
secrets.example/Auth0ClientSecret.txt
```

Copy them to the required filenames:

```bash
cp secrets.example/AccountsApiKey.txt secrets/AccountsApiKey.txt
cp secrets.example/AccountsApplicationApiKey.txt secrets/AccountsApplicationApiKey.txt
cp secrets.example/Auth0ClientId.txt secrets/Auth0ClientId.txt
cp secrets.example/Auth0ClientSecret.txt secrets/Auth0ClientSecret.txt
```

Then replace the dummy values with real values.

## Database initialization

The MySQL container uses this SQL dump file on the first startup:

```text
./mySQLDb/accountsdb_dump_v1.sql
```

This file is mounted into the MySQL container at:

```text
/docker-entrypoint-initdb.d/accountsdb_dump_v1.sql
```

MySQL only runs initialization scripts when the database volume is first created.

If the database volume already exists, the SQL dump will not be re-imported automatically.

To reset the database and re-run the SQL initialization script:

```bash
docker compose down -v
docker compose up -d
```

Warning: `docker compose down -v` deletes the MySQL volume and all database data.

## Running the application

After creating the `.env` file and required secret files, run:

```bash
docker compose up -d
```

This will start all services in detached mode.

To view logs:

```bash
docker compose logs -f
```

To view logs for a specific service:

```bash
docker compose logs -f accountsapi
docker compose logs -f accountsapplicationapi
docker compose logs -f portfoliofe
docker compose logs -f accountsdb
```

## Accessing the application

The frontend is exposed on port `1000`.

Open:

```text
http://localhost:1000
```

Internal services communicate using Docker Compose service names:

```text
portfoliofe -> accountsapplicationapi
accountsapplicationapi -> accountsapi
accountsapi -> accountsdb
```

The internal URLs are:

```text
http://accountsapplicationapi:8080
http://accountsapi:8080
accountsdb:3306
```

## Stopping the application

To stop the containers:

```bash
docker compose down
```

To stop the containers and remove the database volume:

```bash
docker compose down -v
```

Use `-v` carefully because it deletes database data.

## Pulling public Docker images

This production Compose setup uses public Docker images.

To pull the latest images:

```bash
docker compose pull
```

Then start or recreate the containers:

```bash
docker compose up -d
```

If containers are already running and you want to recreate them with the pulled images:

```bash
docker compose up -d --force-recreate
```

 

## Health checks

The MySQL service includes a healthcheck. The `accountsapi` service waits for MySQL to become healthy before starting.

```yaml
depends_on:
  accountsdb:
    condition: service_healthy
```

 
## Common errors

### Environment variable is not set

Example:

```text
The "AUTH0_DOMAIN" variable is not set. Defaulting to a blank string.
The "MYSQL_PASSWORD" variable is not set. Defaulting to a blank string.
MYSQL_ROOT_PASSWORD is required
```

Fix: create a `.env` file in the same directory as `docker-compose.yml` and provide values:

```env
AUTH0_DOMAIN=your-auth0-domain.example.com
MYSQL_PASSWORD=change-me
MYSQL_ROOT_PASSWORD=change-me-root
```

### Secret file not found

Example:

```text
secrets/AccountsApiKey.txt not found
```

Fix: create the missing file inside the `secrets` directory.

```bash
mkdir secrets
touch secrets/AccountsApiKey.txt
```

Then add the real secret value to the file.

### MySQL data does not reset

If changes to the SQL dump are not appearing, the MySQL volume probably already exists.

Reset the database volume:

```bash
docker compose down -v
docker compose up -d
```

Warning: this deletes existing database data.

## Suggested `.gitignore`

Use a `.gitignore` like this to avoid committing real secrets:

```gitignore
.env

secrets/AccountsApiKey.txt
secrets/AccountsApplicationApiKey.txt
secrets/Auth0ClientId.txt
secrets/Auth0ClientSecret.txt

mysql-data/
```

## Disclaimer

This project is a simple prototype created for demonstration purposes only. It is provided “as is”, without any warranties or guarantees.

The author is not responsible for any issues that may result from using, modifying, deploying, or distributing this project, including data loss, security issues, or service interruptions.

This project is not intended to be used as-is in a production environment. Before any public or commercial deployment, review the security configuration, secret management, database setup, authentication flow, error handling, logging, and infrastructure settings.