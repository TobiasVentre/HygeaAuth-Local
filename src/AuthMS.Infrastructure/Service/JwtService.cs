using Application.Exceptions;
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

        public Task<string> GenerateAccessToken(User user)
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
                Expires = DateTime.UtcNow.AddMinutes(GetAccessTokenLifetimeInMinutes()),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();            
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
            var serializedJwt = tokenHandler.WriteToken(tokenConfig);

            return Task.FromResult(serializedJwt);
        }

        public Task<string> GenerateRefreshToken()
        {
            var size = _configuration.GetValue<int?>("RefreshTokenSettings:Lenght")
                ?? _configuration.GetValue<int?>("RefreshTokenSettings:Length")
                ?? 64;

            var buffer = new byte[size];
            using var rn = RandomNumberGenerator.Create();
            rn.GetBytes(buffer);

            return Task.FromResult(Convert.ToBase64String(buffer));
        }
        public Task<int> GetRefreshTokenLifetimeInMinutes()
        {
            return Task.FromResult(_configuration.GetValue<int?>("RefreshTokenSettings:LifeTimeInMinutes") ?? 240);
        }

        public int GetRefreshTokenIdleTimeoutInMinutes()
        {
            return _configuration.GetValue<int>("RefreshTokenSettings:IdleTimeoutMinutes", 15);
        }

        public int GetMaxFailedAccessAttempts()
        {
            return _configuration.GetValue<int>("LockoutSettings:MaxFailedAccessAttempts", 5);
        }

        public int GetLockoutDurationMinutes()
        {
            return _configuration.GetValue<int>("LockoutSettings:LockoutDurationMinutes", 15);
        }

        public Guid GetUserIdFromExpiredAccessToken(string token)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var claimValue = principal.FindFirst(CustomClaims.UserId)?.Value
                             ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(claimValue, out var userId))
            {
                throw new InvalidRefreshTokenException("El Access Token expirado es invalido.");
            }

            return userId;
        }

        /// <summary>
        /// Agrega claims específicos según el rol del usuario
        /// </summary>
        private void AddRoleBasedClaims(ClaimsIdentity claims, User user)
        {
            // Permisos comunes para todos los usuarios autenticados
            claims.AddClaim(new Claim(CustomClaims.CanEditOwnProfile, "true"));
            claims.AddClaim(new Claim(CustomClaims.CanViewOwnAppointments, "true"));

            // Permisos específicos para técnicos
            if (user.Role == UserRoles.Technician)
            {
                claims.AddClaim(new Claim(CustomClaims.CanViewTechnicianInfo, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageAppointments, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageSchedule, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanViewClientInfo, "true"));
                
            }

            // Permisos específicos para clientes
            if (user.Role == UserRoles.Client)
            {
                claims.AddClaim(new Claim(CustomClaims.CanViewClientInfo, "true"));
                claims.AddClaim(new Claim(CustomClaims.UserId, user.UserId.ToString()));
                
                // Los clientes pueden ver información básica de técnicos para reservar turnos
                claims.AddClaim(new Claim(CustomClaims.CanViewTechnicianInfo, "limited"));
            }

            if (user.Role == UserRoles.ProviderAdmin)
            {
                claims.AddClaim(new Claim(CustomClaims.CanViewTechnicianInfo, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanViewClientInfo, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageAppointments, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageSchedule, "true"));
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var rawKey = _configuration["JwtSettings:key"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawKey))
            {
                throw new InvalidOperationException("JwtSettings:key no esta configurado.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(rawKey);
            if (keyBytes.Length < 32)
            {
                keyBytes = SHA256.HashData(keyBytes);
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidRefreshTokenException("El Access Token expirado es invalido.");
            }

            return principal;
        }

        private int GetAccessTokenLifetimeInMinutes()
        {
            return _configuration.GetValue<int?>("JwtSettings:TokenExpirationMinutes") ?? 18000;
        }
        
    }
    
}
