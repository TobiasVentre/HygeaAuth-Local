using System;

namespace Application.Dtos.Notification
{
    public class MedicalReminderPayload
    {
        public Guid ReminderId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string ReminderType { get; set; } = string.Empty; // "Medicación", "Seguimiento", "Exámenes"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ReminderDate { get; set; }
        public TimeSpan ReminderTime { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
    }
}
