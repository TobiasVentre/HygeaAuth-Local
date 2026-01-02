using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Dni { get; set; }
        public string Password { get; set; }
        public bool IsEmailVerified { get; set; }           // Estado de verificación del email
        public int AccessFailedCount { get; set; }           // Número de intentos fallidos
        public DateTime? LockoutEndDate { get; set; }       // Fecha/hora de desbloqueo

        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<Notification> Notifications { get; set; }

    }
}
