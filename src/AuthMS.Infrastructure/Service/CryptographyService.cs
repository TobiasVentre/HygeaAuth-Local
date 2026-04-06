using Application.Interfaces.IServices.ICryptographyService;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Service
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

        public Task<string> GenerateSalt()
        {
            var buffer = new byte[_saltSize];
            using var rn = RandomNumberGenerator.Create();
            rn.GetBytes(buffer);
            return Task.FromResult(Convert.ToBase64String(buffer));
        }

        public async Task<string> HashPassword(string password)
        {
            var salt = await GenerateSalt();
            var saltBytes = Convert.FromBase64String(salt);

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

            return $"{salt}.{Convert.ToBase64String(hash)}";
        }

        public async Task<bool> VerifyPassword(string hashedPassword, string password)
        {
            var parts = hashedPassword.Split('.');
            if (parts.Length != 2)
            {
                throw new FormatException("El formato del hash almacenado es incorrecto.");
            }

            var saltBytes = Convert.FromBase64String(parts[0]);
            var storedBytes = Convert.FromBase64String(parts[1]);

            var hash = await Task.Run(() =>
            {
                using var argon2 = new Argon2i(Encoding.UTF8.GetBytes(password))
                {
                    DegreeOfParallelism = _degreeOfParallelism,
                    MemorySize = _memorySize,
                    Iterations = _iterations,
                    Salt = saltBytes
                };

                return argon2.GetBytes(storedBytes.Length);
            });

            return CryptographicOperations.FixedTimeEquals(storedBytes, hash);
        }
    }
}
