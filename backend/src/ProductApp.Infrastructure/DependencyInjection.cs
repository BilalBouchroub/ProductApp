using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApp.Application.Products;
using ProductApp.Application.Products.Images;
using ProductApp.Infrastructure.Persistence;
using ProductApp.Infrastructure.Products;
using ProductApp.Application.ProductionSteps;
using ProductApp.Infrastructure.ProductionSteps;
using ProductApp.Application.SapIntegration;
using ProductApp.Infrastructure.SapIntegration;
using ProductApp.Application.MarketAnalysis;
using ProductApp.Infrastructure.MarketAnalysis;
using ProductApp.Application.Production;
using ProductApp.Application.Production.References;
using ProductApp.Infrastructure.Production;
using ProductApp.Application.Commercial;
using ProductApp.Infrastructure.Commercial;
using Microsoft.AspNetCore.Identity;
using ProductApp.Application.Auth;
using ProductApp.Domain.Identity;
using ProductApp.Infrastructure.Identity;
using ProductApp.Application.Administration;
using ProductApp.Infrastructure.Administration;
using ProductApp.Application.Common;
using ProductApp.Application.SmartProduct;
using ProductApp.Infrastructure.SmartProduct;

namespace ProductApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
        services.AddHttpContextAccessor();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<EmailOptions>(options =>
        {
            options.SmtpHost = GetRequiredEmailSetting(configuration, "SMTP_HOST", "Email:SmtpHost");
            options.SmtpPort = GetIntegerEmailSetting(configuration, "SMTP_PORT", "Email:SmtpPort", 587);
            options.EnableSsl = GetBooleanEmailSetting(configuration, "SMTP_ENABLE_SSL", "Email:EnableSsl", true);
            options.Username = GetRequiredEmailSetting(configuration, "SMTP_USER", "Email:Username");
            options.Password = GetRequiredEmailSetting(configuration, "SMTP_PASSWORD", "Email:Password");
            options.FromAddress = GetRequiredEmailSetting(configuration, "SMTP_FROM_EMAIL", "Email:FromAddress");
            options.FromName = GetEmailSetting(configuration, "PLATFORM_NAME", "Email:FromName", "ProductApp");
            options.FrontendBaseUrl = GetEmailSetting(
                configuration, "PLATFORM_URL", "Email:FrontendBaseUrl", "http://localhost:5173");
        });
        services.Configure<MailtrapApiOptions>(options =>
        {
            options.ApiToken = GetRequiredEmailSetting(
                configuration, "MAILTRAP_API_TOKEN", "Email:MailtrapApiToken");
            options.FromAddress = GetRequiredEmailSetting(
                configuration, "MAILTRAP_FROM_EMAIL", "Email:MailtrapFromAddress");
            options.FromName = GetEmailSetting(
                configuration, "PLATFORM_NAME", "Email:FromName", "ProductApp");
            options.FrontendBaseUrl = GetEmailSetting(
                configuration, "PLATFORM_URL", "Email:FrontendBaseUrl", "http://localhost:5173");
        });
        services.AddSingleton(TimeProvider.System);
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddRoles<Role>()
            .AddUserStore<ProductAppUserStore>()
            .AddRoleStore<ProductAppRoleStore>()
            .AddDefaultTokenProviders();
        services.AddScoped<IAuthService, JwtAuthenticationService>();
        services.AddScoped<IPasswordResetNotifier, LoggingPasswordResetNotifier>();
        services.AddScoped<IAdministrationRepository, AdministrationRepository>();
        var emailProvider = GetEmailSetting(
            configuration, "EMAIL_PROVIDER", "Email:Provider", "Smtp");
        if (emailProvider.Equals("MailtrapApi", StringComparison.OrdinalIgnoreCase))
        {
            var apiBaseUrl = GetEmailSetting(
                configuration,
                "MAILTRAP_API_BASE_URL",
                "Email:MailtrapApiBaseUrl",
                "https://send.api.mailtrap.io/");
            services.AddHttpClient<IUserInvitationSender, MailtrapApiUserInvitationSender>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(20);
            });
        }
        else if (emailProvider.Equals("Smtp", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IUserInvitationSender, SmtpUserInvitationSender>();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported email provider '{emailProvider}'. Use 'Smtp' or 'MailtrapApi'.");
        }
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductImageStorage, LocalProductImageStorage>();
        services.AddScoped<IProductionStepRepository, ProductionStepRepository>();
        services.AddScoped<IMarketAnalysisRepository, MarketAnalysisRepository>();
        services.AddScoped<IProductionWorkflowRepository, ProductionWorkflowRepository>();
        services.AddScoped<IProductionReferenceRepository, ProductionReferenceRepository>();
        services.AddScoped<ICommercialRepository, CommercialRepository>();
        services.Configure<SmartProductOptions>(configuration.GetSection(SmartProductOptions.SectionName));
        services.PostConfigure<SmartProductOptions>(options =>
        {
            options.Provider = configuration["SMART_PRODUCT_PROVIDER"] ?? options.Provider;
            if (options.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                options.Model = configuration["GEMINI_MODEL"] ?? options.Model;
                options.EmbeddingModel = configuration["GEMINI_EMBEDDING_MODEL"] ?? options.EmbeddingModel;
                options.GeminiApiBaseUrl = configuration["GEMINI_API_BASE_URL"] ?? options.GeminiApiBaseUrl;
            }
            else
            {
                options.Model = configuration["OPENAI_MODEL"] ?? options.Model;
                options.EmbeddingModel = configuration["OPENAI_EMBEDDING_MODEL"] ?? options.EmbeddingModel;
                options.OpenAiApiBaseUrl = configuration["OPENAI_API_BASE_URL"] ?? options.OpenAiApiBaseUrl;
            }
        });

        var smartProductProvider = configuration["SMART_PRODUCT_PROVIDER"]
            ?? configuration[$"{SmartProductOptions.SectionName}:Provider"]
            ?? "Gemini";
        if (smartProductProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IAiProvider, GeminiProvider>(ConfigureGeminiClient);
        }
        else if (smartProductProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IAiProvider, OpenAiProvider>(ConfigureOpenAiClient);
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported SMART PRODUCT provider '{smartProductProvider}'. Use 'Gemini' or 'OpenAI'.");
        }
        services.AddScoped<IAiConversationRepository, AiConversationRepository>();
        services.AddScoped<IAiBusinessToolCatalog, ProductAppAiToolCatalog>();
        services.AddScoped<IAiAttachmentStorage, LocalAiAttachmentStorage>();
        services.AddScoped<IAiDocumentExtractor, OfficeTextDocumentExtractor>();
        services.AddScoped<IAiKnowledgeService, AiKnowledgeService>();

        var sapProviderName = configuration[$"{SapIntegrationOptions.SectionName}:Provider"]
            ?? SapProvider.Mock.ToString();
        if (!Enum.TryParse<SapProvider>(sapProviderName, true, out var sapProvider))
        {
            throw new InvalidOperationException(
                $"Unsupported SAP provider '{sapProviderName}'. Use 'Mock' or 'OData'.");
        }

        if (sapProvider == SapProvider.OData)
        {
            throw new InvalidOperationException(
                "The SAP OData provider is selected but has not been implemented yet.");
        }

        services.AddSingleton<ISapDataProvider, MockSapDataProvider>();

        return services;
    }

    private static string GetRequiredEmailSetting(
        IConfiguration configuration, string environmentKey, string sectionKey) =>
        GetEmailSetting(configuration, environmentKey, sectionKey, string.Empty);

    private static string GetEmailSetting(
        IConfiguration configuration, string environmentKey, string sectionKey, string fallback)
    {
        var environmentValue = configuration[environmentKey];
        if (!string.IsNullOrWhiteSpace(environmentValue)) return environmentValue;

        var sectionValue = configuration[sectionKey];
        return string.IsNullOrWhiteSpace(sectionValue) ? fallback : sectionValue;
    }

    private static int GetIntegerEmailSetting(
        IConfiguration configuration, string environmentKey, string sectionKey, int fallback)
    {
        var value = GetEmailSetting(configuration, environmentKey, sectionKey, fallback.ToString());
        return int.TryParse(value, out var parsed) && parsed is > 0 and <= 65535 ? parsed : fallback;
    }

    private static bool GetBooleanEmailSetting(
        IConfiguration configuration, string environmentKey, string sectionKey, bool fallback)
    {
        var value = GetEmailSetting(configuration, environmentKey, sectionKey, fallback.ToString());
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static void ConfigureGeminiClient(IServiceProvider provider, HttpClient client)
    {
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SmartProductOptions>>().Value;
        client.BaseAddress = new Uri(options.GeminiApiBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 15, 300));
    }

    private static void ConfigureOpenAiClient(IServiceProvider provider, HttpClient client)
    {
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SmartProductOptions>>().Value;
        client.BaseAddress = new Uri(options.OpenAiApiBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 15, 300));
    }
}
