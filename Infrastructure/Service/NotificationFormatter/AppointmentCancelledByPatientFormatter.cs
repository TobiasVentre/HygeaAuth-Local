using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class AppointmentCancelledByPatientFormatter : INotificationFormatter
    {
        private readonly ILogger<AppointmentCancelledByPatientFormatter> _logger;

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AppointmentCancelledByPatientFormatter(ILogger<AppointmentCancelledByPatientFormatter> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.AppointmentCancelledByPatient;

        public Task<string> FormatAsync(Notification n, User user)
        {
            _logger.LogInformation("Deserializando payload AppointmentCancelledByPatient...");

            var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido AppointmentCancelledByPatient");

            string formattedTime = dto.AppointmentTime.ToString(@"hh\:mm");

            var html = $@"
            <html>
              <body style='font-family: Arial; color: #333;'>
                <div style='max-width: 600px; margin: auto; padding: 20px;'>

                  <h2 style='color: #2c5aa0;'>🗑️ CuidarMed+ — Cancelación Confirmada</h2>

                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

                  <p>Tu turno ha sido cancelado correctamente.</p>

                  <div style='background: #eef5ff; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0;'>📋 Detalles del Turno Cancelado</h3>

                    <p><strong>🆔 ID:</strong> {dto.AppointmentId}</p>
                    <p><strong>👨‍⚕️ Médico:</strong> {dto.DoctorName}</p>
                    <p><strong>📅 Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {formattedTime} hs</p>
                    <p><strong>Especialidad:</strong> {dto.Specialty}</p>
                  </div>

                  <p>Esperamos verte pronto nuevamente.</p>

                  <hr />
                  <p style='font-size: 12px; text-align: center; color: #666;'>Correo automático — No responder.</p>
                </div>
              </body>
            </html>";

            return Task.FromResult(html);
        }
    }
}
