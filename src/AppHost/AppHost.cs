var builder = DistributedApplication.CreateBuilder(args);

// One PostgreSQL database, teamgym_arena, that every module shares, each in its own schema (ADR-0007). The resource
// name is the connection string name that the API and the migration runner read (ConnectionStrings__teamgym).
var database = builder.AddPostgres("postgres")
    .WithImageTag("17-alpine")
    .AddDatabase("teamgym", databaseName: "teamgym_arena");

// Applies every module's migrations, then exits. The API starts only after it has succeeded.
var migrations = builder.AddProject<Projects.TeamGym_Arena_MigrationService>("migrations")
    .WithReference(database)
    .WaitFor(database);

var api = builder.AddProject<Projects.TeamGym_Arena_Api>("api")
    .WithReference(database)
    .WaitForCompletion(migrations)
    .WithHttpHealthCheck("/health");

// The Vite dev server proxies /api, /bff and /hubs to the API (src/Web/vite.config.ts), so the browser sees one
// origin, as it does in production where the API serves the built SPA (ADR-0005).
builder.AddViteApp("web", "../Web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
