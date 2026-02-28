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

const createPatient = async (data: CreatePatientRequest) => {
  try {
    const response = await api.post('/patients', data);
    
    if (response.data.success) {
      toast.success(response.data.message || 'Paciente cadastrado com sucesso!');
      return response.data.data;
    }
    
    // Se success: false no response
    toast.error(response.data.message || 'Erro ao cadastrar paciente');
    
    // Mostrar erros específicos se houver
    if (response.data.errors && response.data.errors.length > 0) {
      response.data.errors.forEach((error: string) => {
        toast.error(error);
      });
    }
    
    return null;
  } catch (error: any) {
    console.error('Erro ao cadastrar paciente:', error);
    
    // Capturar mensagem de erro do back-end
    const errorMessage = error.response?.data?.message || 
                        error.response?.data?.errors?.[0] ||
                        'Erro ao cadastrar paciente';
    
    toast.error(errorMessage);
    
    return null;
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