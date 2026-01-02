using Application.Interfaces.ICommand;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Command
{
    public class RefreshTokenCommand : IRefreshTokenCommand
    {
        private readonly AppDbContext _context;

        public RefreshTokenCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task Insert(RefreshToken token)
        {
            _context.RefreshTokens.AddAsync(token);

            await _context.SaveChangesAsync();
        }

        public async Task Delete(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);

            await _context.SaveChangesAsync();
        }

        public async Task RotateRefreshToken(RefreshToken oldRefreshToken, RefreshToken newRefreshToken)
        {
            _context.RefreshTokens.Remove(oldRefreshToken);
            _context.RefreshTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync();
        }
    }
}
