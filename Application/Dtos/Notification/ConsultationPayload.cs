using System;

namespace Application.Dtos.Notification
{
    public class ConsultationPayload
    {
        public Guid ConsultationId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public DateTime ConsultationDate { get; set; }
        public TimeSpan ConsultationTime { get; set; }
        public string MeetingLink { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Iniciada", "En curso", "Finalizada", "Cancelada"
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
