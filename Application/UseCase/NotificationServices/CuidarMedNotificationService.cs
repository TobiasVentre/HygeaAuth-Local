using Application.Dtos.Notification;
using Application.Interfaces.IRepositories;
using Domain.Entities;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.UseCase.NotificationServices
{
    /// <summary>
    /// Servicio para crear notificaciones específicas de CuidarMed+
    /// </summary>
    public class CuidarMedNotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public CuidarMedNotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        /// <summary>
        /// Crea una notificación cuando se crea un turno médico
        /// </summary>
        public async Task CreateAppointmentCreatedNotification(int userId, AppointmentPayload appointmentData)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.AppointmentCreated,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(appointmentData)
            };

            await _notificationRepository.Add(notification);
        }

        /// <summary>
        /// Crea una notificación de recordatorio de turno (24 horas antes)
        /// </summary>
        public async Task CreateAppointmentReminderNotification(int userId, AppointmentPayload appointmentData)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.AppointmentReminder,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(appointmentData)
            };

            await _notificationRepository.Add(notification);
        }

        /// <summary>
        /// Crea una notificación cuando una receta está lista
        /// </summary>
        public async Task CreatePrescriptionReadyNotification(int userId, PrescriptionPayload prescriptionData)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.PrescriptionReady,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(prescriptionData)
            };

            await _notificationRepository.Add(notification);
        }

        /// <summary>
        /// Crea una notificación cuando inicia una consulta virtual
        /// </summary>
        public async Task CreateConsultationStartedNotification(int userId, ConsultationPayload consultationData)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.ConsultationStarted,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(consultationData)
            };

            await _notificationRepository.Add(notification);
        }

        /// <summary>
        /// Crea una notificación de recordatorio de medicación
        /// </summary>
        public async Task CreateMedicationReminderNotification(int userId, MedicalReminderPayload reminderData)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.MedicationReminder,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(reminderData)
            };

            await _notificationRepository.Add(notification);
        }

        /// <summary>
        /// Crea una notificación cuando se cancela un turno
        /// </summary>
        public async Task CreateAppointmentCancelledNotification(int userId, AppointmentPayload appointmentData)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.AppointmentCancelled,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(appointmentData)
            };

            await _notificationRepository.Add(notification);
        }

        /// <summary>
        /// Crea una notificación cuando se reprograma un turno
        /// </summary>
        public async Task CreateAppointmentRescheduledNotification(int userId, AppointmentPayload appointmentData)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.AppointmentRescheduled,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(appointmentData)
            };

            await _notificationRepository.Add(notification);
        }
    }
}
