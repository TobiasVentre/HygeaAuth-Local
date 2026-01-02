using Application.Dtos.Request;
using Application.Interfaces.IQuery;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.UseCase.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IUserQuery _userQuery;
        private readonly IEmailService _emailService;

        public NotificationService(
            INotificationRepository repo,
            IUserQuery userQuery,
            IEmailService emailService)
        {
            _repo = repo;
            _userQuery = userQuery;
            _emailService = emailService;
        }


        public async Task EnqueueEvent(NotificationEventRequest request)
        {
            Console.WriteLine("📥 [DEBUG] NotificationEventRequest recibido:");
            Console.WriteLine($"     UserId: {request.UserId}");
            Console.WriteLine($"     EventType RAW: '{request.EventType}'");
            Console.WriteLine($"     Payload: {JsonSerializer.Serialize(request.Payload)}");

            if (!Enum.TryParse<NotificationType>(request.EventType, out var type))
            {
                Console.WriteLine("❌ [ERROR] NO se pudo convertir EventType → NotificationType");
                throw new InvalidOperationException($"Tipo de evento '{request.EventType}' inválido.");
            }

            Console.WriteLine($"✔ [DEBUG] Enum parseado correctamente → {type}");

            var notif = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = request.UserId,
                Type = type,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(request.Payload)
            };

            Console.WriteLine("📝 [DEBUG] Notificación persistida con Type: " + notif.Type);

            await _repo.Add(notif);
        }

    }
}
