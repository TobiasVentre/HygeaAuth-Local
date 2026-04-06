using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class AppointmentRescheduledFormatter : INotificationFormatter
{
    private readonly ILogger<AppointmentRescheduledFormatter> _logger;

    private static readonly JsonSerializerOptions _opts = new()
    { PropertyNameCaseInsensitive = true };

    public AppointmentRescheduledFormatter(
        ILogger<AppointmentRescheduledFormatter> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(NotificationType type) =>
        type == NotificationType.AppointmentRescheduled;

    public Task<string> FormatAsync(Notification n, User user)
    {
        _logger.LogInformation("Deserializando payload AppointmentRescheduled...");

        var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                 ?? throw new InvalidOperationException("Payload inválido AppointmentRescheduled");

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
        var html = $@"
        <html>
          <body style='font-family: Arial, sans-serif; color: #333;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>

              <h2 style='color: #2c5aa0;'>🔄 CuidarMed+ — Turno Reprogramado</h2>

              <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

              <p>Tu turno ha sido reprogramado exitosamente.</p>

              <div style='background-color: #f8f9fa;
                          padding: 15px;
                          border-radius: 8px;
                          margin: 20px 0;'>
                <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Nueva Información del Turno</h3>

                <p><strong>👨‍⚕️ Médico:</strong> {dto.DoctorName}</p>
                <p><strong>📅 Nueva fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                <p><strong>🕐 Nuevo horario:</strong> {formattedTime} hs</p>
                <p><strong>📌 Especialidad:</strong> {dto.Specialty}</p>
                <p><strong>Motivo:</strong> {dto.Notes}</p>
              </div>

              <p>Recordá asistir con unos minutos de anticipación.</p>

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
