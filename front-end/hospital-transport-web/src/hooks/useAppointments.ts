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

  const deleteAppointment = async (id: string): Promise<boolean> => {
    try {
      setLoading(true);
      const response = await api.delete(`/appointments/${id}`);
      if (response.status === 200 || response.status === 204) {
        toast.success('Agendamento cancelado com sucesso!');
        return true;
      }
      return false;
    } catch (error: any) {
      toast.error('Erro ao cancelar agendamento');
      return false;
    } finally {
      setLoading(false);
    }
  };

  const downloadPassengerList = async (date: string) => {
    try {
      const response = await api.get('/appointments/passenger-list-pdf', {
        params: { date },
        responseType: 'blob'
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `lista_passageiros_${date}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();

      toast.success('Lista de passageiros baixada com sucesso!');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Erro ao baixar lista de passageiros');
    }
  };

  const downloadAnnualReport = async (year: number) => {
    try {
      const response = await api.get('/appointments/annual-report-pdf', {
        params: { year },
        responseType: 'blob'
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `relatorio_anual_${year}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();

      toast.success('Relatório anual baixado com sucesso!');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Erro ao baixar relatório anual');
    }
  };

  return {
    loading,
    getAppointments,
    getRecentAppointments,
    getSeatAvailability,
    createAppointment,
    downloadTicket,
    deleteAppointment,
    downloadPassengerList,
    downloadAnnualReport
  };
}