using Domain.Entities;

namespace Application.Dtos.Response
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Dni { get; set; }
        public string Role { get; set; }
        public string? Specialty { get; set; }



        public static explicit operator UserResponse(User user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Dni = user.Dni,
                Role = user.Role,
                Specialty = user.Specialty
            };
        }
    }
}
