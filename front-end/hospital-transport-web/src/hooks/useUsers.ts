import { useState } from 'react';
import api from '@/lib/api';
import { UserData, CreateUserRequest } from '@/types/user';
import { BaseResponse } from '@/types/common';
import { toast } from 'sonner';

export function useUsers() {
  const [loading, setLoading] = useState(false);

  const getUsers = async (): Promise<UserData[]> => {
    try {
      setLoading(true);
      const response = await api.get<BaseResponse<UserData[]>>('/users');
      return response.data.data || [];
    } catch (error: any) {
      toast.error('Erro ao buscar usuários');
      return [];
    } finally {
      setLoading(false);
    }
  };

  const createUser = async (data: CreateUserRequest): Promise<UserData | null> => {
    try {
      setLoading(true);
      const response = await api.post<BaseResponse<UserData>>('/users', data);
      if (response.data.success) {
        toast.success('Usuário cadastrado com sucesso!');
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

  const updateUser = async (id: string, data: CreateUserRequest): Promise<UserData | null> => {
  try {
    setLoading(true);
    const response = await api.put<BaseResponse<UserData>>(`/users/${id}`, { ...data, id });
    if (response.data.success) {
      toast.success('Usuário atualizado com sucesso!');
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

  const getUserById = async (id: string): Promise<UserData | null> => {
    try {
      const response = await api.get<BaseResponse<UserData>>(`/users/${id}`);
      return response.data.data;
    } catch (error: any) {
      toast.error('Erro ao buscar usuário');
      return null;
    }
  };

  return { loading, getUsers, createUser, updateUser, getUserById };
}