using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class EmailVerificationToken
    {
        public Guid Id { get; set; }
        
        public string Email { get; set; }
        
        public string Token { get; set; }
        
        public DateTime Expiration { get; set; }
    }
}
