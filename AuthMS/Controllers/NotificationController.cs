using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthMS.Controllers
{
    [Route("api/v1/notifications/events")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Enqueues a new notification event for asynchronous processing.
        /// </summary>
        /// <param name="request"> the details of the notification event to be created.</param>
        /// <response code="202">The request was accepted and the notification event has been enqueued.</response>
        [HttpPost]
        public async Task<IActionResult> PostEvent([FromBody] NotificationEventRequest request)
        {
            await _notificationService.EnqueueEvent(request);
            return Accepted();
        }
    }
}
