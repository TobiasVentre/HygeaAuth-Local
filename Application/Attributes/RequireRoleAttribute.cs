using Microsoft.AspNetCore.Authorization;
using System;

namespace Application.Attributes
{
    /// <summary>
    /// Atributo personalizado para requerir roles específicos
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireRoleAttribute : AuthorizeAttribute
    {
        public RequireRoleAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }

    /// <summary>
    /// Atributo para endpoints solo de técnico
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireTechnicianAttribute : RequireRoleAttribute
    {
        public RequireTechnicianAttribute() : base("Technician") { }
    }

    /// <summary>
    /// Atributo para endpoints solo de cliente
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireClientAttribute : RequireRoleAttribute
    {
        public RequireClientAttribute() : base("Client") { }
    }

    /// <summary>
    /// Atributo para endpoints de clientes y técnicos
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireClientOrTechnicianAttribute : RequireRoleAttribute
    {
        public RequireClientOrTechnicianAttribute() : base("Client", "Technician") { }
    }
}
