"use client";

import { useState } from "react";
import { useAppointments } from "@/hooks/useAppointments";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { FileText } from "lucide-react";
import { toast } from "sonner";

export default function DestinationsReportPage() {
  const { downloadDestinationsReport } = useAppointments();
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");

  const handleDownload = () => {
    if (!dateFrom || !dateTo) {
      toast.error("Selecione a data inicial e final");
      return;
    }
    downloadDestinationsReport(dateFrom, dateTo);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Destinos</h1>
        <p className="text-muted-foreground">Relatório dos destinos mais frequentes</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Relatório de Destinos</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Gera um PDF com os principais destinos em ordem alfabética e o número de viagens para cada um no período selecionado.
          </p>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="space-y-2">
              <Label>Data Inicial</Label>
              <Input type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Data Final</Label>
              <Input type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
            </div>
            <div className="flex items-end">
              <Button onClick={handleDownload} className="w-full">
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
