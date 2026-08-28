using System.Net.Mail;
using PulseTech.Api.Common.Auth;

namespace PulseTech.Api.Features.Auth;

public static class AuthEndpoints
{
    private const int MinPasswordLength = 8;

    // BCrypt only considers the first 72 bytes of the input; reject longer secrets explicitly
    // instead of silently truncating them.
    private const int MaxPasswordLength = 72;

    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<AuthRepository>();
        services.AddScoped<AuthService>();
        return services;
    }

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", Register).WithName("Register").AllowAnonymous();
        group.MapPost("/login", Login).WithName("Login").AllowAnonymous();

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        AuthService authService,
        CancellationToken cancellationToken)
    {
        if (Validate(request.Email, request.Password) is { } validationError)
        {
            return validationError;
        }

        var result = await authService.RegisterAsync(request, cancellationToken);

        return result.Status switch
        {
            RegisterStatus.Success =>
                TypedResults.Created($"/api/users/{result.Response!.UserId}", result.Response),
            RegisterStatus.EmailAlreadyRegistered =>
                TypedResults.Conflict(new { message = "Email is already registered." }),
            _ => TypedResults.Problem("Unexpected registration outcome."),
        };
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        AuthService authService,
        CancellationToken cancellationToken)
    {
        if (Validate(request.Email, request.Password) is { } validationError)
        {
            return validationError;
        }

        var result = await authService.LoginAsync(request, cancellationToken);

        return result.Status switch
        {
            LoginStatus.Success => TypedResults.Ok(result.Response),
            LoginStatus.AccountDisabled =>
                TypedResults.Problem("Account is disabled.", statusCode: StatusCodes.Status403Forbidden),
            _ => TypedResults.Problem(
                "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized),
        };
    }

    private static IResult? Validate(string? email, string? password)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email, out _))
        {
            errors["email"] = ["A valid email address is required."];
        }

        if (string.IsNullOrEmpty(password) || password.Length is < MinPasswordLength or > MaxPasswordLength)
        {
            errors["password"] = [$"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters."];
        }

        return errors.Count > 0 ? TypedResults.ValidationProblem(errors) : null;
    }
}
