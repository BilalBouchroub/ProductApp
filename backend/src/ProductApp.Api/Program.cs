using Microsoft.AspNetCore.Authentication.JwtBearer;
using ProductApp.Api.Infrastructure;
using ProductApp.Application;
using ProductApp.Infrastructure;
using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using ProductApp.Application.Auth;
using ProductApp.Infrastructure.Identity;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ProductApp.Domain.Common;
using ProductApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddOptionalDotEnv(builder.Environment.ContentRootPath);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "ProductApp API", Version = "v1", Description = "API REST de production et d'aide à la décision commerciale." });
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Saisir le jeton JWT obtenu via POST /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("authentication", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("smart-product", httpContext => RateLimitPartition.GetTokenBucketLimiter(
        httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 12,
            TokensPerPeriod = 12,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueLimit = 1,
            AutoReplenishment = true
        }));
});
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
    throw new InvalidOperationException("Configure Jwt:SigningKey with at least 32 characters (prefer Jwt__SigningKey environment variable).");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = true,
        ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = System.Security.Claims.ClaimTypes.Name,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                context.Fail("Invalid user identifier.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var isActive = await dbContext.ApplicationUsers.AsNoTracking()
                .AnyAsync(user => user.Id == userId && user.Status == UserStatus.Active, context.HttpContext.RequestAborted);
            if (!isActive) context.Fail("User account is inactive.");
        }
    };
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy(SecurityPolicies.Administrator, p => p.RequireRole(AppRoles.Administrator));
    options.AddPolicy(SecurityPolicies.Production, p => p.RequireRole(AppRoles.Administrator, AppRoles.ProductionManager));
    options.AddPolicy(SecurityPolicies.Commercial, p => p.RequireRole(AppRoles.Administrator, AppRoles.CommercialManager));
    foreach (var permission in AppPermissions.All) options.AddPolicy(permission, p => p.RequireClaim(ProductAppClaimTypes.Permission, permission));
});
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductApp API v1"); options.DisplayRequestDuration(); });
}
else
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});
app.UseStaticFiles();
var productImagesDirectory = Path.Combine(
    app.Environment.ContentRootPath, "wwwroot", "uploads", "products");
Directory.CreateDirectory(productImagesDirectory);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(productImagesDirectory),
    RequestPath = "/uploads/products"
});
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/api/health").AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program;

public static class SecurityPolicies
{
    public const string Administrator = "AdministratorAccess";
    public const string Production = "ProductionAccess";
    public const string Commercial = "CommercialAccess";
}
