using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class AppointmentConfirmedDoctorFormatter : INotificationFormatter
{
    private readonly ILogger<AppointmentConfirmedDoctorFormatter> _logger;

    private static readonly JsonSerializerOptions _opts = new()
    { PropertyNameCaseInsensitive = true };

    public AppointmentConfirmedDoctorFormatter(
        ILogger<AppointmentConfirmedDoctorFormatter> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(NotificationType type) =>
        type == NotificationType.AppointmentConfirmedDoctor;

    public Task<string> FormatAsync(Notification n, User user)
    {
        _logger.LogInformation("Deserializando payload AppointmentConfirmedDoctor...");

        AppointmentPayload? dto = null;

        try
        {
            dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deserializando payload en AppointmentConfirmedDoctorFormatter. Payload recibido: {Payload}", n.Payload);
        }

        if (dto == null)
        {
            _logger.LogWarning("⚠ Payload inválido para AppointmentConfirmedDoctor. Se devuelve mensaje genérico.");

            return Task.FromResult(@"
        <html><body>
        <h3>Notificación inválida</h3>
        <p>No se pudo leer la información del turno.</p>
        </body></html>
    ");
        }

        string formattedTime = "";

        // 1) Si viene un TimeSpan válido
        if (TimeSpan.TryParse(dto.AppointmentTime.ToString(), out var ts))
        {
            formattedTime = $"{ts.Hours:D2}:{ts.Minutes:D2}";
        }
        // 2) Si viene un DateTime (muy común en reschedule y confirmaciones)
        else if (DateTime.TryParse(dto.AppointmentTime.ToString(), out var dt))
        {
            formattedTime = dt.ToString("HH:mm");
        }
        // 3) Si viene como "15:30" con formato raro
        else if (!string.IsNullOrWhiteSpace(dto.AppointmentTime.ToString()))
        {
            try
            {
                var cleaned = dto.AppointmentTime.ToString().Substring(0, 5); // "HH:mm"
                formattedTime = cleaned;
            }
            catch
            {
                formattedTime = "—";
            }
        }
        else
        {
            formattedTime = "—";
        }


        var html = $@"
        <html>
            <body style='font-family: Arial; color:#333;'>
                <div style='max-width:600px;margin:auto;padding:20px;'>

                    <h2 style='color:#2c5aa0;'>✔️ Confirmación del Turno</h2>

                    <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>

                    <p>Has confirmado correctamente el turno del paciente
                    <strong>{dto.PatientName}</strong>.</p>

                    <div style='background:#eef5ff;padding:15px;border-radius:8px;margin:20px 0;'>
                        <p><strong>Paciente:</strong> {dto.PatientName}</p>
                        <p><strong>Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                        <p><strong>Hora:</strong> {formattedTime} hs</p>
                        <p><strong>Especialidad:</strong> {dto.Specialty}</p>
                    </div>

                    <p>El paciente fue notificado automáticamente.</p>

                </div>
            </body>
        </html>";

        return Task.FromResult(html);
    }
}
