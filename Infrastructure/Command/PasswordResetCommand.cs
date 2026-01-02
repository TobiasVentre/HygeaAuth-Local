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
    public class PasswordResetCommand : IPasswordResetCommand
    {
        private readonly AppDbContext _context;

        public PasswordResetCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task Insert(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Add(token);

            await _context.SaveChangesAsync();
        }

        public async Task Delete(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Remove(token);

            await _context.SaveChangesAsync();
        }
    }
}
