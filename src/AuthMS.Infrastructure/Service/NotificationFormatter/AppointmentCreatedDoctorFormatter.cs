using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class AppointmentCreatedDoctorFormatter : INotificationFormatter
    {
        private readonly ILogger<AppointmentCreatedDoctorFormatter> _logger;

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AppointmentCreatedDoctorFormatter(ILogger<AppointmentCreatedDoctorFormatter> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.AppointmentCreatedDoctor;

        public Task<string> FormatAsync(Notification n, User user)
        {
            _logger.LogInformation("Deserializando payload de AppointmentCreatedDoctor...");

            var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido para AppointmentCreatedDoctor");

            // Convertir AppointmentTime a un formato visible
            string appointmentTimeFormatted = dto.AppointmentTime.ToString(@"hh\:mm");

            var html = $@"
            <html>
              <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                  <h2 style='color: #2c5aa0;'>🏥 CuidarMed+ — Nuevo Turno Asignado</h2>

                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

                  <p>📅 Un paciente ha reservado un nuevo turno y requiere tu confirmación.</p>

                  <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Detalles del Turno</h3>

                    <p><strong>🆔 ID del Turno:</strong> {dto.AppointmentId}</p>
                    <p><strong>👤 Paciente:</strong> {dto.PatientName}</p>
                    <p><strong>📅 Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {appointmentTimeFormatted} hs</p>
                    <p><strong>Tipo:</strong> {dto.AppointmentType}</p>
                    {(dto.AppointmentType == "Virtual" ? $"<p><strong>Link:</strong> {dto.MeetingLink}</p>" : "")}
                    {(string.IsNullOrWhiteSpace(dto.Notes) ? "" : $"<p><strong>Notas:</strong> {dto.Notes}</p>")}
                  </div>

                  <p>Por favor ingresa al sistema para confirmar o rechazar el turno.</p>

                  <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                  <p style='font-size: 12px; color: #666; text-align: center;'>
                    Este es un mensaje automático. No respondas a este correo.
                  </p>
                </div>
              </body>
            </html>";

            return Task.FromResult(html);
        }
    }
}
