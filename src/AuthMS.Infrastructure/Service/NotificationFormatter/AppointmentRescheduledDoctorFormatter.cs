using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class AppointmentRescheduledDoctorFormatter : INotificationFormatter
{
    private readonly ILogger<AppointmentRescheduledDoctorFormatter> _logger;

    private static readonly JsonSerializerOptions _opts = new()
    { PropertyNameCaseInsensitive = true };

    public AppointmentRescheduledDoctorFormatter(
        ILogger<AppointmentRescheduledDoctorFormatter> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(NotificationType type) =>
        type == NotificationType.AppointmentRescheduledDoctor;

    public Task<string> FormatAsync(Notification n, User user)
    {
        _logger.LogInformation("Deserializando payload AppointmentRescheduledDoctor...");

        var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                 ?? throw new InvalidOperationException("Payload inválido AppointmentRescheduledDoctor");

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

              <h2 style='color: #2c5aa0;'>🔄 CuidarMed+ — Reprogramación Exitosa</h2>

              <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

              <p>Has reprogramado correctamente el turno de tu paciente 
              <strong>{dto.PatientName}</strong>.</p>

              <div style='background-color: #f8f9fa;
                          padding: 15px;
                          border-radius: 8px;
                          margin: 20px 0;'>
                <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Detalles Actualizados</h3>

                <p><strong>👤 Paciente:</strong> {dto.PatientName}</p>
                <p><strong>📅 Nueva fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                <p><strong>🕐 Nuevo horario:</strong> {formattedTime} hs</p>
                <p><strong>📌 Especialidad:</strong> {dto.Specialty}</p>
                <p><strong>Motivo:</strong> {dto.Notes}</p>
              </div>

              <p>El paciente ha sido notificado automáticamente.</p>

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
