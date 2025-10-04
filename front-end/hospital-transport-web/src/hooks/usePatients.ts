import { useState } from 'react';
import api from '@/lib/api';
import { Patient, CreatePatientRequest, PatientSearchResult } from '@/types/patient';
import { BaseResponse } from '@/types/common';
import { toast } from 'sonner';

export function usePatients() {
  const [loading, setLoading] = useState(false);

  const getPatients = async (): Promise<Patient[]> => {
    try {
      setLoading(true);
      const response = await api.get<BaseResponse<Patient[]>>('/patients');
      return response.data.data || [];
    } catch (error: any) {
      toast.error('Erro ao buscar pacientes');
      return [];
    } finally {
      setLoading(false);
    }
  };

  const searchPatients = async (searchTerm: string): Promise<PatientSearchResult[]> => {
    try {
      const response = await api.get<BaseResponse<PatientSearchResult[]>>(`/patients/search`, {
        params: { searchTerm }
      });
      return response.data.data || [];
    } catch (error: any) {
      toast.error('Erro ao buscar pacientes');
      return [];
    }
  };

  const createPatient = async (data: CreatePatientRequest): Promise<Patient | null> => {
    try {
      setLoading(true);
      const response = await api.post<BaseResponse<Patient>>('/patients', data);
      if (response.data.success) {
        toast.success('Paciente cadastrado com sucesso!');
        return response.data.data;
      }
      toast.error(response.data.message);
      return null;
    } catch (error: any) {
      const errors = error.response?.data?.errors || [];
      errors.forEach((err: string) => toast.error(err));
      return null;
    } finally {
      setLoading(false);
    }
  };

  const updatePatient = async (id: string, data: CreatePatientRequest): Promise<Patient | null> => {
  try {
    setLoading(true);
    const response = await api.put<BaseResponse<Patient>>(`/patients/${id}`, { ...data, id });
    if (response.data.success) {
      toast.success('Paciente atualizado com sucesso!');
      return response.data.data;
    }
    toast.error(response.data.message);
    return null;
  } catch (error: any) {
    const errors = error.response?.data?.errors || [];
    errors.forEach((err: string) => toast.error(err));
    return null;
  } finally {
    setLoading(false);
  }
};

const getPatientById = async (id: string): Promise<Patient | null> => {
  try {
    const response = await api.get<BaseResponse<Patient>>(`/patients/${id}`);
    return response.data.data;
  } catch (error: any) {
    toast.error('Erro ao buscar paciente');
    return null;
  }
};

return { loading, getPatients, searchPatients, createPatient, updatePatient, getPatientById };
}