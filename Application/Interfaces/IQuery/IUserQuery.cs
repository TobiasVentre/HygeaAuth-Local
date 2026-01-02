using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IQuery
{
    public interface IUserQuery
    {        
        Task<User> GetUserById(int id);
        Task<User> GetUserByEmail(string email);
        Task<bool> ExistEmail(string email);
    }
}
