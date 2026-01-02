using Application.Interfaces.IServices.IAuthServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Application.UseCase.AuthServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;
        private readonly bool _enableEmails;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _config = configuration;
            _logger = logger;

            // ==========================================
            // 🔍 LOGS PARA VERIFICAR DE DÓNDE SALEN LOS DATOS
            // ==========================================

            _logger.LogWarning("🔍 CONFIG EmailSettings:SmtpServer = {v}", _config["EmailSettings:SmtpServer"]);
            _logger.LogWarning("🔍 CONFIG EmailSettings:SmtpPort = {v}", _config["EmailSettings:SmtpPort"]);
            _logger.LogWarning("🔍 CONFIG EmailSettings:SenderEmail = {v}", _config["EmailSettings:SenderEmail"]);
            _logger.LogWarning("🔍 CONFIG EmailSettings:SenderPassword = {v}", _config["EmailSettings:SenderPassword"]);
            _logger.LogWarning("🔍 CONFIG EmailSettings:EnableEmails = {v}", _config["EmailSettings:EnableEmails"]);

            _logger.LogWarning("🔍 ENV EmailSettings__SmtpServer = {v}", Environment.GetEnvironmentVariable("EmailSettings__SmtpServer"));
            _logger.LogWarning("🔍 ENV EmailSettings__SmtpPort = {v}", Environment.GetEnvironmentVariable("EmailSettings__SmtpPort"));
            _logger.LogWarning("🔍 ENV EmailSettings__SenderEmail = {v}", Environment.GetEnvironmentVariable("EmailSettings__SenderEmail"));
            _logger.LogWarning("🔍 ENV EmailSettings__SenderPassword = {v}", Environment.GetEnvironmentVariable("EmailSettings__SenderPassword"));
            _logger.LogWarning("🔍 ENV EmailSettings__EnableEmails = {v}", Environment.GetEnvironmentVariable("EmailSettings__EnableEmails"));

            // ==========================================
            // 🔧 ASIGNACIÓN DE CONFIGURACIONES
            // ==========================================

            _smtpServer = _config["EmailSettings:SmtpServer"]
                          ?? Environment.GetEnvironmentVariable("EmailSettings__SmtpServer")
                          ?? "smtp.gmail.com";

            _smtpPort = int.TryParse(
                _config["EmailSettings:SmtpPort"] ??
                Environment.GetEnvironmentVariable("EmailSettings__SmtpPort"),
                out var port) ? port : 587;

            _senderEmail = _config["EmailSettings:SenderEmail"]
                           ?? Environment.GetEnvironmentVariable("EmailSettings__SenderEmail")
                           ?? "cuidarmed.notificaciones@gmail.com";

            _senderPassword = _config["EmailSettings:SenderPassword"]
                              ?? Environment.GetEnvironmentVariable("EmailSettings__SenderPassword");

            _enableEmails = bool.TryParse(
                _config["EmailSettings:EnableEmails"] ??
                Environment.GetEnvironmentVariable("EmailSettings__EnableEmails"),
                out var enable) ? enable : true;

            _logger.LogWarning("🔍 FINAL EnableEmails = {Enable}", _enableEmails);
            _logger.LogWarning("🔍 FINAL SMTP Server = {Server}:{Port}", _smtpServer, _smtpPort);

            if (_enableEmails && string.IsNullOrEmpty(_senderPassword))
            {
                throw new Exception("Falta EmailSettings:SenderPassword en variables de entorno o configuración.");
            }
        }


        // ============================================================
        //  MÉTODOS PÚBLICOS
        // ============================================================

        public async Task SendPasswordResetEmail(string email, string resetCode)
        {
            await SendEmailAsync(email, "Restablecimiento de contraseña",
                $"Tu código de restablecimiento es: {resetCode}");
        }

        public async Task SendEmailVerification(string email, string verificationCode)
        {
            await SendEmailAsync(email, "Verificación de cuenta",
                $"Tu código de verificación es: {verificationCode}");
        }

        public async Task SendCustomNotification(string email, string message)
        {
            await SendEmailAsync(email, "Notificación", message, isHtml: true);
        }


        // ============================================================
        //  ENVÍO PRINCIPAL
        // ============================================================

        private async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            _logger.LogWarning("📧 Preparando envío de email a {Email} (EnableEmails={Enable})",
                to, _enableEmails);

            // ==========================
            // MODO DEV → NO ENVÍA EMAIL
            // ==========================

            if (!_enableEmails)
            {
                _logger.LogWarning("📧 [DEV MODE] Email NO ENVIADO. Solo se imprime en logs.");
                _logger.LogInformation("📧 DESTINATARIO: {Email}", to);
                _logger.LogInformation("📧 ASUNTO: {Subject}", subject);

                string preview = body.Length > 300 ? body.Substring(0, 300) + "..." : body;
                _logger.LogInformation("📧 CUERPO (300 chars): {Body}", preview);
                return;
            }

            // ==========================
            // ENVÍO REAL
            // ==========================

            try
            {
                _logger.LogWarning("📧 Enviando email REAL vía SMTP {Server}:{Port}",
                    _smtpServer, _smtpPort);

                using var smtp = new SmtpClient(_smtpServer, _smtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var mail = new MailMessage(_senderEmail, to, subject, body)
                {
                    IsBodyHtml = isHtml
                };

                await smtp.SendMailAsync(mail);

                _logger.LogWarning("📧 EMAIL ENVIADO ✔ a {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR enviando email a {Email}", to);
                throw;
            }
        }
    }
}