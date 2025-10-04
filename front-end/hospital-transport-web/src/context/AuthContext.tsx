'use client'

import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import api from '@/lib/api';
import { User, LoginRequest, LoginResponse } from '@/types/auth';
import { BaseResponse } from '@/types/common';
import { toast } from 'sonner';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const storedUser = localStorage.getItem('user');
    const token = localStorage.getItem('token');
    
    if (storedUser && token) {
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, []);

  const login = async (credentials: LoginRequest) => {
    try {
      const response = await api.post<BaseResponse<LoginResponse>>('/auth/login', credentials);
      
      if (response.data.success && response.data.data) {
        const userData: User = {
          userId: response.data.data.userId,
          fullName: response.data.data.fullName,
          username: response.data.data.username,
          token: response.data.data.token,
        };

        localStorage.setItem('user', JSON.stringify(userData));
        localStorage.setItem('token', userData.token);
        setUser(userData);
        toast.success('Login realizado com sucesso!');
        router.push('/dashboard');
      } else {
        toast.error(response.data.message || 'Erro ao fazer login');
      }
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Erro ao fazer login');
      throw error;
    }
  };

  const logout = () => {
    localStorage.removeItem('user');
    localStorage.removeItem('token');
    setUser(null);
    router.push('/login');
    toast.info('Logout realizado com sucesso');
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};