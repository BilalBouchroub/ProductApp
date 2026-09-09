using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductApp.Application.Auth;

namespace ProductApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [AllowAnonymous, HttpPost("login"), EnableRateLimiting("authentication")]
    public Task<TokenPairDto> Login(LoginRequest request, CancellationToken ct) =>
        sender.Send(new LoginCommand(request.Email, request.Password, ClientIp(), Request.Headers.UserAgent.ToString()), ct);

    [AllowAnonymous, HttpPost("refresh"), EnableRateLimiting("authentication")]
    public Task<TokenPairDto> Refresh(RefreshRequest request, CancellationToken ct) =>
        sender.Send(new RefreshTokenCommand(request.RefreshToken, ClientIp(), Request.Headers.UserAgent.ToString()), ct);

    [AllowAnonymous, HttpPost("forgot-password"), EnableRateLimiting("authentication")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    { await sender.Send(new ForgotPasswordCommand(request.Email), ct); return Accepted(); }

    [AllowAnonymous, HttpPost("reset-password"), EnableRateLimiting("authentication")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    { await sender.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword), ct); return NoContent(); }

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    { await sender.Send(new LogoutCommand(request.RefreshToken, ClientIp()), ct); return NoContent(); }

    [Authorize, HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    { await sender.Send(new ChangePasswordCommand(CurrentUserId(), request.CurrentPassword, request.NewPassword), ct); return NoContent(); }

    [Authorize, HttpGet("me")]
    public Task<AuthenticatedUserDto> Me(CancellationToken ct) => sender.Send(new GetCurrentUserQuery(CurrentUserId()), ct);

    private Guid CurrentUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new AuthenticationException("Authenticated user identifier is missing.");
    private string ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
