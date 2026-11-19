var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.TeamGym_Arena_Api>("api")
    .WithHttpHealthCheck("/health");

// The Vite dev server proxies /api, /bff and /hubs to the API (src/Web/vite.config.ts), so the browser sees one
// origin, as it does in production where the API serves the built SPA (ADR-0005).
builder.AddViteApp("web", "../Web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
