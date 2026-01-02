using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class PrescriptionReadyFormatter : INotificationFormatter
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.PrescriptionReady;

        public Task<string> FormatAsync(Notification n, User user)
        {
            var dto = JsonSerializer.Deserialize<PrescriptionPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido");

            var html = $@"
            <html>
              <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                  <h2 style='color: #2c5aa0;'>🏥 CuidarMed+</h2>
                  
                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>
                  
                  <div style='background-color: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;'>
                    <h3 style='color: #155724; margin-top: 0;'>✅ Receta Médica Lista</h3>
                    <p style='margin: 0; color: #155724;'><strong>Tu receta médica está disponible para descargar.</strong></p>
                  </div>
                  
                  <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Detalles de la Receta</h3>
                    <p><strong>🆔 ID de Receta:</strong> {dto.PrescriptionId}</p>
                    <p><strong>📄 Número:</strong> {dto.PrescriptionNumber}</p>
                    <p><strong>👨‍⚕️ Médico:</strong> {dto.DoctorName}</p>
                    <p><strong>🏥 Especialidad:</strong> {dto.Specialty}</p>
                    <p><strong>📅 Fecha:</strong> {dto.PrescriptionDate:dd/MM/yyyy}</p>
                  </div>

                  <div style='background-color: #e8f4fd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0; margin-top: 0;'>📥 Descargar Receta</h3>
                    <p>🔗 <strong>Enlace de descarga:</strong></p>
                    <p><a href='{dto.DownloadUrl}' style='color: #2c5aa0; text-decoration: none; font-weight: bold; background-color: #f8f9fa; padding: 10px 15px; border-radius: 5px; display: inline-block;'>{dto.DownloadUrl}</a></p>
                    <p><em>💡 Haz clic en el enlace para descargar tu receta en formato PDF.</em></p>
                  </div>

                  {(!string.IsNullOrEmpty(dto.Notes) ? 
                    $@"<div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #856404; margin-top: 0;'>📝 Instrucciones del Médico</h3>
                        <p>{dto.Notes}</p>
                      </div>" : "")}

                  <div style='background-color: #f8d7da; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; color: #721c24;'><strong>⚠️ Importante:</strong> Esta receta tiene validez legal. Preséntala en tu farmacia de confianza.</p>
                  </div>

                  <div style='background-color: #d1ecf1; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; color: #0c5460;'><strong>💡 Consejo:</strong> Guarda una copia digital de tu receta en un lugar seguro.</p>
                  </div>

                  <p>¡Gracias por usar <strong>CuidarMed+</strong>!</p>
                  
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
