using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices
{
    public interface INotificationFormatter
    {
        bool CanHandle(NotificationType type);
        Task<string> FormatAsync(Notification notification, User user);
    }
}
