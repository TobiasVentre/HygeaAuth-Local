using Application.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class ResetCodeGenerator : IResetCodeGenerator
    {
        private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";

        public string GenerateResetCode(int length)
        {
            var codeChars = new char[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[sizeof(uint)];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(randomBytes);
                    uint num = BitConverter.ToUInt32(randomBytes, 0);
                    codeChars[i] = AllowedChars[(int)(num % AllowedChars.Length)];
                }
            }
            return new string(codeChars);
        }
    }
}
