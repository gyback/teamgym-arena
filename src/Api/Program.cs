using System.Text.Json.Serialization;
using TeamGym.Arena.Api.OpenApi;
using TeamGym.Arena.Api.Spa;
using TeamGym.Arena.Modules.Applications;
using TeamGym.Arena.Modules.Competitions;
using TeamGym.Arena.Modules.Organisations;
using TeamGym.Arena.Modules.Scheduling;
using TeamGym.Arena.Modules.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddApiOpenApi();

// The web defaults also read numbers from JSON strings, so the contract would describe every number as
// `integer | string`. The API accepts JSON numbers only.
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);

// Each module registers its own services and its DbContext. The API never migrates the schema: the migration
// runner in src/MigrationService does that before the API starts (ADR-0007).
builder
    .AddUsersModule()
    .AddOrganisationsModule()
    .AddCompetitionsModule()
    .AddApplicationsModule()
    .AddSchedulingModule();

var app = builder.Build();

// Unhandled exceptions and empty error responses become RFC 9457 problem details.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSpaStaticFiles();

app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapUsersEndpoints();
app.MapOrganisationsEndpoints();
app.MapCompetitionsEndpoints();
app.MapApplicationsEndpoints();
app.MapSchedulingEndpoints();

// Last, so that every API, BFF and health endpoint takes precedence over the client-side routes.
app.MapSpaFallback();

app.Run();
