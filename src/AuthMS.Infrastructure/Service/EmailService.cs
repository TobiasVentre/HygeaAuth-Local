using Application.Interfaces.IServices.IAuthServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Service
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string? _senderPassword;
        private readonly bool _enableEmails;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;

            _smtpServer = configuration["EmailSettings:SmtpServer"]
                          ?? Environment.GetEnvironmentVariable("EmailSettings__SmtpServer")
                          ?? "smtp.gmail.com";

            _smtpPort = int.TryParse(
                configuration["EmailSettings:SmtpPort"] ?? Environment.GetEnvironmentVariable("EmailSettings__SmtpPort"),
                out var smtpPort)
                ? smtpPort
                : 587;

            _senderEmail = configuration["EmailSettings:SenderEmail"]
                           ?? Environment.GetEnvironmentVariable("EmailSettings__SenderEmail")
                           ?? "cuidarmed.notificaciones@gmail.com";

            _senderPassword = configuration["EmailSettings:SenderPassword"]
                              ?? Environment.GetEnvironmentVariable("EmailSettings__SenderPassword");

            _enableEmails = bool.TryParse(
                configuration["EmailSettings:EnableEmails"] ?? Environment.GetEnvironmentVariable("EmailSettings__EnableEmails"),
                out var enableEmails)
                ? enableEmails
                : true;

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

            return SendEmailAsync(email, "Restablecimiento de contraseña", $"Tu código de restablecimiento es: {resetCode}");
        }

        public Task SendEmailVerification(string email, string verificationCode)
        {
            if (!_enableEmails)
            {
                _logger.LogWarning("SMTP disabled. Verification code for {Email}: {VerificationCode}", email, verificationCode);
            }

            return SendEmailAsync(email, "Verificación de cuenta", $"Tu código de verificación es: {verificationCode}");
        }

        public Task SendCustomNotification(string email, string message)
            => SendEmailAsync(email, "Notificación", message, isHtml: true);

        private async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            if (!_enableEmails)
            {
                _logger.LogInformation("Email disabled. Destination: {Email}. Subject: {Subject}", to, subject);
                return;
            }

            try
            {
                using var smtp = new SmtpClient(_smtpServer, _smtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                using var mail = new MailMessage(_senderEmail, to, subject, body)
                {
                    IsBodyHtml = isHtml
                };

                await smtp.SendMailAsync(mail);
                _logger.LogInformation("Email sent to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", to);
                throw;
            }
        }
    }
}
