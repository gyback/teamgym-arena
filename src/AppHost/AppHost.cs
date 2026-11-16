var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TeamGym_Arena_Api>("api")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
