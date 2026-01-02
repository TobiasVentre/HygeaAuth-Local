using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface IPasswordResetCommand
    {
        Task Insert(PasswordResetToken token);
        Task Delete(PasswordResetToken refreshToken);
    }
}
