using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.ICryptographyService
{
    public interface ICryptographyService
    {
        Task<string> GenerateSalt();  
        Task<string> HashPassword(string password);
        Task<bool> VerifyPassword(string hashedPassword, string password);
    }
}
