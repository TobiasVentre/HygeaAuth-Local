using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.ICryptographyService;
using Application.Interfaces.IServices.IUserServices;
using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using Microsoft.Extensions.Logging;


namespace Application.UseCase.UserServices
{
    public class UserPostServices : IUserPostServices
    {
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;
        private readonly ICryptographyService _cryptographyService;      
        private readonly ILogger<UserPostServices> _logger;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IDirectoryProfileProvisioningService _directoryProfileProvisioningService;

        public UserPostServices(
            IUserQuery userQuery, 
            IUserCommand userCommand, 
            ICryptographyService cryptographyService, 
            ILogger<UserPostServices> logger,
            IEmailVerificationService emailVerificationService,
            IDirectoryProfileProvisioningService directoryProfileProvisioningService
        )
        {
            _userQuery = userQuery;
            _userCommand = userCommand;
            _cryptographyService = cryptographyService;
            _logger = logger;
            _emailVerificationService = emailVerificationService;
            _directoryProfileProvisioningService = directoryProfileProvisioningService;
        }

        public async Task<UserResponse> Register(UserRequest request)
            => await RegisterInternal(request, null, allowProviderAdmin: false);

        public async Task<UserResponse> RegisterProviderAdmin(ProviderAdminCreateRequest request)
        {
            if (request.ProviderEntityId == Guid.Empty)
            {
                throw new InvalidValueException("La entidad proveedora es obligatoria para crear un ProviderAdmin.");
            }

            var providerAdminRequest = new UserRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Dni = request.Dni,
                Password = request.Password,
                Phone = request.Phone,
                Role = UserRoles.ProviderAdmin
            };

            return await RegisterInternal(providerAdminRequest, request.ProviderEntityId, allowProviderAdmin: true);
        }

        public async Task<UserResponse> RegisterTechnicianForProvider(ProviderTechnicianCreateRequest request, Guid providerAdminAuthUserId)
        {
            if (providerAdminAuthUserId == Guid.Empty)
            {
                throw new InvalidValueException("No se pudo resolver el usuario autenticado de la entidad proveedora.");
            }

            var providerEntityId = await _directoryProfileProvisioningService.ResolveProviderEntityIdForProviderAdminAsync(providerAdminAuthUserId);
            var technicianRequest = new UserRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Dni = request.Dni,
                Password = request.Password,
                Phone = request.Phone,
                Role = UserRoles.Technician,
                Specialty = request.Specialty
            };

            return await RegisterInternal(technicianRequest, providerEntityId, allowProviderAdmin: false);
        }

        private async Task<UserResponse> RegisterInternal(UserRequest request, Guid? providerEntityIdOverride, bool allowProviderAdmin)
        {
            await CheckEmailExist(request.Email);
            var hashedPassword = await _cryptographyService.HashPassword(request.Password);

            var roleRaw = string.IsNullOrWhiteSpace(request.Role) ? UserRoles.Client : request.Role.Trim();

            // Normalizar a los valores canónicos (UserRoles.*) aceptando minúsculas
            string role =
                roleRaw.Equals("client", StringComparison.OrdinalIgnoreCase) ? UserRoles.Client :
                roleRaw.Equals("technician", StringComparison.OrdinalIgnoreCase) ? UserRoles.Technician :
                roleRaw.Equals("provideradmin", StringComparison.OrdinalIgnoreCase) ? UserRoles.ProviderAdmin :
                roleRaw.Equals("admin", StringComparison.OrdinalIgnoreCase) ? UserRoles.Admin :
                roleRaw;

            // Si NO querés permitir admin desde registro público, dejalo fuera acá
            var allowedRoles = allowProviderAdmin
                ? new[] { UserRoles.Client, UserRoles.Technician, UserRoles.ProviderAdmin }
                : new[] { UserRoles.Client, UserRoles.Technician };

            if (!allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidValueException(
                    allowProviderAdmin
                        ? $"El rol '{roleRaw}' no es válido. Los roles permitidos son: '{UserRoles.Client}', '{UserRoles.Technician}' o '{UserRoles.ProviderAdmin}'."
                        : $"El rol '{roleRaw}' no es válido para registro público. Los roles permitidos son: '{UserRoles.Client}' o '{UserRoles.Technician}'."
                );
            }

            if (role == UserRoles.Technician && string.IsNullOrWhiteSpace(request.Specialty))
            {
                throw new InvalidValueException("La especialidad es obligatoria para el rol 'Technician'.");
            }

            if ((role == UserRoles.Client || role == UserRoles.ProviderAdmin) && !string.IsNullOrWhiteSpace(request.Specialty))
            {
                throw new InvalidValueException($"La especialidad no debe enviarse para el rol '{role}'.");
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
                Specialty = role == UserRoles.Technician ? request.Specialty?.Trim() : null,
                IsEmailVerified = false, // La cuenta no está verificada hasta que se confirme el código
            };            
            
            _logger.LogInformation("Guardando usuario en base de datos AUTH. Email: {Email}, Role: {Role}", user.Email, user.Role);
            await _userCommand.Insert(user);

            try
            {
                _logger.LogInformation("Provisionando perfil en DirectoryMS. UserId: {UserId}, Role: {Role}", user.UserId, user.Role);
                await _directoryProfileProvisioningService.ProvisionProfileAsync(
                    user.UserId,
                    user.Role,
                    user.FirstName,
                    user.LastName,
                    user.Specialty,
                    providerEntityIdOverride);

                // Enviar código de confirmación por email
                _logger.LogInformation("Enviando código de confirmación a {Email}", user.Email);
                await _emailVerificationService.SendVerificationEmail(user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falló la provisión del perfil en DirectoryMS. Se revierte el usuario {UserId}", user.UserId);
                await _userCommand.Delete(user);
                throw;
            }

            _logger.LogInformation("Usuario guardado exitosamente en base de datos AUTH. UserId: {UserId}, Email: {Email}", user.UserId, user.Email);
            
            
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
