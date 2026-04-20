using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.ICryptographyService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Service
{
    public sealed class BootstrapUserSeeder
    {
        private readonly IConfiguration _configuration;
        private readonly IUserQuery _userQuery;
        private readonly IUserCommand _userCommand;
        private readonly ICryptographyService _cryptographyService;
        private readonly IDirectoryProfileProvisioningService _directoryProfileProvisioningService;
        private readonly ILogger<BootstrapUserSeeder> _logger;

        public BootstrapUserSeeder(
            IConfiguration configuration,
            IUserQuery userQuery,
            IUserCommand userCommand,
            ICryptographyService cryptographyService,
            IDirectoryProfileProvisioningService directoryProfileProvisioningService,
            ILogger<BootstrapUserSeeder> logger)
        {
            _configuration = configuration;
            _userQuery = userQuery;
            _userCommand = userCommand;
            _cryptographyService = cryptographyService;
            _directoryProfileProvisioningService = directoryProfileProvisioningService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await SeedAdminAsync();
            await SeedProviderAdminAsync();
        }

        private async Task SeedAdminAsync()
        {
            var settings = GetSettings("BootstrapUsers:Admin");
            if (!settings.Enabled)
            {
                return;
            }

            await EnsureUserExistsAsync(settings, UserRoles.Admin, providerEntityId: null);
        }

        private async Task SeedProviderAdminAsync()
        {
            var settings = GetSettings("BootstrapUsers:ProviderAdmin");
            if (!settings.Enabled)
            {
                return;
            }

            var providerEntityIdRaw = _configuration["BootstrapUsers:ProviderAdmin:ProviderEntityId"];
            Guid? providerEntityId = null;

            if (!string.IsNullOrWhiteSpace(providerEntityIdRaw))
            {
                if (!Guid.TryParse(providerEntityIdRaw, out var parsedProviderEntityId))
                {
                    throw new InvalidOperationException("BootstrapUsers:ProviderAdmin:ProviderEntityId no es un GUID valido.");
                }

                providerEntityId = parsedProviderEntityId;
            }

            await EnsureUserExistsAsync(settings, UserRoles.ProviderAdmin, providerEntityId);
        }

        private async Task EnsureUserExistsAsync(BootstrapUserSettings settings, string role, Guid? providerEntityId)
        {
            var existingUser = await _userQuery.GetUserByEmail(settings.Email);
            if (existingUser is not null)
            {
                _logger.LogInformation(
                    "Bootstrap user skipped because email already exists. Email: {Email}, ExistingRole: {ExistingRole}, RequestedRole: {RequestedRole}",
                    existingUser.Email,
                    existingUser.Role,
                    role);
                return;
            }

            var user = new User
            {
                Role = role,
                IsActive = true,
                IsEmailVerified = true,
                FirstName = settings.FirstName,
                LastName = settings.LastName,
                Email = settings.Email,
                Dni = settings.Dni,
                Password = await _cryptographyService.HashPassword(settings.Password),
                Specialty = null
            };

            await _userCommand.Insert(user);

            try
            {
                if (role == UserRoles.ProviderAdmin)
                {
                    await _directoryProfileProvisioningService.ProvisionProfileAsync(
                        user.UserId,
                        user.Role,
                        user.FirstName,
                        user.LastName,
                        specialty: null,
                        providerEntityIdOverride: providerEntityId);
                }
            }
            catch
            {
                await _userCommand.Delete(user);
                throw;
            }

            _logger.LogInformation("Bootstrap user created successfully. Email: {Email}, Role: {Role}", user.Email, user.Role);
        }

        private BootstrapUserSettings GetSettings(string sectionPath)
        {
            var enabled = _configuration.GetValue<bool>($"{sectionPath}:Enabled");
            if (!enabled)
            {
                return BootstrapUserSettings.Disabled;
            }

            return new BootstrapUserSettings(
                Enabled: true,
                FirstName: GetRequiredValue(sectionPath, "FirstName"),
                LastName: GetRequiredValue(sectionPath, "LastName"),
                Email: GetRequiredValue(sectionPath, "Email"),
                Dni: GetRequiredValue(sectionPath, "Dni"),
                Password: GetRequiredValue(sectionPath, "Password"));
        }

        private string GetRequiredValue(string sectionPath, string key)
        {
            var value = _configuration[$"{sectionPath}:{key}"]?.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            throw new InvalidOperationException($"{sectionPath}:{key} es obligatorio cuando el bootstrap esta habilitado.");
        }

        private sealed record BootstrapUserSettings(
            bool Enabled,
            string FirstName,
            string LastName,
            string Email,
            string Dni,
            string Password)
        {
            public static BootstrapUserSettings Disabled { get; } = new(false, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }
    }
}
