using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IUserServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase.UserServices
{
    public class UserGetServices : IUserGetServices
    {
        private readonly IUserQuery _userQuery;

        public UserGetServices(IUserQuery userQuery)
        {
            _userQuery = userQuery;
        }


        public async Task<UserResponse> GetUserById(Guid id)
        {
            var user = await _userQuery.GetUserById(id);

            if (user == null)
                throw new NotFoundException("No se encontró el usuario");

            return (UserResponse)user;
        }

        public async Task<TechnicianPublicProfileResponse> GetTechnicianPublicProfileById(Guid id)
        {
            var user = await _userQuery.GetUserById(id);

            if (user == null)
                throw new NotFoundException("No se encontró el técnico");

            if (!string.Equals(user.Role, Domain.Entities.UserRoles.Technician, StringComparison.OrdinalIgnoreCase))
                throw new NotFoundException("No se encontró el técnico");

            return new TechnicianPublicProfileResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Specialty = user.Specialty
            };
        }
    }
}
