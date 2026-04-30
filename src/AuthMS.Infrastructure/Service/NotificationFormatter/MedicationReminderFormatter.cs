using Application.Dtos.Notification;
using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class MedicationReminderFormatter : INotificationFormatter
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.MedicationReminder;

        public Task<string> FormatAsync(Notification n, User user)
        {
            var dto = JsonSerializer.Deserialize<MedicalReminderPayload>(n.Payload!, _opts)
                      ?? throw new InvalidOperationException("Payload inválido");

            var html = $@"
            <html>
              <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                  <h2 style='color: #2c5aa0;'>🏥 Hygea</h2>
                  
                  <p>Hola <strong>{user.FirstName} {user.LastName}</strong>,</p>
                  
                  <div style='background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                    <h3 style='color: #856404; margin-top: 0;'>💊 Recordatorio de Medicación</h3>
                    <p style='margin: 0; color: #856404;'><strong>Es hora de tomar tu medicación.</strong></p>
                  </div>
                  
                  <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='color: #2c5aa0; margin-top: 0;'>📋 Detalles de la Medicación</h3>
                    <p><strong>💊 Medicamento:</strong> {dto.MedicationName}</p>
                    <p><strong>📏 Dosis:</strong> {dto.Dosage}</p>
                    <p><strong>📅 Fecha:</strong> {dto.ReminderDate:dd/MM/yyyy}</p>
                    <p><strong>🕐 Hora:</strong> {dto.ReminderTime:hh\\:mm} hs</p>
                    <p><strong>📝 Tipo:</strong> {dto.ReminderType}</p>
                  </div>

                  {(!string.IsNullOrEmpty(dto.Instructions) ? 
                    $@"<div style='background-color: #e8f4fd; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #2c5aa0; margin-top: 0;'>📝 Instrucciones</h3>
                        <p>{dto.Instructions}</p>
                      </div>" : "")}

                  <div style='background-color: #f8d7da; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; color: #721c24;'><strong>⚠️ Importante:</strong> No olvides tomar tu medicación según las indicaciones de tu médico.</p>
                  </div>

                  <div style='background-color: #d1ecf1; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; color: #0c5460;'><strong>💡 Consejo:</strong> Mantén un registro de tus tomas para un mejor seguimiento del tratamiento.</p>
                  </div>

                  <p>¡Cuida tu salud con <strong>Hygea</strong>!</p>
                  
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
