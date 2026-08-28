using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PulseTech.Api.Features.Auth;

/// <summary>
/// Demonstrates the two RBAC protection styles on Minimal API endpoints.
/// </summary>
public static class ProtectedDemoEndpoints
{
    public static IEndpointRouteBuilder MapProtectedDemoEndpoints(this IEndpointRouteBuilder app)
    {
        // 1) Any authenticated user (valid Bearer token, any role).
        app.MapGet("/api/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithTags("Auth")
            .RequireAuthorization();

        // 2) Restricted to the "Admin" role.
        app.MapGet("/api/admin/ping", AdminPing)
            .WithName("AdminPing")
            .WithTags("Auth")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        return app;
    }

    private static IResult GetCurrentUser(ClaimsPrincipal user) =>
        TypedResults.Ok(new
        {
            userId = user.FindFirstValue(ClaimTypes.NameIdentifier),
            email = user.FindFirstValue(ClaimTypes.Email),
            role = user.FindFirstValue(ClaimTypes.Role),
        });

    private static IResult AdminPing() =>
        TypedResults.Ok(new { message = "pong from the admin area" });
}
