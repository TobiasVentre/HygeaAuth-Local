using Application.Interfaces.IServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Service
{
    public class RefreshTokenHasher : IRefreshTokenHasher
    {
        private readonly byte[] _keyBytes;

        public RefreshTokenHasher(IConfiguration configuration)
        {
            var rawKey = configuration["RefreshTokenSettings:HashKey"]
                         ?? configuration["JwtSettings:key"]
                         ?? string.Empty;

            if (string.IsNullOrWhiteSpace(rawKey))
            {
                throw new InvalidOperationException("No se encontró una clave para hashear refresh tokens.");
            }

            _keyBytes = Encoding.UTF8.GetBytes(rawKey);
            if (_keyBytes.Length < 32)
            {
                _keyBytes = SHA256.HashData(_keyBytes);
            }
        }

        public string Hash(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token vacío.", nameof(token));
            }

            using var hmac = new HMACSHA256(_keyBytes);
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = hmac.ComputeHash(tokenBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
