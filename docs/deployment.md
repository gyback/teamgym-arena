# Deployment

This is the authoritative list of the settings and runtime behaviour that an operator relies on to run TeamGym
Arena. It implements the public deployment contract in
[ADR-0009](adr/0009-cloud-hosting-and-infrastructure-as-code.md#7-public-deployment-contract). Every pull
request that adds or changes a setting updates this file.

> **Status: pre-alpha.** Only the settings that the code reads today are listed. The rest of the contract
> (container images, forwarded headers, managed identity database authentication, identity, Data Protection and
> e-mail) is added as it is implemented, starting with [#37](https://github.com/gyback/teamgym-arena/issues/37).

## Configuration

Configuration comes only from environment variables, using ASP.NET Core's `__` separator for nested keys.

| Setting | Required | Default | Description |
| --- | --- | --- | --- |
| `ConnectionStrings__teamgym` | Yes | Not set | The [Npgsql connection string](https://www.npgsql.org/doc/connection-string-parameters.html) of the PostgreSQL 17 database that every module shares, for example `Host=db;Database=teamgym_arena;Username=teamgym;Password=…`. The API and the migration runner both read it. Treat the value as a secret when it contains a password. |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` | The hosting environment. Use `Production` for every deployed instance. `Development` also maps the OpenAPI document at `/openapi/v1.json`. |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | No | Not set | The OTLP endpoint for logs, traces and metrics, for example the Aspire dashboard, Grafana, Jaeger or SigNoz. When it is not set, no telemetry is exported. |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | No | `grpc` | The OTLP transport: `grpc` or `http/protobuf`. |
| `OTEL_EXPORTER_OTLP_HEADERS` | No | Not set | Extra headers for the OTLP exporter, such as an API key for a hosted backend, as comma-separated `key=value` pairs. Treat the value as a secret. |
| `OTEL_SERVICE_NAME` | No | The assembly name | The service name attached to all telemetry. |
| `OTEL_RESOURCE_ATTRIBUTES` | No | Not set | Extra resource attributes, for example `deployment.environment=production`. |

## Database migrations

The API never changes the database schema
([ADR-0007](adr/0007-primary-data-store-and-data-access.md#4-schema-migration-lifecycle)). Run the migration runner
(`src/MigrationService`) to completion before you start a new version of the API. It reads the same
`ConnectionStrings__teamgym` setting, applies every module's pending migrations in its own PostgreSQL schema
(`users`, `organisations`, `competitions`, `applications` and `scheduling`), and exits with code `0` on success
or a non-zero code on failure. If it fails, keep the previous API version running. The migrations follow
expand-and-contract, so the previous version keeps working against the migrated schema. The migration runner
needs a database role that may create schemas and tables. The API needs only `USAGE` on the five module schemas
and `SELECT`, `INSERT`, `UPDATE` and `DELETE` on their tables. Grant those with `ALTER DEFAULT PRIVILEGES` for the
migration runner's role as well, so that tables that later migrations create are covered.

## Health endpoints

Both endpoints are mapped in every environment, allow anonymous access and return only `Healthy` or
`Unhealthy` as plain text, with status `200` or `503`.

| Endpoint | Purpose | Checks |
| --- | --- | --- |
| `GET /alive` | Liveness: the process is running. | Checks tagged `live`. |
| `GET /health` | Readiness: the instance can serve traffic. | Every registered check, including a connection to the database for each module, which counts as unhealthy after 5 seconds. |
