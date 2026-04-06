using Application.Dtos.Request;
using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IUserServices
{
    public interface IUserPostServices
    {
        Task<UserResponse> Register(UserRequest request);
        Task<UserResponse> RegisterTechnicianForProvider(ProviderTechnicianCreateRequest request, Guid providerAdminAuthUserId);
        Task<UserResponse> RegisterProviderAdmin(ProviderAdminCreateRequest request);
    }
}
