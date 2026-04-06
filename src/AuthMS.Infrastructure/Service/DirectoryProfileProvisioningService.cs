using System.Net;
using System.Net.Http.Json;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Service
{
    internal sealed class DirectoryProfileProvisioningService : IDirectoryProfileProvisioningService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DirectoryProfileProvisioningService> _logger;

        public DirectoryProfileProvisioningService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<DirectoryProfileProvisioningService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ProvisionProfileAsync(Guid authUserId, string role, string firstName, string lastName, string? specialty = null, Guid? providerEntityIdOverride = null)
        {
            var providerEntityId = providerEntityIdOverride ?? await ResolveDefaultProviderEntityIdAsync();

            if (role == UserRoles.Client)
            {
                var fullName = $"{firstName} {lastName}".Trim();
                await PostJsonAsync("v1/client-profiles", new
                {
                    authUserId,
                    providerEntityId,
                    fullName
                });

                return;
            }

            if (role == UserRoles.Technician)
            {
                if (string.IsNullOrWhiteSpace(specialty))
                {
                    throw new InvalidOperationException("No se puede crear TechnicianProfile en DirectoryMS sin specialty.");
                }

                await PostJsonAsync("v1/technician-profiles", new
                {
                    authUserId,
                    providerEntityId,
                    specialty = specialty.Trim()
                });

                return;
            }

            if (role == UserRoles.ProviderAdmin)
            {
                var fullName = $"{firstName} {lastName}".Trim();
                await PostJsonAsync("v1/provider-admin-profiles", new
                {
                    authUserId,
                    providerEntityId,
                    fullName
                });

                return;
            }

            _logger.LogInformation("No se requiere provision en DirectoryMS para el rol {Role}", role);
        }

        public async Task<Guid> ResolveProviderEntityIdForProviderAdminAsync(Guid providerAdminAuthUserId)
        {
            var profile = await _httpClient.GetFromJsonAsync<ProviderAdminProfileDto>($"v1/provider-admin-profiles/by-auth-user/{providerAdminAuthUserId}");
            if (profile is null || profile.ProviderEntityId == Guid.Empty)
            {
                throw new InvalidOperationException("No se pudo resolver la entidad proveedora del usuario autenticado.");
            }

            return profile.ProviderEntityId;
        }

        private async Task<Guid> ResolveDefaultProviderEntityIdAsync()
        {
            var configuredProviderId = _configuration["Integrations:DirectoryMS:DefaultProviderEntityId"];
            if (Guid.TryParse(configuredProviderId, out var providerEntityId))
            {
                return providerEntityId;
            }

            var providers = await _httpClient.GetFromJsonAsync<List<ProviderEntityDto>>("v1/providers");
            var enabledProvider = providers?.FirstOrDefault(x => x.IsEnabled);

            if (enabledProvider is null)
            {
                throw new InvalidOperationException("No hay proveedores habilitados en DirectoryMS para crear el perfil.");
            }

            return enabledProvider.Id;
        }

        private async Task PostJsonAsync(string relativePath, object payload)
        {
            using var response = await _httpClient.PostAsJsonAsync(relativePath, payload);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new InvalidOperationException($"DirectoryMS rechazo la provision del perfil. {body}");
            }

            throw new InvalidOperationException(
                $"DirectoryMS devolvio {(int)response.StatusCode} {response.ReasonPhrase} al crear el perfil. {body}");
        }

        private sealed class ProviderEntityDto
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public bool IsEnabled { get; init; }
        }

        private sealed class ProviderAdminProfileDto
        {
            public Guid Id { get; init; }
            public Guid AuthUserId { get; init; }
            public Guid ProviderEntityId { get; init; }
            public string FullName { get; init; } = string.Empty;
        }
    }
}
