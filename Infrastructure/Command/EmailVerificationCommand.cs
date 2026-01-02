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
    public class EmailVerificationCommand : IEmailVerificationCommand
    {
        private readonly AppDbContext _context;

        public EmailVerificationCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task Insert(EmailVerificationToken token)
        {
             _context.EmailVerificationTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }
}
