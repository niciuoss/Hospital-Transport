"use client";

import { useEffect, useState } from "react";
import { useAppointments } from "@/hooks/useAppointments";
import { usePatients } from "@/hooks/usePatients";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
  Calendar,
  Users,
  Plus,
  Download,
  TrendingUp,
  Activity,
} from "lucide-react";
import Link from "next/link";
import { formatDateTime } from "@/lib/utils";
import { Appointment } from "@/types/appointment";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieLabelRenderProps,
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line,
  Legend,
} from "recharts";

const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#8884D8"];

export default function DashboardPage() {
  const [recentAppointments, setRecentAppointments] = useState<Appointment[]>(
    []
  );
  const [allAppointments, setAllAppointments] = useState<Appointment[]>([]);
  const [stats, setStats] = useState({
    patients: 0,
    appointments: 0,
    thisMonth: 0,
    priority: 0,
  });
  const { getRecentAppointments, getAppointments, downloadTicket } =
    useAppointments();
  const { getPatients } = usePatients();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    const [appointments, allApp, patients] = await Promise.all([
      getRecentAppointments(5),
      getAppointments(),
      getPatients(),
    ]);

    setRecentAppointments(appointments);
    setAllAppointments(allApp);

    const thisMonth = allApp.filter((a) => {
      const date = new Date(a.appointmentDate);
      const now = new Date();
      return (
        date.getMonth() === now.getMonth() &&
        date.getFullYear() === now.getFullYear()
      );
    }).length;

    const priority = allApp.filter((a) => a.isPriority).length;

    setStats({
      patients: patients.length,
      appointments: allApp.length,
      thisMonth,
      priority,
    });
  };

  // Dados para gráfico de tratamentos
  const treatmentData = [
    {
      name: "Semanal",
      value: allAppointments.filter((a) => a.treatmentType === "Semanal")
        .length,
    },
    {
      name: "Mensal",
      value: allAppointments.filter((a) => a.treatmentType === "Mensal").length,
    },
    {
      name: "Trimestral",
      value: allAppointments.filter((a) => a.treatmentType === "Trimestral")
        .length,
    },
    {
      name: "Outro",
      value: allAppointments.filter((a) => a.treatmentType === "Outro").length,
    },
  ].filter((item) => item.value > 0);

  // Dados para gráfico mensal (últimos 6 meses)
  const monthlyData = Array.from({ length: 6 }, (_, i) => {
    const date = new Date();
    date.setMonth(date.getMonth() - i);
    const month = date.toLocaleDateString("pt-BR", { month: "short" });
    const count = allAppointments.filter((a) => {
      const aDate = new Date(a.appointmentDate);
      return (
        aDate.getMonth() === date.getMonth() &&
        aDate.getFullYear() === date.getFullYear()
      );
    }).length;
    return {
      month: month.charAt(0).toUpperCase() + month.slice(1),
      total: count,
    };
  }).reverse();
  // Dados para gráfico de destinos
  const destinationCount: Record<string, number> = {};
  allAppointments.forEach((a) => {
    destinationCount[a.destinationHospital] =
      (destinationCount[a.destinationHospital] || 0) + 1;
  });
  const destinationData = Object.entries(destinationCount)
    .map(([name, value]) => ({ name, value }))
    .sort((a, b) => b.value - a.value)
    .slice(0, 5);
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Visão geral do sistema</p>
      </div>
      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Total de Pacientes
            </CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.patients}</div>
            <p className="text-xs text-muted-foreground mt-1">
              Cadastrados no sistema
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Total de Agendamentos
            </CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.appointments}</div>
            <p className="text-xs text-muted-foreground mt-1">
              Todos os períodos
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Este Mês</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.thisMonth}</div>
            <p className="text-xs text-muted-foreground mt-1">
              Agendamentos em{" "}
              {new Date().toLocaleDateString("pt-BR", { month: "long" })}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Prioritários</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.priority}</div>
            <p className="text-xs text-muted-foreground mt-1">
              {stats.appointments > 0
                ? ((stats.priority / stats.appointments) * 100).toFixed(1)
                : 0}
              % do total
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Ações Rápidas</CardTitle>
        </CardHeader>
        <CardContent className="flex gap-4 flex-wrap">
          <Link href="/patients/new">
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Novo Paciente
            </Button>
          </Link>
          <Link href="/appointments/new">
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Novo Agendamento
            </Button>
          </Link>
          <Link href="/users/new">
            <Button variant="outline">
              <Plus className="h-4 w-4 mr-2" />
              Novo Usuário
            </Button>
          </Link>
          <Link href="/reports">
            <Button variant="outline">
              <Calendar className="h-4 w-4 mr-2" />
              Ver Relatórios
            </Button>
          </Link>
        </CardContent>
      </Card>

      {/* Gráficos */}
      <div className="grid gap-4 md:grid-cols-2">
        {/* Gráfico de Tipos de Tratamento */}
        <Card>
          <CardHeader>
            <CardTitle>Agendamentos por Tipo de Tratamento</CardTitle>
          </CardHeader>
          <CardContent>
            {treatmentData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={treatmentData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent }: PieLabelRenderProps) =>
                      `${name}: ${(Number(percent ?? 0) * 100).toFixed(0)}%`
                    }
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {treatmentData.map((entry, index) => (
                      <Cell
                        key={`cell-${index}`}
                        fill={COLORS[index % COLORS.length]}
                      />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <p className="text-center text-muted-foreground py-12">
                Nenhum dado disponível
              </p>
            )}
          </CardContent>
        </Card>

        {/* Gráfico de Top Destinos */}
        <Card>
          <CardHeader>
            <CardTitle>Top 5 Destinos</CardTitle>
          </CardHeader>
          <CardContent>
            {destinationData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={destinationData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="name"
                    angle={-45}
                    textAnchor="end"
                    height={100}
                    interval={0}
                  />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="value" fill="#8884d8" name="Agendamentos" />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <p className="text-center text-muted-foreground py-12">
                Nenhum dado disponível
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Gráfico de Evolução Mensal */}
      <Card>
        <CardHeader>
          <CardTitle>Evolução de Agendamentos (Últimos 6 Meses)</CardTitle>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={monthlyData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line
                type="monotone"
                dataKey="total"
                stroke="#8884d8"
                name="Agendamentos"
                strokeWidth={2}
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Recent Appointments */}
      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <CardTitle>Últimos Agendamentos</CardTitle>
            <Link href="/appointments">
              <Button variant="outline" size="sm">
                Ver Todos
              </Button>
            </Link>
          </div>
        </CardHeader>
        <CardContent>
          {recentAppointments.length === 0 ? (
            <p className="text-muted-foreground text-center py-4">
              Nenhum agendamento encontrado
            </p>
          ) : (
            <div className="space-y-4">
              {recentAppointments.map((appointment) => (
                <div
                  key={appointment.id}
                  className="flex items-center justify-between p-4 border rounded-lg hover:bg-muted/50 transition-colors"
                >
                  <div className="flex-1">
                    <p className="font-medium">
                      {appointment.patient.fullName}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      Destino: {appointment.destinationHospital}
                    </p>
                    <div className="flex gap-4 mt-1">
                      <p className="text-sm text-muted-foreground">
                        Poltrona: {appointment.seatNumber}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        {formatDateTime(appointment.appointmentDate)}
                      </p>
                    </div>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => downloadTicket(appointment.id)}
                  >
                    <Download className="h-4 w-4 mr-2" />
                    PDF
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
