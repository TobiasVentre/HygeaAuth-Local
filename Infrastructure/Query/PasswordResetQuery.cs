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
    public class PasswordResetQuery : IPasswordResetQuery
    {
        private readonly AppDbContext _context;

        public PasswordResetQuery(AppDbContext context)
        {
            _context = context;
        }       

        public async Task<PasswordResetToken> GetByEmailAndCode(string email, string resetCode)
        {
            return await _context.PasswordResetTokens.FirstOrDefaultAsync(
                t => t.Email == email && EF.Functions.Collate(t.Token, "Latin1_General_CS_AS") == resetCode);
        }

        public async Task<IEnumerable<PasswordResetToken>> GetExpiredTokensByEmail(string email)
        {
            return await _context.PasswordResetTokens
                                 .Where(t => t.Email == email && t.Expiration < DateTime.Now)
                                 .ToListAsync();
        }
    }
}
