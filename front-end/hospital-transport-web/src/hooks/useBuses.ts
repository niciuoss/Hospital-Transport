import { useState } from 'react';
import  api  from '@/lib/api';
import { Bus } from '@/types/bus';
import { BaseResponse } from '@/types/common';

export function useBuses() {
  const [loading, setLoading] = useState(false);

  const getBuses = async (): Promise<Bus[]> => {
    try {
      setLoading(true);
      const response = await api.get<BaseResponse<Bus[]>>('/buses');
      return response.data.data || [];
    } catch (error) {
      console.error('Erro ao buscar ônibus:', error);
      return [];
    } finally {
      setLoading(false);
    }
  };

  return {
    getBuses,
    loading,
  };
}