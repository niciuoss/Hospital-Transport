import { useState } from 'react';
import api from '@/lib/api';
import { Appointment, CreateAppointmentRequest, SeatAvailability } from '@/types/appointment';
import { BaseResponse } from '@/types/common';
import { toast } from 'sonner';

export function useAppointments() {
  const [loading, setLoading] = useState(false);

  const getAppointments = async (): Promise<Appointment[]> => {
    try {
      setLoading(true);
      const response = await api.get<BaseResponse<Appointment[]>>('/appointments');
      return response.data.data || [];
    } catch (error: any) {
      toast.error('Erro ao buscar agendamentos');
      return [];
    } finally {
      setLoading(false);
    }
  };

  const getRecentAppointments = async (count: number = 10): Promise<Appointment[]> => {
    try {
      const response = await api.get<BaseResponse<Appointment[]>>('/appointments/recent', {
        params: { count }
      });
      return response.data.data || [];
    } catch (error: any) {
      toast.error('Erro ao buscar agendamentos recentes');
      return [];
    }
  };

  const getSeatAvailability = async (date: string, isPriority: boolean): Promise<SeatAvailability[]> => {
    try {
      const response = await api.get<BaseResponse<SeatAvailability[]>>('/appointments/seat-availability', {
        params: { date, isPriority }
      });
      return response.data.data || [];
    } catch (error: any) {
      toast.error('Erro ao buscar disponibilidade de poltronas');
      return [];
    }
  };

  const createAppointment = async (data: CreateAppointmentRequest): Promise<Appointment | null> => {
    try {
      setLoading(true);
      const response = await api.post<BaseResponse<Appointment>>('/appointments', data);
      if (response.data.success) {
        toast.success('Agendamento criado com sucesso!');
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

  const downloadTicket = async (appointmentId: string) => {
    try {
      const response = await api.get(`/appointments/${appointmentId}/ticket`, {
        responseType: 'blob'
      });
      
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `passagem_${appointmentId}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      
      toast.success('PDF baixado com sucesso!');
    } catch (error: any) {
      toast.error('Erro ao baixar PDF');
    }
  };

  return { 
    loading, 
    getAppointments, 
    getRecentAppointments, 
    getSeatAvailability, 
    createAppointment,
    downloadTicket 
  };
}