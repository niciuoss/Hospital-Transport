'use client';

import { useEffect, useState } from 'react';
import { useAppointments } from '@/hooks/useAppointments';
import { usePatients } from '@/hooks/usePatients';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { FileText, Calendar, Users, TrendingUp } from 'lucide-react';
import { Appointment } from '@/types/appointment';
import { toast } from 'sonner';

export default function ReportsPage() {
  const { getAppointments, downloadPassengerList } = useAppointments();
  const { getPatients } = usePatients();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [filteredAppointments, setFilteredAppointments] = useState<Appointment[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    filterByDate();
  }, [dateFrom, dateTo, appointments]);

  const loadData = async () => {
    const appointmentsData = await getAppointments();
    setAppointments(appointmentsData);
  };

  const filterByDate = () => {
    let filtered = [...appointments];

    if (dateFrom) {
      filtered = filtered.filter(a => 
        new Date(a.appointmentDate) >= new Date(dateFrom)
      );
    }

    if (dateTo) {
      filtered = filtered.filter(a => 
        new Date(a.appointmentDate) <= new Date(dateTo + 'T23:59:59')
      );
    }

    setFilteredAppointments(filtered);
  };

  const stats = {
    total: filteredAppointments.length,
    priority: filteredAppointments.filter(a => a.isPriority).length,
    withCompanion: filteredAppointments.filter(a => a.companion).length,
    byTreatment: {
      Semanal: filteredAppointments.filter(a => a.treatmentType === 'Semanal').length,
      Mensal: filteredAppointments.filter(a => a.treatmentType === 'Mensal').length,
      Trimestral: filteredAppointments.filter(a => a.treatmentType === 'Trimestral').length,
      Outro: filteredAppointments.filter(a => a.treatmentType === 'Outro').length,
    },
    byDestination: {} as Record<string, number>
  };

  filteredAppointments.forEach(a => {
    stats.byDestination[a.destinationHospital] = (stats.byDestination[a.destinationHospital] || 0) + 1;
  });

  const topDestinations = Object.entries(stats.byDestination)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5);

  const handleDownloadPdf = () => {
    if (!dateFrom) {
      toast.error('Selecione uma data para gerar a lista de passageiros');
      return;
    }
    downloadPassengerList(dateFrom);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Relatórios</h1>
        <p className="text-muted-foreground">Análise e estatísticas dos agendamentos</p>
      </div>

      {/* Filtro de Período */}
      <Card>
        <CardHeader>
          <CardTitle>Período do Relatório</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <Label>Data Inicial</Label>
              <Input
                type="date"
                value={dateFrom}
                onChange={(e) => setDateFrom(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Data Final</Label>
              <Input
                type="date"
                value={dateTo}
                onChange={(e) => setDateTo(e.target.value)}
              />
            </div>
            <div className="flex items-end">
              <Button onClick={handleDownloadPdf} className="w-full">
                <FileText className="h-4 w-4 mr-2" />
                Gerar Lista de Passageiros (PDF)
              </Button>
            </div>
          </div>
          <p className="text-sm text-muted-foreground mt-4">
            * A lista de passageiros será gerada apenas para a <strong>Data Inicial</strong> selecionada
          </p>
        </CardContent>
      </Card>

      {/* Estatísticas Gerais */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total de Agendamentos</CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Prioritários</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.priority}</div>
            <p className="text-xs text-muted-foreground">
              {stats.total > 0 ? ((stats.priority / stats.total) * 100).toFixed(1) : 0}% do total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Com Acompanhante</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.withCompanion}</div>
            <p className="text-xs text-muted-foreground">
              {stats.total > 0 ? ((stats.withCompanion / stats.total) * 100).toFixed(1) : 0}% do total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Destinos Únicos</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{Object.keys(stats.byDestination).length}</div>
          </CardContent>
        </Card>
      </div>

      {/* Por Tipo de Tratamento */}
      <Card>
        <CardHeader>
          <CardTitle>Agendamentos por Tipo de Tratamento</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {Object.entries(stats.byTreatment).map(([type, count]) => (
              <div key={type} className="flex items-center justify-between">
                <div className="flex items-center gap-4 flex-1">
                  <span className="font-medium min-w-[100px]">{type}</span>
                  <div className="flex-1 bg-muted rounded-full h-2">
                    <div
                      className="bg-primary h-2 rounded-full transition-all"
                      style={{ width: stats.total > 0 ? `${(count / stats.total) * 100}%` : '0%' }}
                    />
                  </div>
                </div>
                <span className="font-bold ml-4">{count}</span>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Top Destinos */}
      <Card>
        <CardHeader>
          <CardTitle>Top 5 Destinos Mais Frequentes</CardTitle>
        </CardHeader>
        <CardContent>
          {topDestinations.length === 0 ? (
            <p className="text-center text-muted-foreground py-8">
              Nenhum agendamento encontrado no período selecionado
            </p>
          ) : (
            <div className="space-y-4">
              {topDestinations.map(([hospital, count], index) => (
                <div key={hospital} className="flex items-center justify-between">
                  <div className="flex items-center gap-4 flex-1">
                    <span className="font-bold text-2xl text-muted-foreground w-8">#{index + 1}</span>
                    <span className="font-medium">{hospital}</span>
                  </div>
                  <span className="font-bold">{count} agendamentos</span>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}