using System.Net;

namespace ProductApp.Infrastructure.Identity;

internal sealed record UserInvitationEmail(string Subject, string PlainText, string Html);

internal static class UserInvitationEmailTemplate
{
    public static UserInvitationEmail Create(
        string platformName,
        string frontendBaseUrl,
        string fullName,
        string email,
        string role,
        string temporaryPassword)
    {
        var loginUrl = $"{frontendBaseUrl.TrimEnd('/')}/login";
        var subject = $"Bienvenue sur {platformName} — votre compte est prêt";
        var plainText = $"""
            Bonjour {fullName},

            Votre compte {platformName} a été créé par un administrateur.

            Adresse de connexion : {loginUrl}
            Identifiant : {email}
            Rôle : {role}
            Mot de passe temporaire : {temporaryPassword}

            Pour protéger votre compte, connectez-vous puis changez immédiatement ce mot de passe.
            Ne partagez pas cet e-mail ni vos informations de connexion.

            L’équipe {platformName}
            """;

        var safePlatformName = WebUtility.HtmlEncode(platformName);
        var safeFullName = WebUtility.HtmlEncode(fullName);
        var safeEmail = WebUtility.HtmlEncode(email);
        var safeRole = WebUtility.HtmlEncode(role);
        var safePassword = WebUtility.HtmlEncode(temporaryPassword);
        var safeLoginUrl = WebUtility.HtmlEncode(loginUrl);

        var html = $$"""
            <!doctype html>
            <html lang="fr">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width,initial-scale=1">
              <title>{{WebUtility.HtmlEncode(subject)}}</title>
            </head>
            <body style="margin:0;background:#f1f5f9;color:#0f172a;font-family:Arial,Helvetica,sans-serif;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f1f5f9;padding:32px 12px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:620px;background:#ffffff;border:1px solid #e2e8f0;border-radius:16px;overflow:hidden;box-shadow:0 12px 32px rgba(15,23,42,.08);">
                      <tr>
                        <td style="padding:28px 36px;background:#0f172a;color:#ffffff;">
                          <div style="font-size:12px;letter-spacing:2px;text-transform:uppercase;color:#93c5fd;font-weight:700;">Plateforme industrielle</div>
                          <div style="margin-top:8px;font-size:26px;line-height:1.2;font-weight:800;">{{safePlatformName}}</div>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:36px;">
                          <h1 style="margin:0 0 16px;font-size:24px;line-height:1.3;color:#0f172a;">Bienvenue, {{safeFullName}}</h1>
                          <p style="margin:0 0 24px;font-size:15px;line-height:1.7;color:#475569;">
                            Un administrateur vient de créer votre compte. Vous pouvez maintenant accéder à votre espace professionnel.
                          </p>
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:0 0 26px;background:#f8fafc;border:1px solid #e2e8f0;border-radius:12px;">
                            <tr><td style="padding:18px 20px 8px;font-size:12px;color:#64748b;text-transform:uppercase;font-weight:700;">Identifiant</td></tr>
                            <tr><td style="padding:0 20px 14px;font-size:15px;color:#0f172a;font-weight:700;">{{safeEmail}}</td></tr>
                            <tr><td style="padding:0 20px 8px;font-size:12px;color:#64748b;text-transform:uppercase;font-weight:700;">Rôle attribué</td></tr>
                            <tr><td style="padding:0 20px 14px;font-size:15px;color:#0f172a;font-weight:700;">{{safeRole}}</td></tr>
                            <tr><td style="padding:0 20px 8px;font-size:12px;color:#64748b;text-transform:uppercase;font-weight:700;">Mot de passe temporaire</td></tr>
                            <tr><td style="padding:0 20px 20px;"><span style="display:inline-block;padding:10px 12px;background:#e0f2fe;border-radius:8px;color:#075985;font-family:Consolas,monospace;font-size:16px;font-weight:700;letter-spacing:.5px;">{{safePassword}}</span></td></tr>
                          </table>
                          <table role="presentation" cellspacing="0" cellpadding="0" style="margin:0 auto 26px;">
                            <tr><td style="border-radius:9px;background:#2563eb;"><a href="{{safeLoginUrl}}" style="display:inline-block;padding:14px 24px;color:#ffffff;text-decoration:none;font-size:15px;font-weight:700;">Se connecter à {{safePlatformName}}</a></td></tr>
                          </table>
                          <div style="padding:16px 18px;background:#fff7ed;border-left:4px solid #f97316;border-radius:8px;color:#9a3412;font-size:13px;line-height:1.6;">
                            <strong>Sécurité :</strong> changez ce mot de passe dès votre première connexion et ne transférez pas cet e-mail.
                          </div>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:20px 36px;background:#f8fafc;border-top:1px solid #e2e8f0;text-align:center;font-size:12px;line-height:1.6;color:#64748b;">
                          Cet e-mail automatique a été envoyé par {{safePlatformName}}.<br>Si vous n’attendiez pas cette invitation, contactez votre administrateur.
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        return new UserInvitationEmail(subject, plainText, html);
    }
}
