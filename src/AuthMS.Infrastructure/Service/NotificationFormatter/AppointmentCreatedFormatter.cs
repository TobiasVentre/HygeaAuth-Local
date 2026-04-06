using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class AppointmentCreatedFormatter : INotificationFormatter
    {
        private readonly ILogger<AppointmentCreatedFormatter> _logger;

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AppointmentCreatedFormatter(ILogger<AppointmentCreatedFormatter> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.AppointmentCreated;

        public Task<string> FormatAsync(Notification n, User user)
        {
            _logger.LogInformation("Deserializando payload de AppointmentCreated...");

            var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido");

            _logger.LogInformation("Payload deserializado correctamente.");

            // ===========================================
            // 🕒 MANEJO SEGURO Y UNIVERSAL DEL HORARIO
            // ===========================================

            string appointmentTimeFormatted = "Horario no disponible";

            try
            {
                object rawTime = dto.AppointmentTime; // puede ser string, TimeSpan o null

                if (rawTime == null)
                {
                    _logger.LogWarning("AppointmentTime vino NULL.");
                }
                else
                {
                    // Si es TimeSpan
                    if (rawTime is TimeSpan ts)
                    {
                        appointmentTimeFormatted = ts.ToString(@"hh\:mm");
                        _logger.LogInformation("AppointmentTime detectado como TimeSpan: {0}", appointmentTimeFormatted);
                    }
                    // Si es string
                    else if (rawTime is string str)
                    {
                        if (TimeSpan.TryParse(str, out var parsed))
                        {
                            appointmentTimeFormatted = parsed.ToString(@"hh\:mm");
                            _logger.LogInformation("AppointmentTime detectado como string válido: {0}", appointmentTimeFormatted);
                        }
                        else
                        {
                            appointmentTimeFormatted = str; // fallback seguro
                            _logger.LogWarning("AppointmentTime string no es TimeSpan válido. Se usa literal: {0}", str);
                        }
                    }
                    else
                    {
                        appointmentTimeFormatted = rawTime.ToString();
                        _logger.LogWarning("AppointmentTime tipo desconocido. Se usa ToString(): {0}", appointmentTimeFormatted);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formateando AppointmentTime. Se utilizará 'Horario no disponible'.");
            }


            // ==========
            // HTML FINAL
            // ==========

            var html = $@"
            <html>
              <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                  <h2 style='color: #2c5aa0;'>🏥 CuidarMed+</h2>
                  
                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>
                  
                  <p>✅ Tu turno médico ha sido <strong>creado</strong> exitosamente.</p>
                  
                  <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Detalles del Turno</h3>
                    <p><strong>🆔 ID del Turno:</strong> {dto.AppointmentId}</p>
                    <p><strong>👨‍⚕️ Médico:</strong> {dto.DoctorName}</p>
                    <p><strong>🏥 Especialidad:</strong> {dto.Specialty}</p>
                    <p><strong>📅 Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {appointmentTimeFormatted} hs</p>
                    <p><strong>📍 Tipo:</strong> {(dto.AppointmentType == "Presencial" ? "Teleconsulta" : dto.AppointmentType)}</p>
                  </div>

                  {(dto.AppointmentType == "Virtual" && !string.IsNullOrEmpty(dto.MeetingLink) ?
                    $@"<div style='background-color: #e8f4fd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #2c5aa0; margin-top: 0;'>💻 Consulta Virtual</h3>
                        <p>🔗 <strong>Enlace de la videollamada:</strong></p>
                        <p><a href='{dto.MeetingLink}' style='color: #2c5aa0; text-decoration: none;'>{dto.MeetingLink}</a></p>
                        <p><em>Te recomendamos ingresar 5 minutos antes del horario programado.</em></p>
                      </div>" : "")}

                  {(dto.AppointmentType == "Presencial" ?
                    $@"<div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #856404; margin-top: 0;'>🏥 Consulta Presencial</h3>
                        <p>📍 <strong>Ubicación:</strong> Consultorio médico</p>
                        <p><em>Te recomendamos llegar 10 minutos antes del horario programado.</em></p>
                      </div>" : "")}

                  {(!string.IsNullOrEmpty(dto.Notes) ?
                    $@"<div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #2c5aa0; margin-top: 0;'>📝 Notas Adicionales</h3>
                        <p>{dto.Notes}</p>
                      </div>" : "")}

                  <p>¡Gracias por confiar en <strong>CuidarMed+</strong> para tu salud!</p>
                  
                  <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                  <p style='font-size: 12px; color: #666; text-align: center;'>
                    Este es un mensaje automático. Por favor, no respondas a este correo.
                  </p>
                </div>
              </body>
            </html>";

            return Task.FromResult(html);
        }
    }
}
