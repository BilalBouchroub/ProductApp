using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ProductApp.Application.Administration;

namespace ProductApp.Infrastructure.Identity;

public sealed class MailtrapApiOptions
{
    public string ApiToken { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "ProductApp";
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}

public sealed class MailtrapApiUserInvitationSender(
    HttpClient httpClient,
    IOptions<MailtrapApiOptions> options) : IUserInvitationSender
{
    private readonly MailtrapApiOptions _options = options.Value;

    public async Task SendAsync(
        string email,
        string fullName,
        string role,
        string temporaryPassword,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();
        var content = UserInvitationEmailTemplate.Create(
            _options.FromName,
            _options.FrontendBaseUrl,
            fullName,
            email,
            role,
            temporaryPassword);
        var payload = new
        {
            from = new { email = _options.FromAddress, name = _options.FromName },
            to = new[] { new { email, name = fullName } },
            subject = content.Subject,
            text = content.PlainText,
            html = content.Html,
            category = "User Invitation"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/send")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Mailtrap Email Sending a refusé la requête (HTTP {(int)response.StatusCode}).");
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiToken)
            || string.IsNullOrWhiteSpace(_options.FromAddress))
            throw new InvalidOperationException(
                "Mailtrap Email Sending n’est pas configuré. Définissez MAILTRAP_API_TOKEN et MAILTRAP_FROM_EMAIL.");
    }
}
