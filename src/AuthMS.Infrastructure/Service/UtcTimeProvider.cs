using Application.Interfaces.IServices;
using System;

namespace Infrastructure.Service
{
    public class UtcTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
