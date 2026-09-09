using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Options;
using ProductApp.Application.Administration;

namespace ProductApp.Infrastructure.Identity;

public sealed class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "ProductApp";
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}

public sealed class SmtpUserInvitationSender(IOptions<EmailOptions> options) : IUserInvitationSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string email, string fullName, string role,
        string temporaryPassword, CancellationToken cancellationToken)
    {
        ValidateConfiguration();
        cancellationToken.ThrowIfCancellationRequested();
        var content = UserInvitationEmailTemplate.Create(
            _options.FromName,
            _options.FrontendBaseUrl,
            fullName,
            email,
            role,
            temporaryPassword);

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = content.Subject,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            HeadersEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(email, fullName));
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            content.PlainText, Encoding.UTF8, MediaTypeNames.Text.Plain));
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            content.Html, Encoding.UTF8, MediaTypeNames.Text.Html));

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_options.Username, _options.Password),
            Timeout = 20_000
        };
        await client.SendMailAsync(message, cancellationToken);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost)
            || string.IsNullOrWhiteSpace(_options.Username)
            || string.IsNullOrWhiteSpace(_options.Password)
            || string.IsNullOrWhiteSpace(_options.FromAddress))
            throw new InvalidOperationException(
                "Le service e-mail SMTP n’est pas configuré. Définissez SMTP_HOST, SMTP_USER, SMTP_PASSWORD et SMTP_FROM_EMAIL.");
    }
}
