using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class NotificationEventRequest
    {
        public int UserId { get; set; }
        public string EventType { get; set; }
        public object? Payload { get; set; }
    }
}
