using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface IRefreshTokenCommand
    {
        Task RotateRefreshToken(RefreshToken oldRefreshToken, RefreshToken newRefreshToken);
        Task Delete(RefreshToken refreshToken);
        Task Insert(RefreshToken token);
    }
}
