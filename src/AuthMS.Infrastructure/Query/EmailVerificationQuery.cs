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
            return await _context.EmailVerificationTokens.FirstOrDefaultAsync(
                t => t.Email == email && EF.Functions.Collate(t.Token, "utf8mb4_bin") == verificationCode);
        }

        public async Task<IEnumerable<EmailVerificationToken>> GetExpiredTokensByEmail(string email, DateTime referenceTimeUtc)
        {
            return await _context.EmailVerificationTokens
                                 .Where(t => t.Email == email && t.Expiration < referenceTimeUtc)
                                 .ToListAsync();
        }
    }
}
