'use client';

import { useState } from 'react';
import { Appointment } from '@/types/appointment';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Download, Eye, Trash2 } from 'lucide-react';
import { formatDateTime, formatCPF } from '@/lib/utils';

interface AppointmentCardProps {
  appointment: Appointment;
  onDownload: (id: string) => void;
  onDelete: (id: string) => void;
}

export function AppointmentCard({ appointment, onDownload, onDelete }: AppointmentCardProps) {
  const [showDetails, setShowDetails] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);

  const handleDelete = () => {
    onDelete(appointment.id);
    setShowDeleteDialog(false);
  };

  return (
    <>
      <Card className="hover:shadow-md transition-shadow">
        <CardContent className="p-4">
          <div className="flex items-center justify-between gap-4">
            {/* Informações básicas */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <h3 className="font-semibold truncate">{appointment.patient.fullName}</h3>
                {appointment.isPriority && (
                  <Badge variant="destructive" className="text-xs">Prioritário</Badge>
                )}
                {appointment.isTicketPrinted && (
                  <Badge variant="secondary" className="text-xs">Impresso</Badge>
                )}
              </div>
              <p className="text-sm text-muted-foreground">
                CPF: {formatCPF(appointment.patient.cpf)}
              </p>
              <p className="text-sm text-muted-foreground">
                SUS: {appointment.patient.susCardNumber}
              </p>
            </div>

            {/* Ações */}
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setShowDetails(true)}
                title="Ver detalhes"
              >
                <Eye className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => onDownload(appointment.id)}
                title="Baixar PDF"
              >
                <Download className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setShowDeleteDialog(true)}
                title="Cancelar agendamento"
                className="text-destructive hover:text-destructive"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Dialog de Detalhes */}
      <Dialog open={showDetails} onOpenChange={setShowDetails}>
        <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Detalhes do Agendamento</DialogTitle>
            <DialogDescription>
              Informações completas da viagem
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-6">
            {/* Dados do Paciente */}
            <div>
              <h3 className="font-semibold mb-3 flex items-center gap-2">
                Dados do Paciente
                <div className="flex gap-2">
                  {appointment.isPriority && (
                    <Badge variant="destructive">Prioritário</Badge>
                  )}
                  {appointment.isTicketPrinted && (
                    <Badge variant="secondary">PDF Impresso</Badge>
                  )}
                </div>
              </h3>
              <div className="grid gap-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Nome:</span>
                  <span className="font-medium">{appointment.patient.fullName}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">CPF:</span>
                  <span className="font-medium">{formatCPF(appointment.patient.cpf)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Cartão SUS:</span>
                  <span className="font-medium">{appointment.patient.susCardNumber}</span>
                </div>
              </div>
            </div>

            {/* Dados do Agendamento */}
            <div className="border-t pt-4">
              <h3 className="font-semibold mb-3">Dados do Agendamento</h3>
              <div className="grid gap-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Prontuário:</span>
                  <span className="font-medium">{appointment.medicalRecordNumber}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Destino:</span>
                  <span className="font-medium">{appointment.destinationHospital}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Tipo de Tratamento:</span>
                  <span className="font-medium">
                    {appointment.treatmentType}
                    {appointment.treatmentTypeOther && ` - ${appointment.treatmentTypeOther}`}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Data da Viagem:</span>
                  <span className="font-medium">{formatDateTime(appointment.appointmentDate)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Poltrona:</span>
                  <span className="font-medium text-lg">{appointment.seatNumber}</span>
                </div>
              </div>
            </div>

            {/* Acompanhante */}
            {appointment.companion && (
              <div className="border-t pt-4">
                <h3 className="font-semibold mb-3">Acompanhante</h3>
                <div className="grid gap-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Nome:</span>
                    <span className="font-medium">{appointment.companion.fullName}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">CPF:</span>
                    <span className="font-medium">{formatCPF(appointment.companion.cpf)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Poltrona:</span>
                    <span className="font-medium text-lg">{appointment.companionSeatNumber}</span>
                  </div>
                </div>
              </div>
            )}

            {/* Informações do Sistema */}
            <div className="border-t pt-4">
              <h3 className="font-semibold mb-3">Informações do Sistema</h3>
              <div className="grid gap-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Criado por:</span>
                  <span className="font-medium">{appointment.createdByUserName}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Data de criação:</span>
                  <span className="font-medium">{formatDateTime(appointment.createdAt)}</span>
                </div>
              </div>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Dialog de Confirmação de Exclusão */}
      <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cancelar Agendamento</AlertDialogTitle>
            <AlertDialogDescription>
              Tem certeza que deseja cancelar o agendamento de <strong>{appointment.patient.fullName}</strong>?
              Esta ação não pode ser desfeita.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Não, manter agendamento</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Sim, cancelar agendamento
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}