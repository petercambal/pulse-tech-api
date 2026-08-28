using PulseTech.Api.Common.Database;
using PulseTech.Api.Features.Devices;
using PulseTech.Api.Features.Telemetry;

var builder = WebApplication.CreateBuilder(args);

const string frontendCorsPolicy = "frontend";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddDevicesFeature();
builder.Services.AddTelemetryFeature();

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Run migrations before the API starts serving requests.
app.RunDatabaseMigrations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(frontendCorsPolicy);

app.MapDeviceEndpoints();
app.MapTelemetryEndpoints();

app.Run();
