using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Messaging
{
    public interface IUserCreatedEventPublisher
    {
        Task PublishAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default);
    }
}
