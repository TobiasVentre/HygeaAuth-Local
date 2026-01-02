using System.Threading.Tasks;

namespace Application.Interfaces.IServices
{
    public interface IDirectoryService
    {
        Task<bool> CreatePatientAsync(int userId, string firstName, string lastName, string dni);
        Task<bool> CreateDoctorAsync(int userId, string firstName, string lastName);
    }
}

