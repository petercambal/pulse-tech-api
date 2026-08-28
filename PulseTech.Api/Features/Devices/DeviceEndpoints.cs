using System.Security.Claims;
using PulseTech.Api.Common.Auth;

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
        // Every device operation is scoped to the authenticated caller.
        var group = app.MapGroup("/api/devices")
            .WithTags("Devices")
            .RequireAuthorization();

        group.MapGet("/", ListDevices).WithName("ListDevices");
        group.MapGet("/{id:guid}", GetDevice).WithName("GetDevice");
        group.MapPost("/", CreateDevice).WithName("CreateDevice");
        group.MapPut("/{id:guid}", UpdateDevice).WithName("UpdateDevice");
        group.MapDelete("/{id:guid}", DeleteDevice).WithName("DeleteDevice");

        return app;
    }

    private static async Task<IResult> ListDevices(
        ClaimsPrincipal user,
        DeviceRepository repository,
        CancellationToken cancellationToken,
        Guid? appId = null,
        bool? isActive = null,
        int limit = DefaultPageSize,
        int offset = 0)
    {
        limit = Math.Clamp(limit, 1, MaxPageSize);
        offset = Math.Max(offset, 0);

        var devices = await repository.ListAsync(user.GetUserId(), appId, isActive, limit, offset, cancellationToken);

        return TypedResults.Ok(devices);
    }

    private static async Task<IResult> GetDevice(
        Guid id,
        ClaimsPrincipal user,
        DeviceRepository repository,
        CancellationToken cancellationToken)
    {
        var device = await repository.GetByIdAsync(id, user.GetUserId(), cancellationToken);

        return device is null ? NotFound(id) : TypedResults.Ok(device);
    }

    private static async Task<IResult> CreateDevice(
        CreateDeviceRequest request,
        ClaimsPrincipal user,
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

        var created = await repository.CreateAsync(user.GetUserId(), request, cancellationToken);

        return TypedResults.Created($"/api/devices/{created.Id}", created);
    }

    private static async Task<IResult> UpdateDevice(
        Guid id,
        UpdateDeviceRequest request,
        ClaimsPrincipal user,
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

        var updated = await repository.UpdateAsync(id, user.GetUserId(), request, cancellationToken);

        return updated is null ? NotFound(id) : TypedResults.Ok(updated);
    }

    private static async Task<IResult> DeleteDevice(
        Guid id,
        ClaimsPrincipal user,
        DeviceRepository repository,
        CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, user.GetUserId(), cancellationToken);

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
