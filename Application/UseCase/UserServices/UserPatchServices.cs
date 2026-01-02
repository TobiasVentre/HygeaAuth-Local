using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IUserServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.UserServices
{
    public class UserPatchServices : IUserPatchServices
    {
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;

        public UserPatchServices(IUserQuery userQuery, IUserCommand userCommand)
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
        }

        public async Task<GenericResponse> RemoveUserImage(int Id)
        {
            var user = await _userQuery.GetUserById(Id);

            if (user == null)
            {
                throw new NotFoundException("No se encontró ningún usuario con el ID " + Id);
            }

            await _userCommand.Update(user);

            return new GenericResponse { Message = "¡Tu foto de perfil ha sido eliminada exitosamente!" };

        }
    }
}
