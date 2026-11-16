# Deployment

This is the authoritative list of the settings and runtime behaviour that an operator relies on to run TeamGym
Arena. It implements the public deployment contract in
[ADR-0009](adr/0009-cloud-hosting-and-infrastructure-as-code.md#7-public-deployment-contract). Every pull
request that adds or changes a setting updates this file.

> **Status: pre-alpha.** Only the settings that the code reads today are listed. The rest of the contract
> (container images, forwarded headers, database, identity, Data Protection and e-mail) is added as it is
> implemented, starting with [#37](https://github.com/gyback/teamgym-arena/issues/37).

## Configuration

Configuration comes only from environment variables, using ASP.NET Core's `__` separator for nested keys.

| Setting | Required | Default | Description |
| --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` | The hosting environment. Use `Production` for every deployed instance. `Development` also maps the OpenAPI document at `/openapi/v1.json`. |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | No | Not set | The OTLP endpoint for logs, traces and metrics, for example the Aspire dashboard, Grafana, Jaeger or SigNoz. When it is not set, no telemetry is exported. |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | No | `grpc` | The OTLP transport: `grpc` or `http/protobuf`. |
| `OTEL_EXPORTER_OTLP_HEADERS` | No | Not set | Extra headers for the OTLP exporter, such as an API key for a hosted backend, as comma-separated `key=value` pairs. Treat the value as a secret. |
| `OTEL_SERVICE_NAME` | No | The assembly name | The service name attached to all telemetry. |
| `OTEL_RESOURCE_ATTRIBUTES` | No | Not set | Extra resource attributes, for example `deployment.environment=production`. |

## Health endpoints

Both endpoints are mapped in every environment, allow anonymous access and return only `Healthy` or
`Unhealthy` as plain text, with status `200` or `503`.

| Endpoint | Purpose | Checks |
| --- | --- | --- |
| `GET /alive` | Liveness: the process is running. | Checks tagged `live`. |
| `GET /health` | Readiness: the instance can serve traffic. | Every registered check. |
