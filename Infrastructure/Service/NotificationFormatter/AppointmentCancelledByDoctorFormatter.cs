using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class AppointmentCancelledByDoctorFormatter : INotificationFormatter
    {
        private readonly ILogger<AppointmentCancelledByDoctorFormatter> _logger;

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AppointmentCancelledByDoctorFormatter(ILogger<AppointmentCancelledByDoctorFormatter> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.AppointmentCancelledByDoctor;

        public Task<string> FormatAsync(Notification n, User user)
        {
            _logger.LogInformation("Deserializando payload AppointmentCancelledByDoctor...");

            var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido AppointmentCancelledByDoctor");

            string formattedTime;

            if (TimeSpan.TryParse(dto.AppointmentTime.ToString(), out var ts))
            {
                formattedTime = ts.ToString(@"hh\:mm");
            }
            else
            {
                // Si viene "15:30" o algo no TimeSpan, lo dejamos como está
                formattedTime = dto.AppointmentTime.ToString() ?? "";
            }


            var reasonHtml = string.IsNullOrWhiteSpace(dto.Notes)
                ? ""
                : $"<p><strong>Motivo de la cancelación:</strong> {dto.Notes}</p>";

            var html = $@"
            <html>
              <body style='font-family: Arial; color: #333;'>
                <div style='max-width: 600px; margin: auto; padding: 20px;'>

                  <h2 style='color: #c0392b;'>⚠️ CuidarMed+ — Tu turno ha sido cancelado</h2>

                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

                  <p>El médico ha cancelado tu turno. A continuación encontrarás los detalles:</p>

                  <div style='background: #fdecea; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #c0392b;'>📋 Detalles del Turno Cancelado</h3>

                    <p><strong>🆔 ID del Turno:</strong> {dto.AppointmentId}</p>
                    <p><strong>👨‍⚕️ Médico:</strong> {dto.DoctorName}</p>
                    <p><strong>📅 Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {formattedTime} hs</p>
                    <p><strong>Especialidad:</strong> {dto.Specialty}</p>

                    {reasonHtml}
                  </div>

                  <p>Puedes reservar un nuevo turno en cualquier momento desde nuestra aplicación.</p>

                  <hr />
                  <p style='font-size: 12px; text-align: center; color: #666;'>
                    Correo automático — No responder.
                  </p>
                </div>
              </body>
            </html>";

            return Task.FromResult(html);
        }
    }
}
