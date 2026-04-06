namespace Application.Interfaces.IServices
{
    public interface IDirectoryProfileProvisioningService
    {
        Task ProvisionProfileAsync(Guid authUserId, string role, string firstName, string lastName, string? specialty = null, Guid? providerEntityIdOverride = null);
        Task<Guid> ResolveProviderEntityIdForProviderAdminAsync(Guid providerAdminAuthUserId);
    }
}
