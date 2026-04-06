using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class RefreshTokenRequest
    {
        public string ExpiredAccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
