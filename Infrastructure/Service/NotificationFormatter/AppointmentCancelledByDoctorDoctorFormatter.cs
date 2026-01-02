using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class AppointmentCancelledByDoctorDoctorFormatter : INotificationFormatter
    {
        private readonly ILogger<AppointmentCancelledByDoctorDoctorFormatter> _logger;

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AppointmentCancelledByDoctorDoctorFormatter(
            ILogger<AppointmentCancelledByDoctorDoctorFormatter> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.AppointmentCancelledByDoctorDoctor;

        public Task<string> FormatAsync(Notification n, User user)
        {
            _logger.LogInformation("Deserializando payload AppointmentCancelledByDoctorDoctor...");

            var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido AppointmentCancelledByDoctorDoctor");

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
                : $"<p><strong>Motivo informado:</strong> {dto.Notes}</p>";

            var html = $@"
            <html>
              <body style='font-family: Arial; color: #333;'>
                <div style='max-width: 600px; margin: auto; padding: 20px;'>

                  <h2 style='color: #2c5aa0;'>📄 CuidarMed+ — Confirmación de Cancelación</h2>

                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

                  <p>La cancelación del turno ha sido registrada correctamente.</p>

                  <div style='background: #f2f4f7; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0;'>📋 Turno Cancelado</h3>

                    <p><strong>🆔 ID del Turno:</strong> {dto.AppointmentId}</p>
                    <p><strong>👤 Paciente:</strong> {dto.PatientName}</p>
                    <p><strong>📅 Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {formattedTime} hs</p>
                    <p><strong>Especialidad:</strong> {dto.Specialty}</p>

                    {reasonHtml}
                  </div>

                  <p>El paciente ha sido notificado automáticamente.</p>

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
