using Application.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class ArgentinaTimeProvider : ITimeProvider
    {
        private readonly TimeZoneInfo _argentinaTimeZone;

        public ArgentinaTimeProvider()
        {            
            _argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
        }

        public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _argentinaTimeZone);
    }
}
