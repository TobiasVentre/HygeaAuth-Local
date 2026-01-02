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
            // El UserId se asigna automáticamente después de SaveChangesAsync
            // Verificar que se asignó correctamente
            if (user.UserId <= 0)
            {
                throw new Exception($"Error: El UserId no se asignó correctamente después de guardar. UserId actual: {user.UserId}");
            }
        }

        public async Task Update(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
