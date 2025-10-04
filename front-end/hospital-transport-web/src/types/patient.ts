export interface Patient {
  id: string;
  fullName: string;
  rg: string;
  cpf: string;
  age: number;
  birthDate: string;
  susCardNumber: string;
  phoneNumber: string;
  motherName: string;
  createdAt: string;
}

export interface CreatePatientRequest {
  fullName: string;
  rg: string;
  cpf: string;
  age: number;
  birthDate: string;
  susCardNumber: string;
  phoneNumber: string;
  motherName: string;
}

export interface PatientSearchResult {
  id: string;
  fullName: string;
  susCardNumber: string;
  cpf: string;
  displayText: string;
}