using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service.NotificationFormatter
{
    public class DefaultNotificationFormatter : INotificationFormatter
    {
        private readonly Dictionary<NotificationType, string> _templates
          = new()
        {
            // Turnos médicos
            { NotificationType.AppointmentConfirmed, "Tu turno médico ha sido confirmado." },
            { NotificationType.AppointmentCancelled, "Tu turno médico ha sido cancelado." },
            { NotificationType.AppointmentRescheduled, "Tu turno médico ha sido reprogramado." },
            { NotificationType.AppointmentStartingSoon, "Tu turno médico comenzará en breve." },
            
            // Consultas médicas
            { NotificationType.ConsultationEnded, "Tu consulta médica ha finalizado." },
            { NotificationType.ConsultationCancelled, "Tu consulta médica ha sido cancelada." },
            
            // Recetas y documentos
            { NotificationType.MedicalOrderReady, "Tu orden médica está lista para descargar." },
            { NotificationType.DocumentGenerated, "Tu documento médico está disponible." },
            
            // Recordatorios médicos
            { NotificationType.FollowUpReminder, "Recordatorio: tienes una cita de seguimiento programada." },
            { NotificationType.TestResultsReady, "Tus resultados de laboratorio están disponibles." },
            
            // Sistema general
            { NotificationType.AccountActivated, "Tu cuenta ha sido activada exitosamente." },
            { NotificationType.PasswordReset, "Tu contraseña ha sido restablecida." },
            { NotificationType.EmailVerification, "Verifica tu dirección de email." },
            
            // Genéricos
            { NotificationType.Custom, "Tienes una nueva notificación." }
        };

        public bool CanHandle(NotificationType type) => true; // capturamos TODO lo demás

        public Task<string> FormatAsync(Notification n, User user)
        {
            if (string.IsNullOrWhiteSpace(n.Payload) || n.Payload == "null")
            {
                if (_templates.TryGetValue(n.Type, out var tpl))
                    return Task.FromResult(tpl);
                return Task.FromResult("Tienes una nueva notificación.");
            }

            // si viene payload “custom”
            return Task.FromResult(n.Payload!);
        }
    }
}
