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
    public class UserCommand : IUserCommand
    {
        private readonly AppDbContext _context;

        public UserCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task Insert(User user)
        {
            _context.Users.Add(user);
            var saved = await _context.SaveChangesAsync();
            if (saved == 0)
            {
                throw new Exception("No se pudo guardar el usuario en la base de datos. Ningún cambio fue persistido.");
            }
            if (user.UserId == Guid.Empty)
            {
                throw new Exception("Error: El UserId no se asignó correctamente después de guardar.");
            }
        }

        public async Task Update(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
