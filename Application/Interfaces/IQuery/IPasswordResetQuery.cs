using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IPasswordResetQuery
    {
        Task<PasswordResetToken> GetByEmailAndCode(string email, string resetCode);
        Task<IEnumerable<PasswordResetToken>> GetExpiredTokensByEmail(string email);
    }
}
