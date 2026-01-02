using System.Threading.Tasks;

namespace Application.Interfaces.IServices
{
    public interface IDirectoryService
    {
        Task<bool> CreateClientAsync(int userId, string firstName, string lastName, string dni);
        Task<bool> CreateFumigatorAsync(int userId, string firstName, string lastName);
        Task<bool> CreateAdminAsync(int userId, string firstName, string lastName);
    }
}

