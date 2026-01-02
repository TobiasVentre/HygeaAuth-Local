using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class AppointmentCancelledByPatientDoctorFormatter : INotificationFormatter
    {
        private readonly ILogger<AppointmentCancelledByPatientDoctorFormatter> _logger;

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AppointmentCancelledByPatientDoctorFormatter(ILogger<AppointmentCancelledByPatientDoctorFormatter> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.AppointmentCancelledByPatientDoctor;

        public Task<string> FormatAsync(Notification n, User user)
        {
            _logger.LogInformation("Deserializando payload AppointmentCancelledByPatientDoctor...");

            var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido AppointmentCancelledByPatientDoctor");

            string formattedTime = dto.AppointmentTime.ToString(@"hh\:mm");

            var html = $@"
            <html>
              <body style='font-family: Arial; color: #333;'>
                <div style='max-width: 600px; margin: auto; padding: 20px;'>

                  <h2 style='color: #c0392b;'>❌ CuidarMed+ — Turno Cancelado por el Paciente</h2>

                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

                  <p>El paciente <strong>{dto.PatientName}</strong> ha cancelado su turno.</p>

                  <div style='background: #fcebea; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #c0392b;'>📝 Detalles del Turno Cancelado</h3>

                    <p><strong>🆔 ID:</strong> {dto.AppointmentId}</p>
                    <p><strong>👤 Paciente:</strong> {dto.PatientName}</p>
                    <p><strong>📅 Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {formattedTime} hs</p>
                    <p><strong>Especialidad:</strong> {dto.Specialty}</p>
                    {(string.IsNullOrWhiteSpace(dto.Notes) ? "" : $"<p><strong>Notas:</strong> {dto.Notes}</p>")}
                  </div>

                  <p>Ya puedes volver a ofrecer el horario a otros pacientes.</p>

                  <hr />
                  <p style='font-size: 12px; text-align: center; color: #666;'>Correo automático — No responder.</p>
                </div>
              </body>
            </html>";

            return Task.FromResult(html);
        }
    }
}
