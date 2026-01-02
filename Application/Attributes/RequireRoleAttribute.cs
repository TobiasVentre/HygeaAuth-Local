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
    /// Atributo para endpoints solo de médicos
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireDoctorAttribute : RequireRoleAttribute
    {
        public RequireDoctorAttribute() : base("Doctor") { }
    }

    /// <summary>
    /// Atributo para endpoints solo de pacientes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequirePatientAttribute : RequireRoleAttribute
    {
        public RequirePatientAttribute() : base("Patient") { }
    }

    /// <summary>
    /// Atributo para endpoints de pacientes y médicos
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequirePatientOrDoctorAttribute : RequireRoleAttribute
    {
        public RequirePatientOrDoctorAttribute() : base("Patient", "Doctor") { }
    }
}
