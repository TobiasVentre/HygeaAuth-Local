using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.ICryptographyService;
using Application.Interfaces.IServices.IUserServices;
using Application.Interfaces.IServices.IAuthServices;
using Application.Interfaces.Messaging;
using Domain.Entities;
using Domain.Events;
using Microsoft.Extensions.Logging;


namespace Application.UseCase.UserServices
{
    public class UserPostServices : IUserPostServices
    {
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;
        private readonly ICryptographyService _cryptographyService;      
        private readonly ILogger<UserPostServices> _logger;
        private readonly IUserCreatedEventPublisher _userCreatedEventPublisher;
        private readonly IEmailVerificationService _emailVerificationService;

        public UserPostServices(
            IUserQuery userQuery, 
            IUserCommand userCommand, 
            ICryptographyService cryptographyService, 
            ILogger<UserPostServices> logger,
            IUserCreatedEventPublisher userCreatedEventPublisher,
            IEmailVerificationService emailVerificationService
        )
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
            _cryptographyService = cryptographyService;
            _logger = logger;
            _userCreatedEventPublisher = userCreatedEventPublisher;
            _emailVerificationService = emailVerificationService;
        }

        public async Task<UserResponse> Register(UserRequest request)
        {
            await CheckEmailExist(request.Email);
            var hashedPassword = await _cryptographyService.HashPassword(request.Password);

            // Validar y asignar el rol: si viene vacío o null, por defecto es Patient
            var role = string.IsNullOrWhiteSpace(request.Role) 
                ? UserRoles.Patient 
                : request.Role.Trim(); // Limpiar espacios en blanco

            // Validar que el rol sea válido (comparación case-sensitive)
            if (role != UserRoles.Patient && role != UserRoles.Doctor)
            {
                throw new InvalidValueException($"El rol '{role}' no es válido. Los roles permitidos son: '{UserRoles.Patient}' o '{UserRoles.Doctor}'");
            }
            
            _logger.LogInformation("Registrando usuario con rol: {Role}", role);
            var user = new User
            {
                Role = role,
                IsActive = true,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Dni = request.Dni,
                Password = hashedPassword,
                IsEmailVerified = false, // La cuenta no está verificada hasta que se confirme el código
            };            
            
            _logger.LogInformation("Guardando usuario en base de datos AUTH. Email: {Email}, Role: {Role}", user.Email, user.Role);
            await _userCommand.Insert(user);

            // Enviar código de confirmación por email
            _logger.LogInformation("Enviando código de confirmación a {Email}", user.Email);
            await _emailVerificationService.SendVerificationEmail(user.Email);

            var evt = new UserCreatedEvent
            {
                UserId = user.UserId,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Dni = user.Dni,

                DateOfBirth = request.DateOfBirth,
                Adress = request.Adress,
                HealthPlan = request.HealthPlan,
                MembershipNumber = request.MembershipNumber,
                LicenseNumber = request.LicenseNumber,
                Phone = request.Phone,
                Biography = request.Biography,
                Specialty = request.Specialty
            };

            //publicar evento en rabbitmq
            await _userCreatedEventPublisher.PublishAsync(evt);

            _logger.LogInformation("Usuario guardado exitosamente en base de datos AUTH. UserId: {UserId}, Email: {Email}", user.UserId, user.Email);
            
            
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
