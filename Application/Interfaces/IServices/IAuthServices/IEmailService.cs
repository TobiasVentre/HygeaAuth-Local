using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IAuthServices
{
    public interface IEmailService
    {
        Task SendPasswordResetEmail(string email, string resetCode);
        Task SendEmailVerification(string email, string verificationCode);
        Task SendCustomNotification(string email, string message);
    }
}
