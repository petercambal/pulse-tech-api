namespace PulseTech.Api.Features.Telemetry;

public static class TelemetryEndpoints
{
    private const int DefaultLimit = 10_000;
    private const int MaxLimit = 50_000;

    public static IServiceCollection AddTelemetryFeature(this IServiceCollection services)
    {
        services.AddScoped<TelemetryRepository>();
        return services;
    }

    public static IEndpointRouteBuilder MapTelemetryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/devices/{deviceId:guid}/telemetry/query", GetDeviceTelemetry)
            .WithName("GetDeviceTelemetry")
            .WithTags("Telemetry");

        return app;
    }

    private static async Task<IResult> GetDeviceTelemetry(
        Guid deviceId,
        GetDeviceTelemetryRequest? request,
        TelemetryRepository repository,
        CancellationToken cancellationToken)
    {
        request ??= new GetDeviceTelemetryRequest();

        var to = request.To ?? DateTimeOffset.UtcNow;
        var from = request.From ?? to.AddHours(-24);

        if (from >= to)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["from"] = ["'from' must be earlier than 'to'."],
            });
        }

        var limit = Math.Clamp(request.Limit ?? DefaultLimit, 1, MaxLimit);

        if (!await repository.DeviceExistsAsync(deviceId, cancellationToken))
        {
            return TypedResults.NotFound(new { message = $"Device '{deviceId}' was not found." });
        }

        var readings = await repository.GetForDeviceAsync(deviceId, from, to, limit, cancellationToken);

        return TypedResults.Ok(new DeviceTelemetryResponse(deviceId, from, to, readings.Count, readings));
    }
}
