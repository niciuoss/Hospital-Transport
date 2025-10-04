'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAppointments } from '@/hooks/useAppointments';
import { usePatients } from '@/hooks/usePatients';
import { useAuth } from '@/context/AuthContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { SeatSelector } from '@/components/appointments/SeatSelector';
import { ArrowLeft } from 'lucide-react';
import Link from 'next/link';
import { PatientSearchResult } from '@/types/patient';
import { SeatAvailability } from '@/types/appointment';

export default function NewAppointmentPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { createAppointment, getSeatAvailability, loading } = useAppointments();
  const { searchPatients } = usePatients();

  const [step, setStep] = useState(1);
  const [patientSearch, setPatientSearch] = useState('');
  const [patientResults, setPatientResults] = useState<PatientSearchResult[]>([]);
  const [companionResults, setCompanionResults] = useState<PatientSearchResult[]>([]);
  const [seats, setSeats] = useState<SeatAvailability[]>([]);
  const [companionSeats, setCompanionSeats] = useState<SeatAvailability[]>([]);

  const [formData, setFormData] = useState({
    patientId: '',
    patientName: '',
    medicalRecordNumber: '',
    destinationHospital: '',
    treatmentType: '1',
    treatmentTypeOther: '',
    isPriority: false,
    seatNumber: null as number | null,
    appointmentDate: '',
    appointmentTime: '08:00',
    hasCompanion: false,
    companionId: '',
    companionName: '',
    companionSeatNumber: null as number | null,
  });

  const handlePatientSearch = async (term: string) => {
    setPatientSearch(term);
    if (term.length >= 3) {
      const results = await searchPatients(term);
      setPatientResults(results);
    } else {
      setPatientResults([]);
    }
  };

  const handleCompanionSearch = async (term: string) => {
    if (term.length >= 3) {
      const results = await searchPatients(term);
      setCompanionResults(results);
    } else {
      setCompanionResults([]);
    }
  };

  const selectPatient = (patient: PatientSearchResult) => {
    setFormData({
      ...formData,
      patientId: patient.id,
      patientName: patient.fullName,
    });
    setPatientResults([]);
    setPatientSearch('');
  };

  const selectCompanion = (patient: PatientSearchResult) => {
    setFormData({
      ...formData,
      companionId: patient.id,
      companionName: patient.fullName,
    });
    setCompanionResults([]);
  };

  const loadSeats = async () => {
    if (formData.appointmentDate) {
      const dateTime = `${formData.appointmentDate}T${formData.appointmentTime}:00`;
      const availableSeats = await getSeatAvailability(dateTime, formData.isPriority);
      setSeats(availableSeats);

      if (formData.hasCompanion) {
        setCompanionSeats(availableSeats);
      }
    }
  };

  useEffect(() => {
    if (step === 2) {
      loadSeats();
    }
  }, [step]);

  const handleSubmit = async () => {
    if (!user || !formData.seatNumber) return;

    const appointmentDateTime = `${formData.appointmentDate}T${formData.appointmentTime}:00`;

    const result = await createAppointment({
      patientId: formData.patientId,
      medicalRecordNumber: formData.medicalRecordNumber,
      destinationHospital: formData.destinationHospital,
      treatmentType: parseInt(formData.treatmentType),
      treatmentTypeOther: formData.treatmentType === '4' ? formData.treatmentTypeOther : undefined,
      isPriority: formData.isPriority,
      seatNumber: formData.seatNumber,
      appointmentDate: appointmentDateTime,
      companionId: formData.hasCompanion ? formData.companionId : undefined,
      companionSeatNumber: formData.hasCompanion && formData.companionSeatNumber !== null ? formData.companionSeatNumber : undefined, // CORREÇÃO AQUI
      createdByUserId: user.userId,
    });

    if (result) {
      router.push('/appointments');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/appointments">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold">Novo Agendamento</h1>
          <p className="text-muted-foreground">Crie um novo agendamento de transporte</p>
        </div>
      </div>

      {step === 1 && (
        <Card>
          <CardHeader>
            <CardTitle>Dados do Agendamento</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Buscar Paciente */}
            <div className="space-y-2">
              <Label>Paciente *</Label>
              {formData.patientId ? (
                <div className="flex items-center justify-between p-3 border rounded-lg bg-muted">
                  <span>{formData.patientName}</span>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setFormData({ ...formData, patientId: '', patientName: '' })}
                  >
                    Trocar
                  </Button>
                </div>
              ) : (
                <>
                  <Input
                    placeholder="Digite o nome, CPF ou cartão SUS do paciente..."
                    value={patientSearch}
                    onChange={(e) => handlePatientSearch(e.target.value)}
                  />
                  {patientResults.length > 0 && (
                    <div className="border rounded-lg mt-2 max-h-48 overflow-y-auto">
                      {patientResults.map((patient) => (
                        <button
                          key={patient.id}
                          onClick={() => selectPatient(patient)}
                          className="w-full text-left p-3 hover:bg-muted transition-colors border-b last:border-b-0"
                        >
                          <p className="font-medium">{patient.fullName}</p>
                          <p className="text-sm text-muted-foreground">
                            CPF: {patient.cpf} | SUS: {patient.susCardNumber}
                          </p>
                        </button>
                      ))}
                    </div>
                  )}
                </>
              )}
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="medicalRecordNumber">Número do Prontuário *</Label>
                <Input
                  id="medicalRecordNumber"
                  value={formData.medicalRecordNumber}
                  onChange={(e) => setFormData({ ...formData, medicalRecordNumber: e.target.value })}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="destinationHospital">Hospital de Destino *</Label>
                <Input
                  id="destinationHospital"
                  value={formData.destinationHospital}
                  onChange={(e) => setFormData({ ...formData, destinationHospital: e.target.value })}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="treatmentType">Tipo de Tratamento *</Label>
                <Select
                  value={formData.treatmentType}
                  onValueChange={(value) => setFormData({ ...formData, treatmentType: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="1">Semanal</SelectItem>
                    <SelectItem value="2">Mensal</SelectItem>
                    <SelectItem value="3">Trimestral</SelectItem>
                    <SelectItem value="4">Outro</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {formData.treatmentType === '4' && (
                <div className="space-y-2">
                  <Label htmlFor="treatmentTypeOther">Especificar Tratamento *</Label>
                  <Input
                    id="treatmentTypeOther"
                    value={formData.treatmentTypeOther}
                    onChange={(e) => setFormData({ ...formData, treatmentTypeOther: e.target.value })}
                    required
                  />
                </div>
              )}

              <div className="space-y-2">
                <Label htmlFor="appointmentDate">Data da Viagem *</Label>
                <Input
                  id="appointmentDate"
                  type="date"
                  value={formData.appointmentDate}
                  onChange={(e) => setFormData({ ...formData, appointmentDate: e.target.value })}
                  min={new Date().toISOString().split('T')[0]}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="appointmentTime">Horário *</Label>
                <Input
                  id="appointmentTime"
                  type="time"
                  value={formData.appointmentTime}
                  onChange={(e) => setFormData({ ...formData, appointmentTime: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="isPriority"
                checked={formData.isPriority}
                onCheckedChange={(checked) => setFormData({ ...formData, isPriority: checked as boolean })}
              />
              <Label htmlFor="isPriority" className="cursor-pointer">
                Paciente Prioritário (permite poltronas 1, 2 e 3)
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="hasCompanion"
                checked={formData.hasCompanion}
                onCheckedChange={(checked) => setFormData({ ...formData, hasCompanion: checked as boolean })}
              />
              <Label htmlFor="hasCompanion" className="cursor-pointer">
                Possui Acompanhante
              </Label>
            </div>

            {formData.hasCompanion && (
              <div className="space-y-2">
                <Label>Acompanhante *</Label>
                {formData.companionId ? (
                  <div className="flex items-center justify-between p-3 border rounded-lg bg-muted">
                    <span>{formData.companionName}</span>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setFormData({ ...formData, companionId: '', companionName: '' })}
                    >
                      Trocar
                    </Button>
                  </div>
                ) : (
                  <>
                    <Input
                      placeholder="Digite o nome do acompanhante..."
                      onChange={(e) => handleCompanionSearch(e.target.value)}
                    />
                    {companionResults.length > 0 && (
                      <div className="border rounded-lg mt-2 max-h-48 overflow-y-auto">
                        {companionResults.map((patient) => (
                          <button
                            key={patient.id}
                            onClick={() => selectCompanion(patient)}
                            className="w-full text-left p-3 hover:bg-muted transition-colors border-b last:border-b-0"
                          >
                            <p className="font-medium">{patient.fullName}</p>
                            <p className="text-sm text-muted-foreground">
                              CPF: {patient.cpf} | SUS: {patient.susCardNumber}
                            </p>
                          </button>
                        ))}
                      </div>
                    )}
                  </>
                )}
              </div>
            )}
            <div className="flex gap-4 justify-end">
              <Link href="/appointments">
                <Button type="button" variant="outline">
                  Cancelar
                </Button>
              </Link>
              <Button
                onClick={() => setStep(2)}
                disabled={!formData.patientId || !formData.appointmentDate || (formData.hasCompanion && !formData.companionId)}
              >
                Próximo: Selecionar Poltrona
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {step === 2 && (
        <Card>
          <CardHeader>
            <CardTitle>Selecione a Poltrona do Paciente</CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <SeatSelector
              seats={seats}
              selectedSeat={formData.seatNumber}
              onSelectSeat={(seatNumber) => setFormData({ ...formData, seatNumber })}
              isPriority={formData.isPriority}
            />

            {formData.hasCompanion && formData.seatNumber && (
              <>
                <div className="border-t pt-6">
                  <h3 className="text-lg font-semibold mb-4">Selecione a Poltrona do Acompanhante</h3>
                  <SeatSelector
                    seats={companionSeats.map(seat => ({
                      ...seat,
                      isAvailable: seat.isAvailable && seat.seatNumber !== formData.seatNumber
                    }))}
                    selectedSeat={formData.companionSeatNumber}
                    onSelectSeat={(seatNumber) => setFormData({ ...formData, companionSeatNumber: seatNumber })}
                    isPriority={false}
                  />
                </div>
              </>
            )}

            <div className="flex gap-4 justify-end">
              <Button type="button" variant="outline" onClick={() => setStep(1)}>
                Voltar
              </Button>
              <Button
                onClick={handleSubmit}
                disabled={loading || !formData.seatNumber || (formData.hasCompanion && !formData.companionSeatNumber)}
              >
                {loading ? 'Criando...' : 'Confirmar Agendamento'}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
