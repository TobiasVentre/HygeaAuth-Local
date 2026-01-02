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
    public class RefreshTokenQuery : IRefreshTokenQuery
    {
        private readonly AppDbContext _context;

        public RefreshTokenQuery(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GetByToken(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);

            return refreshToken;
        }
    }
}
