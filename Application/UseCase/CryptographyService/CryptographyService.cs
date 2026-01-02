using Application.Interfaces.IServices.ICryptographyService;
using Microsoft.Extensions.Configuration;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.CryptographyService
{
    public class CryptographyService : ICryptographyService
    {
        private readonly int _saltSize;
        private readonly int _hashSize;
        private readonly int _degreeOfParallelism;
        private readonly int _memorySize;
        private readonly int _iterations;

        public CryptographyService(IConfiguration configuration)
        {            
            _saltSize = configuration.GetValue<int>("SaltSettings:SaltSize", 16);
            _hashSize = configuration.GetValue<int>("SaltSettings:HashSize", 32);
            _degreeOfParallelism = configuration.GetValue<int>("Argon2Settings:DegreeOfParallelism", 8);
            _memorySize = configuration.GetValue<int>("Argon2Settings:MemorySize", 8192);
            _iterations = configuration.GetValue<int>("Argon2Settings:Iterations", 40);
        }

        public async Task<string> GenerateSalt()
        {
            var buffer = new byte[_saltSize];
            using var rn = RandomNumberGenerator.Create();
            rn.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        public async Task<string> HashPassword(string password)
        {
            var salt = await GenerateSalt();
            byte[] saltBytes = Convert.FromBase64String(salt);

            var hash = await Task.Run(() =>
            {
                using var argon2 = new Argon2i(Encoding.UTF8.GetBytes(password))
                {
                    DegreeOfParallelism = _degreeOfParallelism,
                    MemorySize = _memorySize,
                    Iterations = _iterations,
                    Salt = saltBytes
                };

                return argon2.GetBytes(_hashSize);
            });
            
            var combinedHash = $"{salt}.{Convert.ToBase64String(hash)}";
            return combinedHash;
        }

        public async Task<bool> VerifyPassword(string hashedPassword, string password)
        {            
            var parts = hashedPassword.Split('.');
            if (parts.Length != 2)
                throw new FormatException("El formato del hash almacenado es incorrecto.");

            var salt = parts[0];
            var storedPasswordHash = parts[1];            
            byte[] saltBytes = Convert.FromBase64String(salt);
            
            var hash = await Task.Run(() =>
            {
                using var argon2 = new Argon2i(Encoding.UTF8.GetBytes(password))
                {
                    DegreeOfParallelism = _degreeOfParallelism,
                    MemorySize = _memorySize,
                    Iterations = _iterations,
                    Salt = saltBytes
                };

                return argon2.GetBytes(_hashSize);
            });

            var enteredPasswordHash = Convert.ToBase64String(hash);            
            var storedBytes = Convert.FromBase64String(storedPasswordHash);
            var enteredBytes = Convert.FromBase64String(enteredPasswordHash);
            bool isValid = CryptographicOperations.FixedTimeEquals(storedBytes, enteredBytes);

            return isValid;
        }
    }
}
