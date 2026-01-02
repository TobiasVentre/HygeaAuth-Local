using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IUserServices;

namespace Application.UseCase.UserServices
{
    public class UserPutServices : IUserPutServices
    {
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;

        public UserPutServices(IUserQuery userQuery, IUserCommand userCommand)
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
        }

        public async Task<UserResponse> UpdateUser(int Id, UserUpdateRequest request)
        {
            var user = await _userQuery.GetUserById(Id);

            if (user == null)
            {
                throw new NotFoundException("No se encontró ningún usuario con el ID  " + Id);
            }
            
            if (user.Email != request.Email)
            {
                await CheckEmailExist(request.Email);
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Dni = request.Dni;
            
            

            await _userCommand.Update(user);

            return new UserResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Dni = user.Dni,
                Role = user.Role
            };
        }

        private async Task CheckEmailExist(string email)
        {
            var emailExist = await _userQuery.ExistEmail(email);

            if (emailExist)
            {
                throw new InvalidEmailException("El correo electrónico ingresado ya está registrado.");
            }
        }
    }
    
}
