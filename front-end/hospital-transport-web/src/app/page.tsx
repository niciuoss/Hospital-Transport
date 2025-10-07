'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ArrowRight, Calendar, Users, FileText } from 'lucide-react';

export default function HomePage() {
  const router = useRouter();
  const { user, loading } = useAuth();

  useEffect(() => {
    // loading = dashboard
    if (!loading && user) {
      router.push('/dashboard');
    }
  }, [user, loading, router]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Carregando...</p>
        </div>
      </div>
    );
  }

  //!user = landing page
  if (!user) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800">
        <div className="container mx-auto px-4 py-16">
          <div className="text-center mb-16">
            <h1 className="text-5xl font-bold text-primary mb-4">SATH</h1>
            <p className="text-xl text-muted-foreground mb-8">
              Sistema de Agendamento de Transporte Hospitalar
            </p>
            <Button size="lg" onClick={() => router.push('/login')}>
              Acessar Sistema
              <ArrowRight className="ml-2 h-5 w-5" />
            </Button>
          </div>

          <div className="grid gap-6 md:grid-cols-3 max-w-5xl mx-auto">
            <Card>
              <CardHeader>
                <Users className="h-10 w-10 text-primary mb-2" />
                <CardTitle>Gestão de Pacientes</CardTitle>
                <CardDescription>
                  Cadastre e gerencie pacientes de forma simples e organizada
                </CardDescription>
              </CardHeader>
            </Card>

            <Card>
              <CardHeader>
                <Calendar className="h-10 w-10 text-primary mb-2" />
                <CardTitle>Agendamentos</CardTitle>
                <CardDescription>
                  Sistema inteligente de seleção de poltronas e controle de viagens
                </CardDescription>
              </CardHeader>
            </Card>

            <Card>
              <CardHeader>
                <FileText className="h-10 w-10 text-primary mb-2" />
                <CardTitle>Relatórios</CardTitle>
                <CardDescription>
                  Acompanhe estatísticas e gere relatórios detalhados
                </CardDescription>
              </CardHeader>
            </Card>
          </div>

          <div className="text-center mt-16 text-sm text-muted-foreground">
            <p>Sistema desenvolvido para otimizar o transporte de pacientes</p>
            <p className="mt-2">© 2025 Hospital Transport - Todos os direitos reservados</p>
          </div>
        </div>
      </div>
    );
  }

  return null;
}