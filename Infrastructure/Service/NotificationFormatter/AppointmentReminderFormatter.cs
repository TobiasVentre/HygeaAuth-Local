using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class AppointmentReminderFormatter : INotificationFormatter
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.AppointmentReminder;

        public Task<string> FormatAsync(Notification n, User user)
        {
            var dto = JsonSerializer.Deserialize<AppointmentPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido");

            var html = $@"
            <html>
              <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                  <h2 style='color: #2c5aa0;'>🏥 CuidarMed+</h2>
                  
                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>
                  
                  <div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                    <h3 style='color: #856404; margin-top: 0;'>⏰ Recordatorio de Turno</h3>
                    <p style='margin: 0; color: #856404;'><strong>Tu turno médico es mañana. ¡No lo olvides!</strong></p>
                  </div>
                  
                  <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Detalles del Turno</h3>
                    <p><strong>🆔 ID del Turno:</strong> {dto.AppointmentId}</p>
                    <p><strong>👨‍⚕️ Médico:</strong> {dto.DoctorName}</p>
                    <p><strong>🏥 Especialidad:</strong> {dto.Specialty}</p>
                    <p><strong>📅 Fecha:</strong> {dto.AppointmentDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {dto.AppointmentTime:hh\\:mm} hs</p>
                    <p><strong>📍 Tipo:</strong> {dto.AppointmentType}</p>
                  </div>

                  {(dto.AppointmentType == "Virtual" && !string.IsNullOrEmpty(dto.MeetingLink) ? 
                    $@"<div style='background-color: #e8f4fd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #2c5aa0; margin-top: 0;'>💻 Consulta Virtual</h3>
                        <p>🔗 <strong>Enlace de la videollamada:</strong></p>
                        <p><a href='{dto.MeetingLink}' style='color: #2c5aa0; text-decoration: none; font-weight: bold;'>{dto.MeetingLink}</a></p>
                        <p><em>📱 Te recomendamos:</em></p>
                        <ul>
                          <li>Probar tu conexión a internet</li>
                          <li>Verificar que tu cámara y micrófono funcionen</li>
                          <li>Ingresar 5 minutos antes del horario programado</li>
                        </ul>
                      </div>" : "")}

                  {(dto.AppointmentType == "Presencial" ? 
                    $@"<div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #856404; margin-top: 0;'>🏥 Consulta Presencial</h3>
                        <p>📍 <strong>Ubicación:</strong> Consultorio médico</p>
                        <p><em>📱 Te recomendamos:</em></p>
                        <ul>
                          <li>Llegar 10 minutos antes del horario programado</li>
                          <li>Traer tu DNI y obra social</li>
                          <li>Llevar estudios médicos previos si los tienes</li>
                        </ul>
                      </div>" : "")}

                  <div style='background-color: #d1ecf1; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; color: #0c5460;'><strong>💡 Importante:</strong> Si necesitas cancelar o reprogramar tu turno, hazlo con al menos 2 horas de anticipación.</p>
                  </div>

                  <p>¡Te esperamos en <strong>CuidarMed+</strong>!</p>
                  
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
