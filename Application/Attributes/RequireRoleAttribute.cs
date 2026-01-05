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
    /// Atributo para endpoints solo de fumigador
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireFumigatorAttribute : RequireRoleAttribute
    {
        public RequireFumigatorAttribute() : base("Fumigator") { }
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
    /// Atributo para endpoints de clientes y fumigadores
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireClientOrFumigatorAttribute : RequireRoleAttribute
    {
        public RequireClientOrFumigatorAttribute() : base("Client", "Fumigator") { }
    }
}
