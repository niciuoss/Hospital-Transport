'use client';

import { useEffect, useState } from 'react';
import { useAppointments } from '@/hooks/useAppointments';
import { Appointment } from '@/types/appointment';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Pagination, PaginationContent, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from '@/components/ui/pagination';
import { AppointmentCard } from '@/components/appointments/AppointmentCard';
import { Plus, Calendar, Filter } from 'lucide-react';
import Link from 'next/link';

const ITEMS_PER_PAGE = 12;

export default function AppointmentsPage() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [filteredAppointments, setFilteredAppointments] = useState<Appointment[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const { getAppointments, downloadTicket, deleteAppointment, loading } = useAppointments();

  const [filters, setFilters] = useState({
    search: '',
    isPriority: 'all',
    treatmentType: 'all',
    dateFrom: '',
    dateTo: '',
  });

  useEffect(() => {
    loadAppointments();
  }, []);

  useEffect(() => {
    applyFilters();
  }, [filters, appointments]);

  const loadAppointments = async () => {
    const data = await getAppointments();
    setAppointments(data);
  };

  const applyFilters = () => {
    let filtered = [...appointments];

    if (filters.search) {
      filtered = filtered.filter(a =>
        a.patient.fullName.toLowerCase().includes(filters.search.toLowerCase()) ||
        a.destinationHospital.toLowerCase().includes(filters.search.toLowerCase()) ||
        a.medicalRecordNumber.includes(filters.search)
      );
    }

    if (filters.isPriority !== 'all') {
      filtered = filtered.filter(a => 
        a.isPriority === (filters.isPriority === 'true')
      );
    }

    if (filters.treatmentType !== 'all') {
      filtered = filtered.filter(a => a.treatmentType === filters.treatmentType);
    }

    if (filters.dateFrom) {
      filtered = filtered.filter(a => 
        new Date(a.appointmentDate) >= new Date(filters.dateFrom)
      );
    }

    if (filters.dateTo) {
      filtered = filtered.filter(a => 
        new Date(a.appointmentDate) <= new Date(filters.dateTo + 'T23:59:59')
      );
    }

    setFilteredAppointments(filtered);
    setCurrentPage(1);
  };

  const clearFilters = () => {
    setFilters({
      search: '',
      isPriority: 'all',
      treatmentType: 'all',
      dateFrom: '',
      dateTo: '',
    });
  };

  const handleDelete = async (id: string) => {
    const success = await deleteAppointment(id);
    if (success) {
      loadAppointments();
    }
  };

  const totalPages = Math.ceil(filteredAppointments.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const endIndex = startIndex + ITEMS_PER_PAGE;
  const currentAppointments = filteredAppointments.slice(startIndex, endIndex);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Agendamentos</h1>
          <p className="text-muted-foreground">Gerencie os agendamentos de transporte</p>
        </div>
        <Link href="/appointments/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Novo Agendamento
          </Button>
        </Link>
      </div>

      {/* Filtros */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            <div className="space-y-2">
              <label className="text-sm font-medium">Buscar</label>
              <Input
                placeholder="Paciente, hospital ou prontuário..."
                value={filters.search}
                onChange={(e) => setFilters({ ...filters, search: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Prioridade</label>
              <Select value={filters.isPriority} onValueChange={(value) => setFilters({ ...filters, isPriority: value })}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="true">Prioritários</SelectItem>
                  <SelectItem value="false">Não Prioritários</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Tipo de Tratamento</label>
              <Select value={filters.treatmentType} onValueChange={(value) => setFilters({ ...filters, treatmentType: value })}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="Semanal">Semanal</SelectItem>
                  <SelectItem value="Mensal">Mensal</SelectItem>
                  <SelectItem value="Trimestral">Trimestral</SelectItem>
                  <SelectItem value="Outro">Outro</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Data Inicial</label>
              <Input
                type="date"
                value={filters.dateFrom}
                onChange={(e) => setFilters({ ...filters, dateFrom: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Data Final</label>
              <Input
                type="date"
                value={filters.dateTo}
                onChange={(e) => setFilters({ ...filters, dateTo: e.target.value })}
              />
            </div>

            <div className="flex items-end">
              <Button variant="outline" onClick={clearFilters} className="w-full">
                Limpar Filtros
              </Button>
            </div>
          </div>

          <div className="mt-4 text-sm text-muted-foreground">
            Mostrando {currentAppointments.length} de {filteredAppointments.length} agendamentos
          </div>
        </CardContent>
      </Card>

      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Carregando agendamentos...</p>
        </div>
      ) : currentAppointments.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Calendar className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
            <p className="text-muted-foreground">Nenhum agendamento encontrado</p>
            {filteredAppointments.length === 0 && appointments.length > 0 ? (
              <Button variant="outline" onClick={clearFilters} className="mt-4">
                Limpar Filtros
              </Button>
            ) : appointments.length === 0 ? (
              <Link href="/appointments/new">
                <Button className="mt-4">Criar Primeiro Agendamento</Button>
              </Link>
            ) : null}
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-3">
            {currentAppointments.map((appointment) => (
              <AppointmentCard
                key={appointment.id}
                appointment={appointment}
                onDownload={downloadTicket}
                onDelete={handleDelete}
              />
            ))}
          </div>

          {totalPages > 1 && (
            <Pagination>
              <PaginationContent>
                <PaginationItem>
                  <PaginationPrevious
                    onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                    className={currentPage === 1 ? 'pointer-events-none opacity-50' : 'cursor-pointer'}
                  />
                </PaginationItem>
                
                {Array.from({ length: Math.min(totalPages, 7) }, (_, i) => {
                  let pageNum;
                  if (totalPages <= 7) {
                    pageNum = i + 1;
                  } else if (currentPage <= 4) {
                    pageNum = i + 1;
                  } else if (currentPage >= totalPages - 3) {
                    pageNum = totalPages - 6 + i;
                  } else {
                    pageNum = currentPage - 3 + i;
                  }
                  
                  return (
                    <PaginationItem key={pageNum}>
                      <PaginationLink
                        onClick={() => setCurrentPage(pageNum)}
                        isActive={currentPage === pageNum}
                        className="cursor-pointer"
                      >
                        {pageNum}
                      </PaginationLink>
                    </PaginationItem>
                  );
                })}
                
                <PaginationItem>
                  <PaginationNext
                    onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                    className={currentPage === totalPages ? 'pointer-events-none opacity-50' : 'cursor-pointer'}
                  />
                </PaginationItem>
              </PaginationContent>
            </Pagination>
          )}
        </>
      )}
    </div>
  );
}