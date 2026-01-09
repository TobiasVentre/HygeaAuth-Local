using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum NotificationType
    {
        // Turnos de servicio
        AppointmentCreated,
        AppointmentConfirmed,
        AppointmentConfirmedDoctor,
        AppointmentCancelled,
        AppointmentRescheduled,
        AppointmentReminder,
        AppointmentStartingSoon,

        AppointmentRescheduledDoctor,

        // Notificación al fumigador
        AppointmentCreatedDoctor,


        AppointmentCancelledByPatient,
        AppointmentCancelledByPatientDoctor,
        AppointmentCancelledByDoctor,
        AppointmentCancelledByDoctorDoctor,

        // Consultas de servicio
        ConsultationStarted,
        ConsultationEnded,
        ConsultationCancelled,
        
        // Recetas y documentos
        PrescriptionReady,
        MedicalOrderReady,
        DocumentGenerated,
        
        // Recordatorios de servicio
        MedicationReminder,
        FollowUpReminder,
        TestResultsReady,
        
        // Sistema general
        AccountActivated,
        PasswordReset,
        EmailVerification,
        Custom
    }
}
