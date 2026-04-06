using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Query
{
    public class RefreshTokenQuery : IRefreshTokenQuery
    {
        private readonly AppDbContext _context;
        private readonly IRefreshTokenHasher _refreshTokenHasher;

        public RefreshTokenQuery(AppDbContext context, IRefreshTokenHasher refreshTokenHasher)
        {
            _context = context;
            _refreshTokenHasher = refreshTokenHasher;
        }

        public async Task<RefreshToken> GetByToken(string token)
        {
            var hashedToken = _refreshTokenHasher.Hash(token);
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == hashedToken);

            if (refreshToken != null)
            {
                return refreshToken;
            }

            // Compatibilidad temporal: tokens antiguos en texto plano.
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);
        }
    }
}
