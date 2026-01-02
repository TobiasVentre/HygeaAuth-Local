using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Request
{
    public class EmailVerificationRequest
    {
        public string Email { get; set; }
        public string VerificationCode { get; set; }
    }
}
