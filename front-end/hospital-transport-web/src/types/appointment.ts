export interface Appointment {
  id: string;
  patient: Patient;
  medicalRecordNumber: string;
  destinationHospital: string;
  treatmentType: string;
  treatmentTypeOther?: string;
  isPriority: boolean;
  seatNumber: number;
  appointmentDate: string;
  companion?: Patient;
  companionSeatNumber?: number;
  createdByUserName: string;
  createdAt: string;
  isTicketPrinted: boolean;
}

export interface CreateAppointmentRequest {
  patientId: string;
  medicalRecordNumber: string;
  destinationHospital: string;
  treatmentType: number;
  treatmentTypeOther?: string;
  isPriority: boolean;
  seatNumber: number;
  appointmentDate: string;
  companionId?: string;
  companionSeatNumber?: number;
  createdByUserId: string;
}

export interface SeatAvailability {
  seatNumber: number;
  isAvailable: boolean;
  isPriorityOnly: boolean;
}

interface Patient {
  id: string;
  fullName: string;
  cpf: string;
  susCardNumber: string;
}