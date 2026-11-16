using TeamGym.Arena.Modules.Applications;
using TeamGym.Arena.Modules.Competitions;
using TeamGym.Arena.Modules.Organisations;
using TeamGym.Arena.Modules.Scheduling;
using TeamGym.Arena.Modules.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services
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

app.MapDefaultEndpoints();

app.MapUsersEndpoints();
app.MapOrganisationsEndpoints();
app.MapCompetitionsEndpoints();
app.MapApplicationsEndpoints();
app.MapSchedulingEndpoints();

app.Run();
