using Application.Dtos.Request;
using Application.Interfaces.IQuery;
using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository repo,
            IUserQuery userQuery,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _repo = repo;
            _userQuery = userQuery;
            _emailService = emailService;
            _logger = logger;
        }


        public async Task EnqueueEvent(NotificationEventRequest request)
        {
            _logger.LogInformation(
                "NotificationEvent received. UserId={UserId}, EventType={EventType}, Payload={Payload}",
                request.UserId,
                request.EventType,
                JsonSerializer.Serialize(request.Payload));

            if (!Enum.TryParse<NotificationType>(request.EventType, out var type))
            {
                _logger.LogWarning("Invalid notification EventType value: {EventType}", request.EventType);
                throw new InvalidOperationException($"Tipo de evento '{request.EventType}' inválido.");
            }

            _logger.LogInformation("Notification EventType parsed to enum: {Type}", type);

            var notif = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = request.UserId,
                Type = type,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.Now,
                Payload = JsonSerializer.Serialize(request.Payload)
            };

            _logger.LogInformation("Notification enqueued. NotificationId={NotificationId}, Type={Type}", notif.NotificationId, notif.Type);

            await _repo.Add(notif);
        }

    }
}
