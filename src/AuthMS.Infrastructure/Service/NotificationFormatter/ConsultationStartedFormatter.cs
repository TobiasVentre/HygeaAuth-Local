using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class ConsultationStartedFormatter : INotificationFormatter
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.ConsultationStarted;

        public Task<string> FormatAsync(Notification n, User user)
        {
            var dto = JsonSerializer.Deserialize<ConsultationPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido");

            var html = $@"
            <html>
              <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                  <h2 style='color: #2c5aa0;'>🏥 Hygea</h2>
                  
                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>
                  
                  <div style='background-color: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;'>
                    <h3 style='color: #155724; margin-top: 0;'>🎥 Consulta Iniciada</h3>
                    <p style='margin: 0; color: #155724;'><strong>Tu consulta médica ha comenzado. ¡Únete ahora!</strong></p>
                  </div>
                  
                  <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Detalles de la Consulta</h3>
                    <p><strong>🆔 ID de Consulta:</strong> {dto.ConsultationId}</p>
                    <p><strong>👨‍⚕️ Médico:</strong> {dto.DoctorName}</p>
                    <p><strong>🏥 Especialidad:</strong> {dto.Specialty}</p>
                    <p><strong>📅 Fecha:</strong> {dto.ConsultationDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {dto.ConsultationTime:hh\\:mm} hs</p>
                    <p><strong>📊 Estado:</strong> {dto.Status}</p>
                  </div>

                  <div style='background-color: #e8f4fd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0; margin-top: 0;'>💻 Unirse a la Videollamada</h3>
                    <p>🔗 <strong>Enlace de la consulta:</strong></p>
                    <p><a href='{dto.MeetingLink}' style='color: #2c5aa0; text-decoration: none; font-weight: bold; background-color: #f8f9fa; padding: 10px 15px; border-radius: 5px; display: inline-block;'>{dto.MeetingLink}</a></p>
                    <p><em>💡 Haz clic en el enlace para unirte a tu consulta médica.</em></p>
                  </div>

                  <div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #856404; margin-top: 0;'>📱 Recomendaciones Técnicas</h3>
                    <ul>
                      <li>Usa auriculares para mejor calidad de audio</li>
                      <li>Asegúrate de tener buena iluminación</li>
                      <li>Cierra otras aplicaciones para mejor rendimiento</li>
                      <li>Ten tu DNI y estudios médicos a mano</li>
                    </ul>
                  </div>

                  <div style='background-color: #d1ecf1; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; color: #0c5460;'><strong>💡 Importante:</strong> La consulta puede durar entre 15-30 minutos. Prepárate para una conversación detallada sobre tu salud.</p>
                  </div>

                  <p>¡Que tengas una excelente consulta en <strong>Hygea</strong>!</p>
                  
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
