using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IUserServices
{
    public interface IUserGetServices
    {
        Task<UserResponse> GetUserById(int id);
    }
}
