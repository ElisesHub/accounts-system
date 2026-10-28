#!/bin/bash
# 00_create_app_role.sh: runs first (alphabetical), as POSTGRES_USER (superuser)
set -euo pipefail

APP_PASSWORD="$(cat /run/secrets/DbAppPassword)"

psql -v ON_ERROR_STOP=1 \
     --username "$POSTGRES_USER" \
     --dbname "$POSTGRES_DB" \
     -v app_password="$APP_PASSWORD" \
     -v db_name="$POSTGRES_DB" <<-'EOSQL'
    CREATE ROLE accounts_app LOGIN PASSWORD :'app_password';

    GRANT CONNECT ON DATABASE :"db_name" TO accounts_app;
    GRANT USAGE ON SCHEMA public TO accounts_app;

    -- Applies to objects created later by the current role (the superuser)
    ALTER DEFAULT PRIVILEGES IN SCHEMA public
        GRANT SELECT ON TABLES TO accounts_app;
    ALTER DEFAULT PRIVILEGES IN SCHEMA public
        GRANT EXECUTE ON FUNCTIONS TO accounts_app;
EOSQL