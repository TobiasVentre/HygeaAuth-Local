namespace Application.Dtos.Response
{
    public class TechnicianPublicProfileResponse
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Specialty { get; set; }
    }
}
