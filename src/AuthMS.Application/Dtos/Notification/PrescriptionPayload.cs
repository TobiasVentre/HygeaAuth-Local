using System;

namespace Application.Dtos.Notification
{
    public class PrescriptionPayload
    {
        public Guid PrescriptionId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsReady { get; set; }
    }
}
