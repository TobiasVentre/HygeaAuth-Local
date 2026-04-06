using Application.Dtos.Request;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace AuthMS.Api.Controllers
{
    [Route("api/v1/notifications/events")]
    [ApiController]
    [Obsolete("Legacy endpoint. Functional events must be emitted by the owning bounded context, not by AuthMS.")]
    public class NotificationController(INotificationService notificationService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PostEvent([FromBody] NotificationEventRequest request)
        {
            await notificationService.EnqueueEvent(request);
            return Accepted();
        }
    }
}
