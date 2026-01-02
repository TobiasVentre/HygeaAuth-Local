using System.Security.Claims;

namespace Domain.Entities
{
    /// <summary>
    /// Constantes para claims personalizados en JWT
    /// </summary>
    public static class CustomClaims
    {
        // Claims de permisos específicos
        public const string CanEditOwnProfile = "CanEditOwnProfile";
        public const string CanViewDoctorInfo = "CanViewDoctorInfo";
        public const string CanViewPatientInfo = "CanViewPatientInfo";
        public const string CanManageAppointments = "CanManageAppointments";
        public const string CanViewOwnAppointments = "CanViewOwnAppointments";
        public const string CanManageSchedule = "CanManageSchedule";
        
        // Claims de información del usuario
        public const string UserId = "UserId";
        public const string UserEmail = "UserEmail";
        public const string UserRole = "UserRole";
        public const string IsEmailVerified = "IsEmailVerified";
        public const string AccountStatus = "AccountStatus";
        
        // Claims de contexto médico
        public const string Specialty = "Specialty"; // Para doctores
        public const string LicenseNumber = "LicenseNumber"; // Para doctores
        public const string PatientId = "PatientId"; // Para pacientes
    }
}
