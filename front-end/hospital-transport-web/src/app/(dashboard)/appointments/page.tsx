"use client";

import { useEffect, useState } from "react";
import { useAppointments } from "@/hooks/useAppointments";
import { Appointment } from "@/types/appointment";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Plus, Download, Calendar, Filter } from "lucide-react";
import Link from "next/link";
import { formatDateTime } from "@/lib/utils";

export default function AppointmentsPage() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [filteredAppointments, setFilteredAppointments] = useState<
    Appointment[]
  >([]);
  const { getAppointments, downloadTicket, loading } = useAppointments();

  const [filters, setFilters] = useState({
    search: "",
    isPriority: "all",
    treatmentType: "all",
    dateFrom: "",
    dateTo: "",
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

    // Filtro de busca
    if (filters.search) {
      filtered = filtered.filter(
        (a) =>
          a.patient.fullName
            .toLowerCase()
            .includes(filters.search.toLowerCase()) ||
          a.destinationHospital
            .toLowerCase()
            .includes(filters.search.toLowerCase()) ||
          a.medicalRecordNumber.includes(filters.search)
      );
    }

    // Filtro de prioridade
    if (filters.isPriority !== "all") {
      filtered = filtered.filter(
        (a) => a.isPriority === (filters.isPriority === "true")
      );
    }

    // Filtro de tipo de tratamento
    if (filters.treatmentType !== "all") {
      filtered = filtered.filter(
        (a) => a.treatmentType === filters.treatmentType
      );
    }

    // Filtro de data
    if (filters.dateFrom) {
      filtered = filtered.filter(
        (a) => new Date(a.appointmentDate) >= new Date(filters.dateFrom)
      );
    }

    if (filters.dateTo) {
      filtered = filtered.filter(
        (a) =>
          new Date(a.appointmentDate) <= new Date(filters.dateTo + "T23:59:59")
      );
    }

    setFilteredAppointments(filtered);
  };

  const clearFilters = () => {
    setFilters({
      search: "",
      isPriority: "all",
      treatmentType: "all",
      dateFrom: "",
      dateTo: "",
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Agendamentos</h1>
          <p className="text-muted-foreground">
            Gerencie os agendamentos de transporte
          </p>
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
                onChange={(e) =>
                  setFilters({ ...filters, search: e.target.value })
                }
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Prioridade</label>
              <Select
                value={filters.isPriority}
                onValueChange={(value) =>
                  setFilters({ ...filters, isPriority: value })
                }
              >
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
              <Select
                value={filters.treatmentType}
                onValueChange={(value) =>
                  setFilters({ ...filters, treatmentType: value })
                }
              >
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
                onChange={(e) =>
                  setFilters({ ...filters, dateFrom: e.target.value })
                }
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Data Final</label>
              <Input
                type="date"
                value={filters.dateTo}
                onChange={(e) =>
                  setFilters({ ...filters, dateTo: e.target.value })
                }
              />
            </div>

            <div className="flex items-end">
              <Button
                variant="outline"
                onClick={clearFilters}
                className="w-full"
              >
                Limpar Filtros
              </Button>
            </div>
          </div>

          <div className="mt-4 text-sm text-muted-foreground">
            Mostrando {filteredAppointments.length} de {appointments.length}{" "}
            agendamentos
          </div>
        </CardContent>
      </Card>

      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">
            Carregando agendamentos...
          </p>
        </div>
      ) : filteredAppointments.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Calendar className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
            <p className="text-muted-foreground">
              Nenhum agendamento encontrado
            </p>
            {appointments.length > 0 ? (
              <Button variant="outline" onClick={clearFilters} className="mt-4">
                Limpar Filtros
              </Button>
            ) : (
              <Link href="/appointments/new">
                <Button className="mt-4">Criar Primeiro Agendamento</Button>
              </Link>
            )}
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {filteredAppointments.map((appointment) => (
            <Card key={appointment.id}>
              <CardHeader>
                <div className="flex justify-between items-start">
                  <div>
                    <CardTitle>{appointment.patient.fullName}</CardTitle>
                    <p className="text-sm text-muted-foreground mt-1">
                      CPF: {appointment.patient.cpf} | SUS:{" "}
                      {appointment.patient.susCardNumber}
                    </p>
                  </div>
                  <div className="flex gap-2">
                    {appointment.isPriority && (
                      <Badge variant="destructive">Prioritário</Badge>
                    )}
                    {appointment.isTicketPrinted && (
                      <Badge variant="secondary">Impresso</Badge>
                    )}
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <div>
                      <span className="font-medium text-sm">Prontuário:</span>
                      <p className="text-sm text-muted-foreground">
                        {appointment.medicalRecordNumber}
                      </p>
                    </div>
                    <div>
                      <span className="font-medium text-sm">Destino:</span>
                      <p className="text-sm text-muted-foreground">
                        {appointment.destinationHospital}
                      </p>
                    </div>
                    <div>
                      <span className="font-medium text-sm">
                        Tipo de Tratamento:
                      </span>
                      <p className="text-sm text-muted-foreground">
                        {appointment.treatmentType}
                        {appointment.treatmentTypeOther &&
                          ` - ${appointment.treatmentTypeOther}`}
                      </p>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div>
                      <span className="font-medium text-sm">
                        Data da Viagem:
                      </span>
                      <p className="text-sm text-muted-foreground">
                        {formatDateTime(appointment.appointmentDate)}
                      </p>
                    </div>
                    <div>
                      <span className="font-medium text-sm">Poltrona:</span>
                      <p className="text-sm text-muted-foreground">
                        {appointment.seatNumber}
                      </p>
                    </div>
                    {appointment.companion && (
                      <div>
                        <span className="font-medium text-sm">
                          Acompanhante:
                        </span>
                        <p className="text-sm text-muted-foreground">
                          {appointment.companion.fullName} - Poltrona{" "}
                          {appointment.companionSeatNumber}
                        </p>
                      </div>
                    )}
                    <div>
                      <span className="font-medium text-sm">Criado por:</span>
                      <p className="text-sm text-muted-foreground">
                        {appointment.createdByUserName}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="mt-4 flex justify-end">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => downloadTicket(appointment.id)}
                  >
                    <Download className="h-4 w-4 mr-2" />
                    Baixar Passagem (PDF)
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
