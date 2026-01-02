using Application.Interfaces.IQuery;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Query
{
    public class EmailVerificationQuery : IEmailVerificationQuery
    {
        private readonly AppDbContext _context;

        public EmailVerificationQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmailVerificationToken> GetByEmailAndCode(string email, string verificationCode)
        {
            // Obtener todos los tokens para el email y comparar case-insensitive en memoria
            var tokens = await _context.EmailVerificationTokens
                .Where(t => t.Email == email)
                .ToListAsync();
            
            // Comparación case-insensitive
            return tokens.FirstOrDefault(t => 
                string.Equals(t.Token, verificationCode, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<EmailVerificationToken>> GetExpiredTokensByEmail(string email)
        {
            return await _context.EmailVerificationTokens
                                 .Where(t => t.Email == email && t.Expiration < DateTime.UtcNow)
                                 .ToListAsync();
        }
    }
}
