using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public enum NotificationType
    {
        // Turnos médicos
        AppointmentCreated,
        AppointmentConfirmed,
        AppointmentConfirmedDoctor,
        AppointmentCancelled,
        AppointmentRescheduled,
        AppointmentReminder,
        AppointmentStartingSoon,

        AppointmentRescheduledDoctor,

        // Notificación al doctor
        AppointmentCreatedDoctor,


        AppointmentCancelledByPatient,
        AppointmentCancelledByPatientDoctor,
        AppointmentCancelledByDoctor,
        AppointmentCancelledByDoctorDoctor,

        // Consultas médicas
        ConsultationStarted,
        ConsultationEnded,
        ConsultationCancelled,
        
        // Recetas y documentos
        PrescriptionReady,
        MedicalOrderReady,
        DocumentGenerated,
        
        // Recordatorios médicos
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