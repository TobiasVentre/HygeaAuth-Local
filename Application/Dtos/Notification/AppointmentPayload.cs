using System;

namespace Application.Dtos.Notification
{
    public class AppointmentPayload
    {
        public Guid AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string AppointmentType { get; set; } = string.Empty; // "Presencial", "Virtual"
        public string MeetingLink { get; set; } = string.Empty; // Para consultas virtuales
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Pendiente", "Confirmado", "Cancelado"
    }
}
