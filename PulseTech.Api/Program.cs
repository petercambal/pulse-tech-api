using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PulseTech.Api.Common.Auth;
using PulseTech.Api.Common.Database;
using PulseTech.Api.Features.Auth;
using PulseTech.Api.Features.Devices;
using PulseTech.Api.Features.Telemetry;

var builder = WebApplication.CreateBuilder(args);

const string frontendCorsPolicy = "frontend";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAuthFeature();
builder.Services.AddDevicesFeature();
builder.Services.AddTelemetryFeature();

// --- Authentication (custom JWT Bearer, HMAC-SHA256) ---------------------------
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Configuration section '{JwtOptions.SectionName}' is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || Encoding.UTF8.GetByteCount(jwtOptions.SecretKey) < 32)
{
    throw new InvalidOperationException(
        "JwtOptions:SecretKey must be at least 32 bytes (256 bits) long for HMAC-SHA256.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep incoming claim types verbatim so they line up with ClaimTypes.* used by the token issuer.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapProtectedDemoEndpoints();
app.MapDeviceEndpoints();
app.MapTelemetryEndpoints();

app.Run();
