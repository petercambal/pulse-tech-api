namespace PulseTech.Api.Features.Devices;

public static class DeviceEndpoints
{
    private const int DefaultPageSize = 100;
    private const int MaxPageSize = 500;
    private const int NameMaxLength = 100;

    public static IServiceCollection AddDevicesFeature(this IServiceCollection services)
    {
        services.AddScoped<DeviceRepository>();
        return services;
    }

    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/devices").WithTags("Devices");

        group.MapGet("/", ListDevices).WithName("ListDevices");
        group.MapGet("/{id:guid}", GetDevice).WithName("GetDevice");
        group.MapPost("/", CreateDevice).WithName("CreateDevice");
        group.MapPut("/{id:guid}", UpdateDevice).WithName("UpdateDevice");
        group.MapDelete("/{id:guid}", DeleteDevice).WithName("DeleteDevice");

        return app;
    }

    private static async Task<IResult> ListDevices(
        DeviceRepository repository,
        CancellationToken cancellationToken,
        Guid? ownerUserId = null,
        Guid? appId = null,
        bool? isActive = null,
        int limit = DefaultPageSize,
        int offset = 0)
    {
        limit = Math.Clamp(limit, 1, MaxPageSize);
        offset = Math.Max(offset, 0);

        var devices = await repository.ListAsync(ownerUserId, appId, isActive, limit, offset, cancellationToken);

        return TypedResults.Ok(devices);
    }

    private static async Task<IResult> GetDevice(
        Guid id,
        DeviceRepository repository,
        CancellationToken cancellationToken)
    {
        var device = await repository.GetByIdAsync(id, cancellationToken);

        return device is null ? NotFound(id) : TypedResults.Ok(device);
    }

    private static async Task<IResult> CreateDevice(
        CreateDeviceRequest request,
        DeviceRepository repository,
        CancellationToken cancellationToken)
    {
        if (ValidateName(request.Name) is { } nameError)
        {
            return nameError;
        }

        if (!await repository.OwnerExistsAsync(request.OwnerUserId, cancellationToken))
        {
            return ValidationError("ownerUserId", $"User '{request.OwnerUserId}' was not found.");
        }

        if (request.AppId is { } appId && !await repository.ApplicationExistsAsync(appId, cancellationToken))
        {
            return ValidationError("appId", $"Application '{appId}' was not found.");
        }

        var created = await repository.CreateAsync(request, cancellationToken);

        return TypedResults.Created($"/api/devices/{created.Id}", created);
    }

    private static async Task<IResult> UpdateDevice(
        Guid id,
        UpdateDeviceRequest request,
        DeviceRepository repository,
        CancellationToken cancellationToken)
    {
        if (ValidateName(request.Name) is { } nameError)
        {
            return nameError;
        }

        if (request.AppId is { } appId && !await repository.ApplicationExistsAsync(appId, cancellationToken))
        {
            return ValidationError("appId", $"Application '{appId}' was not found.");
        }

        var updated = await repository.UpdateAsync(id, request, cancellationToken);

        return updated is null ? NotFound(id) : TypedResults.Ok(updated);
    }

    private static async Task<IResult> DeleteDevice(
        Guid id,
        DeviceRepository repository,
        CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);

        return deleted ? TypedResults.NoContent() : NotFound(id);
    }

    private static IResult? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ValidationError("name", "'name' is required.");
        }

        return name.Length > NameMaxLength
            ? ValidationError("name", $"'name' must be at most {NameMaxLength} characters.")
            : null;
    }

    private static IResult ValidationError(string field, string message) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    private static IResult NotFound(Guid id) =>
        TypedResults.NotFound(new { message = $"Device '{id}' was not found." });
}
