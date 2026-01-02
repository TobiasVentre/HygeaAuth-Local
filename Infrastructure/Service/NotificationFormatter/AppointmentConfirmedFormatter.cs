using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class AppointmentConfirmedFormatter : INotificationFormatter
{
    private readonly ILogger<AppointmentConfirmedFormatter> _logger;

    private static readonly JsonSerializerOptions _opts = new()
    { PropertyNameCaseInsensitive = true };

    public AppointmentConfirmedFormatter(
        ILogger<AppointmentConfirmedFormatter> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(NotificationType type) =>
        type == NotificationType.AppointmentConfirmed;

    public Task<string> FormatAsync(Notification n, User user)
    {
        _logger.LogInformation("Deserializando payload AppointmentConfirmed...");

        var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                 ?? throw new InvalidOperationException("Payload inválido AppointmentConfirmed");

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

              <h2 style='color: #2c5aa0;'>✔️ CuidarMed+ — Turno Confirmado</h2>

              <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

              <p>Tu turno ha sido <strong>confirmado exitosamente</strong>. Aquí tienes los detalles:</p>

              <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Detalles del Turno</h3>

                <p><strong>🧑‍⚕️ Médico:</strong> {dto.DoctorName}</p>
                <p><strong>📅 Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                <p><strong>🕐 Hora:</strong> {formattedTime} hs</p>
                <p><strong>Especialidad:</strong> {dto.Specialty}</p>
              </div>

              <p>Te esperamos en la fecha acordada.</p>

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
