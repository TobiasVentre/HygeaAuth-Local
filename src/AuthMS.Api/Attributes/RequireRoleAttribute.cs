using Microsoft.AspNetCore.Authorization;

namespace AuthMS.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireRoleAttribute : AuthorizeAttribute
    {
        public RequireRoleAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireTechnicianAttribute : RequireRoleAttribute
    {
        public RequireTechnicianAttribute() : base("Technician")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireClientAttribute : RequireRoleAttribute
    {
        public RequireClientAttribute() : base("Client")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireClientOrTechnicianAttribute : RequireRoleAttribute
    {
        public RequireClientOrTechnicianAttribute() : base("Client", "Technician")
        {
        }
    }
}
