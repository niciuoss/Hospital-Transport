"use client";

import { useState } from "react";
import { useAppointments } from "@/hooks/useAppointments";
import { usePatients } from "@/hooks/usePatients";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { FileText } from "lucide-react";
import { toast } from "sonner";

export default function PatientsReportPage() {
  const { downloadPatientsReport, downloadCompanionsReport } = useAppointments();
  const { downloadRegistrationsReport } = usePatients();

  const [periodFrom, setPeriodFrom] = useState("");
  const [periodTo, setPeriodTo] = useState("");
  const [companionFrom, setCompanionFrom] = useState("");
  const [companionTo, setCompanionTo] = useState("");
  const [registrationsFrom, setRegistrationsFrom] = useState("");
  const [registrationsTo, setRegistrationsTo] = useState("");

  const handlePatientsReport = () => {
    if (!periodFrom || !periodTo) {
      toast.error("Selecione a data inicial e final");
      return;
    }
    downloadPatientsReport(periodFrom, periodTo);
  };

  const handleCompanionsReport = () => {
    if (!companionFrom || !companionTo) {
      toast.error("Selecione a data inicial e final");
      return;
    }
    downloadCompanionsReport(companionFrom, companionTo);
  };

  const handleRegistrationsReport = () => {
    if (!registrationsFrom || !registrationsTo) {
      toast.error("Selecione a data inicial e final");
      return;
    }
    downloadRegistrationsReport(registrationsFrom, registrationsTo);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Relatório de Pacientes</h1>
        <p className="text-muted-foreground">Relatórios detalhados sobre pacientes e acompanhantes</p>
      </div>

      {/* Relatório por Período */}
      <Card>
        <CardHeader>
          <CardTitle>Relatório por Período</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Gera um PDF com o total de pacientes que viajaram no período selecionado.
          </p>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <Label>Data Inicial</Label>
              <Input type="date" value={periodFrom} onChange={(e) => setPeriodFrom(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Data Final</Label>
              <Input type="date" value={periodTo} onChange={(e) => setPeriodTo(e.target.value)} />
            </div>
            <div className="flex items-end">
              <Button onClick={handlePatientsReport} className="w-full">
                <FileText className="h-4 w-4 mr-2" />
                Gerar Relatório (PDF)
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Relatório de Acompanhantes */}
      <Card>
        <CardHeader>
          <CardTitle>Relatório de Acompanhantes</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Gera um PDF com quantos acompanhantes viajaram no ônibus no período selecionado.
          </p>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <Label>Data Inicial</Label>
              <Input type="date" value={companionFrom} onChange={(e) => setCompanionFrom(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Data Final</Label>
              <Input type="date" value={companionTo} onChange={(e) => setCompanionTo(e.target.value)} />
            </div>
            <div className="flex items-end">
              <Button onClick={handleCompanionsReport} className="w-full">
                <FileText className="h-4 w-4 mr-2" />
                Gerar Relatório (PDF)
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Total de Cadastros Realizados */}
      <Card>
        <CardHeader>
          <CardTitle>Total de Cadastros Realizados</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Gera um PDF com o total de cadastros de pacientes realizados no período selecionado.
          </p>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <Label>Data Inicial</Label>
              <Input type="date" value={registrationsFrom} onChange={(e) => setRegistrationsFrom(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Data Final</Label>
              <Input type="date" value={registrationsTo} onChange={(e) => setRegistrationsTo(e.target.value)} />
            </div>
            <div className="flex items-end">
              <Button onClick={handleRegistrationsReport} className="w-full">
                <FileText className="h-4 w-4 mr-2" />
                Gerar Relatório (PDF)
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
