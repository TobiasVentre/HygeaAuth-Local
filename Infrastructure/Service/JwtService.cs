using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class JwtService : IAuthTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GenerateAccessToken(User user)
        {

            var rawKey = _configuration["JwtSettings:key"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(rawKey))
            {
                throw new InvalidOperationException("JwtSettings:key no está configurado.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(rawKey);

            // HMAC-SHA256 necesita una llave de al menos 256 bits (32 bytes).
            if (keyBytes.Length < 32)
            {
                keyBytes = SHA256.HashData(keyBytes);
            }

            var securityKey = new SymmetricSecurityKey(keyBytes);

            var signingCredentials = new SigningCredentials(
                key: securityKey,
                algorithm: SecurityAlgorithms.HmacSha256Signature
            );

            var claims = new ClaimsIdentity();
            
            // Claims estándar
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            claims.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Role, user.Role));
            claims.AddClaim(new Claim("IsActive", user.IsActive.ToString()));
            claims.AddClaim(new Claim("UserId", user.UserId.ToString()));
            claims.AddClaim(new Claim("FirstName", user.FirstName.ToString()));
            claims.AddClaim(new Claim("LastName", user.LastName.ToString()));
            
            // Claims personalizados para CuidarMed+
            claims.AddClaim(new Claim(CustomClaims.UserId, user.UserId.ToString()));
            claims.AddClaim(new Claim(CustomClaims.UserEmail, user.Email));
            claims.AddClaim(new Claim(CustomClaims.UserRole, user.Role));
            claims.AddClaim(new Claim(CustomClaims.IsEmailVerified, user.IsEmailVerified.ToString()));
            claims.AddClaim(new Claim(CustomClaims.AccountStatus, user.IsActive ? "Active" : "Inactive"));
            
            // Claims de permisos específicos según el rol
            AddRoleBasedClaims(claims, user);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:TokenExpirationMinutes"]!)),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();            
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
            var serializedJwt = tokenHandler.WriteToken(tokenConfig);

            return serializedJwt;
        }

        public Task<string> GenerateRefreshToken()
        {
            var size = int.Parse(_configuration["RefreshTokenSettings:Lenght"]!);
            var buffer = new byte[size];
            using var rn = RandomNumberGenerator.Create();
            rn.GetBytes(buffer);

            return Task.FromResult(Convert.ToBase64String(buffer));
        }
        public Task<int> GetRefreshTokenLifetimeInMinutes()
        {
            return Task.FromResult(int.Parse(_configuration["RefreshTokenSettings:LifeTimeInMinutes"]!));
        }

        /// <summary>
        /// Agrega claims específicos según el rol del usuario
        /// </summary>
        private void AddRoleBasedClaims(ClaimsIdentity claims, User user)
        {
            // Permisos comunes para todos los usuarios autenticados
            claims.AddClaim(new Claim(CustomClaims.CanEditOwnProfile, "true"));
            claims.AddClaim(new Claim(CustomClaims.CanViewOwnAppointments, "true"));

            // Permisos específicos para Doctores
            if (user.Role == UserRoles.Doctor)
            {
                claims.AddClaim(new Claim(CustomClaims.CanViewDoctorInfo, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageAppointments, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageSchedule, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanViewPatientInfo, "true"));
                
                // Claims específicos de doctores (ejemplo - puedes expandir según tu modelo)
                // claims.AddClaim(new Claim(CustomClaims.Specialty, user.Specialty ?? ""));
                // claims.AddClaim(new Claim(CustomClaims.LicenseNumber, user.LicenseNumber ?? ""));
            }

            // Permisos específicos para Pacientes
            if (user.Role == UserRoles.Patient)
            {
                claims.AddClaim(new Claim(CustomClaims.CanViewPatientInfo, "true"));
                claims.AddClaim(new Claim(CustomClaims.PatientId, user.UserId.ToString()));
                
                // Los pacientes pueden ver información básica de doctores para reservar turnos
                claims.AddClaim(new Claim(CustomClaims.CanViewDoctorInfo, "limited"));
            }
        }
        
    }
    
}
