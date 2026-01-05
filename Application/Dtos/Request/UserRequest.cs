namespace Application.Dtos.Request
{
    public class UserRequest
    {
        // Datos obligatorios
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Dni { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Client", "Fumigator" o "Admin"
        public string Phone { get; set; }
        // Datos opcionales

        // Datos de Client
        public DateOnly? DateOfBirth { get; set; }
        public string? Adress { get; set; }

        // Datos de fumigator
        public string? LicenseNumber { get; set; }
        public string? Biography { get; set; }
        public string? Specialty { get; set; }

    }
}
