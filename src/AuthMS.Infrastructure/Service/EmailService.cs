using Application.Interfaces.IServices.IAuthServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net;

namespace Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string? _senderPassword;
        private readonly bool _enableEmails;
        private readonly bool _enableSsl;
        private readonly SecureSocketOptions _socketSecurity;
        private readonly string _appName;
        private readonly string _supportEmail;
        private readonly string? _portalBaseUrl;
        private readonly string _verificationPath;
        private readonly string _passwordResetPath;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailTemplateRenderer _templateRenderer;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _templateRenderer = new EmailTemplateRenderer(loggerFactory.CreateLogger<EmailTemplateRenderer>());

            _smtpServer = configuration["EmailSettings:SmtpServer"]
                          ?? Environment.GetEnvironmentVariable("EmailSettings__SmtpServer")
                          ?? "smtp.gmail.com";

            _smtpPort = int.TryParse(
                configuration["EmailSettings:SmtpPort"] ?? Environment.GetEnvironmentVariable("EmailSettings__SmtpPort"),
                out var smtpPort)
                ? smtpPort
                : 587;

            _smtpUser = configuration["EmailSettings:SmtpUser"]
                        ?? Environment.GetEnvironmentVariable("EmailSettings__SmtpUser")
                        ?? configuration["EmailSettings:SenderEmail"]
                        ?? Environment.GetEnvironmentVariable("EmailSettings__SenderEmail")
                        ?? "cuidarmed.notificaciones@gmail.com";

            _senderEmail = configuration["EmailSettings:SenderEmail"]
                           ?? Environment.GetEnvironmentVariable("EmailSettings__SenderEmail")
                           ?? "cuidarmed.notificaciones@gmail.com";

            _senderName = configuration["EmailSettings:SenderName"]
                          ?? Environment.GetEnvironmentVariable("EmailSettings__SenderName")
                          ?? "Hygea";

            _senderPassword = configuration["EmailSettings:SenderPassword"]
                              ?? Environment.GetEnvironmentVariable("EmailSettings__SenderPassword");

            _enableEmails = bool.TryParse(
                configuration["EmailSettings:EnableEmails"] ?? Environment.GetEnvironmentVariable("EmailSettings__EnableEmails"),
                out var enableEmails)
                ? enableEmails
                : true;

            _enableSsl = bool.TryParse(
                configuration["EmailSettings:EnableSsl"] ?? Environment.GetEnvironmentVariable("EmailSettings__EnableSsl"),
                out var enableSsl)
                ? enableSsl
                : true;

            _socketSecurity = ResolveSocketSecurity(configuration);

            _appName = configuration["EmailSettings:AppName"]
                       ?? Environment.GetEnvironmentVariable("EmailSettings__AppName")
                       ?? "Hygea";

            _supportEmail = configuration["EmailSettings:SupportEmail"]
                            ?? Environment.GetEnvironmentVariable("EmailSettings__SupportEmail")
                            ?? _senderEmail;

            _portalBaseUrl = configuration["EmailSettings:PortalBaseUrl"]
                             ?? Environment.GetEnvironmentVariable("EmailSettings__PortalBaseUrl");

            _verificationPath = configuration["EmailSettings:VerificationPath"]
                                ?? Environment.GetEnvironmentVariable("EmailSettings__VerificationPath")
                                ?? "/confirmacion.html";

            _passwordResetPath = configuration["EmailSettings:PasswordResetPath"]
                                 ?? Environment.GetEnvironmentVariable("EmailSettings__PasswordResetPath")
                                 ?? "/blanqueo.html";

            if (_enableEmails && string.IsNullOrWhiteSpace(_senderPassword))
            {
                throw new InvalidOperationException("Falta EmailSettings:SenderPassword en configuración o variables de entorno.");
            }
        }

        public Task SendPasswordResetEmail(string email, string resetCode)
        {
            if (!_enableEmails)
            {
                _logger.LogWarning("SMTP disabled. Password reset code for {Email}: {ResetCode}", email, resetCode);
            }

            const string subject = "Restablecimiento de contrasena";
            var body = _templateRenderer.Render(
                "password-reset.html",
                new Dictionary<string, string?>
                {
                    ["Subject"] = subject,
                    ["AppName"] = _appName,
                    ["RecipientEmail"] = email,
                    ["ResetCode"] = resetCode,
                    ["ActionUrl"] = BuildPortalUrl(_passwordResetPath, email),
                    ["SupportEmail"] = _supportEmail
                },
                $"<html><body><h2>{WebUtility.HtmlEncode(_appName)}</h2><p>Tu codigo de restablecimiento es: <strong>{WebUtility.HtmlEncode(resetCode)}</strong></p></body></html>");

            return SendEmailAsync(email, subject, body, isHtml: true);
        }

        public Task SendEmailVerification(string email, string verificationCode)
        {
            if (!_enableEmails)
            {
                _logger.LogWarning("SMTP disabled. Verification code for {Email}: {VerificationCode}", email, verificationCode);
            }

            const string subject = "Verificacion de cuenta";
            var body = _templateRenderer.Render(
                "verification-code.html",
                new Dictionary<string, string?>
                {
                    ["Subject"] = subject,
                    ["AppName"] = _appName,
                    ["RecipientEmail"] = email,
                    ["VerificationCode"] = verificationCode,
                    ["ActionUrl"] = BuildPortalUrl(_verificationPath, email),
                    ["SupportEmail"] = _supportEmail
                },
                $"<html><body><h2>{WebUtility.HtmlEncode(_appName)}</h2><p>Tu codigo de verificacion es: <strong>{WebUtility.HtmlEncode(verificationCode)}</strong></p></body></html>");

            return SendEmailAsync(email, subject, body, isHtml: true);
        }

        public Task SendCustomNotification(string email, string message)
        {
            const string subject = "Notificacion";
            var body = LooksLikeHtml(message)
                ? message
                : _templateRenderer.Render(
                    "notification-shell.html",
                    new Dictionary<string, string?>
                    {
                        ["Subject"] = subject,
                        ["AppName"] = _appName,
                        ["BodyHtml"] = $"<p>{WebUtility.HtmlEncode(message)}</p>",
                        ["SupportEmail"] = _supportEmail
                    },
                    $"<html><body><h2>{WebUtility.HtmlEncode(_appName)}</h2><p>{WebUtility.HtmlEncode(message)}</p></body></html>",
                    "BodyHtml");

            return SendEmailAsync(email, subject, body, isHtml: true);
        }

        private async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            if (!_enableEmails)
            {
                _logger.LogInformation("Email disabled. Destination: {Email}. Subject: {Subject}", to, subject);
                return;
            }

            try
            {
                var senderPassword = _senderPassword ?? throw new InvalidOperationException("Falta EmailSettings:SenderPassword en configuracion o variables de entorno.");

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_smtpServer, _smtpPort, _socketSecurity);
                await smtp.AuthenticateAsync(_smtpUser, senderPassword);

                var mail = new MimeMessage();
                mail.From.Add(new MailboxAddress(_senderName, _senderEmail));
                mail.To.Add(MailboxAddress.Parse(to));
                mail.Subject = subject;

                if (!string.IsNullOrWhiteSpace(_supportEmail) && !string.Equals(_supportEmail, _senderEmail, StringComparison.OrdinalIgnoreCase))
                {
                    mail.ReplyTo.Add(MailboxAddress.Parse(_supportEmail));
                }

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                mail.Body = bodyBuilder.ToMessageBody();

                await smtp.SendAsync(mail);
                await smtp.DisconnectAsync(true);
                _logger.LogInformation("Email sent to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", to);
                throw;
            }
        }

        private string BuildPortalUrl(string relativePath, string email)
        {
            if (string.IsNullOrWhiteSpace(_portalBaseUrl))
            {
                return string.Empty;
            }

            var baseUri = new Uri(new Uri(_portalBaseUrl.TrimEnd('/') + "/"), relativePath.TrimStart('/'));
            var separator = string.IsNullOrEmpty(baseUri.Query) ? "?" : "&";
            return $"{baseUri}{separator}email={Uri.EscapeDataString(email)}";
        }

        private static bool LooksLikeHtml(string value)
        {
            return value.Contains("<html", StringComparison.OrdinalIgnoreCase)
                || value.Contains("<body", StringComparison.OrdinalIgnoreCase)
                || value.Contains("<table", StringComparison.OrdinalIgnoreCase)
                || value.Contains("<div", StringComparison.OrdinalIgnoreCase)
                || value.Contains("<p", StringComparison.OrdinalIgnoreCase);
        }

        private SecureSocketOptions ResolveSocketSecurity(IConfiguration configuration)
        {
            var configuredValue = configuration["EmailSettings:SocketSecurity"]
                                  ?? Environment.GetEnvironmentVariable("EmailSettings__SocketSecurity");

            if (!string.IsNullOrWhiteSpace(configuredValue))
            {
                if (Enum.TryParse<SecureSocketOptions>(configuredValue, ignoreCase: true, out var socketSecurity))
                {
                    return socketSecurity;
                }

                throw new InvalidOperationException(
                    "EmailSettings:SocketSecurity no es valido. Valores soportados: Auto, None, SslOnConnect, StartTls, StartTlsWhenAvailable.");
            }

            return _enableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;
        }
    }
}
