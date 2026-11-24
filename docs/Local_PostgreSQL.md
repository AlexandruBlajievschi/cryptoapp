# Local PostgreSQL

This setup is the database-only portion of CG-015. It runs PostgreSQL locally;
the .NET API is not connected yet. EF Core, application tables, seed data and
migrations will be added in a separate task. The product backlog is unchanged.

## Requirements

Install Docker Desktop with Docker Compose and start its Linux container engine.
Run all commands below from the repository root, `D:\CryptoApp`.

```powershell
docker compose version
docker info
```

The service uses the official `postgres:18-bookworm` image. The major version is
pinned to 18; pulling this tag can receive minor/security updates. PostgreSQL 18
stores data under `/var/lib/postgresql/18/docker`, so the named volume is mounted
at `/var/lib/postgresql`, following the [official image instructions](https://hub.docker.com/_/postgres).
Do not change the major version on an existing volume without a database upgrade.

## Local configuration

If `.env` does not already exist, copy the template:

```powershell
Copy-Item .env.example .env
```

Edit `.env` and replace `POSTGRES_PASSWORD` with a local password. Do not overwrite
an existing `.env` when updating the project. This workspace may already have an
ignored `.env` with a generated password from initial setup; keep that file.

| Variable | Purpose | Example |
|---|---|---|
| `POSTGRES_DB` | Initial database | `cryptoguide` |
| `POSTGRES_USER` | Local database administrator | `cryptoguide` |
| `POSTGRES_PASSWORD` | Local administrator password | Choose locally |
| `POSTGRES_PORT` | Host port | `5432` |

`.env` is ignored by the root `.gitignore`; only `.env.example` is committed.
Compose rejects missing or empty database/user/password values. The database
port binds to `127.0.0.1` for local access. This local administrator account is
created by the official image and is not a future production application role.

Compose reads `.env`; the .NET application does not automatically read this file.
Later we will configure its connection string with ASP.NET Core user-secrets.

## Start and check

Validate the configuration without printing resolved credentials:

```powershell
docker compose config --quiet
docker compose up -d postgres
docker compose ps
docker compose logs --tail=30 postgres
```

The first start downloads the image and initializes the database. Wait until
`docker compose ps` reports `healthy`. The health check runs `pg_isready` every
five seconds. `up` alone does not prove initialization or authentication succeeded.

If the host port is occupied, choose another `POSTGRES_PORT` (for example `5433`)
in `.env`, then run `docker compose up -d postgres` again. If Docker reports a
daemon/pipe connection error, start Docker Desktop's Linux engine and retry.

## Connect

The image includes `psql`, so a host installation is optional. With the default
database and user from the template:

```powershell
docker compose exec postgres psql -U cryptoguide -d cryptoguide
```

If you changed `POSTGRES_USER` or `POSTGRES_DB`, substitute those values.
Inside `psql`, run:

```sql
SELECT current_database(), current_user, version();
```

Exit with `\q`. The image permits local Unix-socket connections inside the
container without a password; that does not test password access from the host.

For a database GUI, use host `127.0.0.1`, the configured host port, and the
database/user/password from your own `.env`. With host `psql` installed, use:

```powershell
psql -h 127.0.0.1 -p 5432 -U cryptoguide -d cryptoguide -W
```

`-W` prompts for the password. Adjust port, user and database if customized.

## Verify persistence

Using the container's `psql` connection above, create a small verification row:

```sql
CREATE TABLE setup_persistence_check (id integer PRIMARY KEY, note text NOT NULL);
INSERT INTO setup_persistence_check VALUES (1, 'survives container recreation');
```

Exit `psql`, recreate the container, and wait for it to become healthy:

```powershell
docker compose up -d --force-recreate postgres
docker compose ps
```

Reconnect and run:

```sql
SELECT * FROM setup_persistence_check;
DROP TABLE setup_persistence_check;
```

The row should still exist before cleanup. This is a manual infrastructure
check, not an application migration or entity. Only create this table for the
check and remove it afterward.

## Stop and retained data

```powershell
docker compose stop postgres
docker compose start postgres
```

`docker compose down` removes the project's containers and network while keeping
the named `cryptoguide_postgres_data` volume. A later `up` reuses the saved data.
Avoid the `--volumes`/`-v` option on `down` unless you intend to erase the local
database. A named volume preserves container data; it is not a backup.

The image's database/user/password variables initialize an empty volume only.
Changing `.env` does not change the password or rename the database in an already
initialized volume. Use PostgreSQL commands for those changes.

## Verification performed

Verified in this workspace on 24 November 2026:

- `docker compose config --quiet` accepted the configuration.
- The container reached `healthy` and accepted a database connection.
- A verification row survived `up -d --force-recreate postgres`.
- A password-authenticated TCP connection read that row after recreation.
- The verification table was removed; no application tables were created.
- `.env` is ignored by Git and its generated password was not printed.

The container is healthy on `127.0.0.1:5432`, matching the committed template.
The Windows service `postgresql-x64-17` previously occupied that port and was
stopped to free it for Docker. Its startup configuration was not changed; if
it starts again after a reboot, stop it before starting this container or choose
another host port in `.env`.
