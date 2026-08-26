"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAppointments } from "@/hooks/useAppointments";
import { usePatients } from "@/hooks/usePatients";
import { useBuses } from "@/hooks/useBuses";
import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { SeatSelector } from "@/components/appointments/SeatSelector";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { SeatSelectorMicrobus } from "@/components/appointments/SeatSelectorMicrobus";
import { PatientSearchResult, Patient } from "@/types/patient";
import { SeatAvailability } from "@/types/appointment";
import { Bus } from "@/types/bus";
import { toast } from "sonner"; 

export default function NewAppointmentPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { createAppointment, getSeatAvailability, loading } = useAppointments();
  const { searchPatients, getPatients } = usePatients(); 
   const { getBuses } = useBuses();

  const [step, setStep] = useState(1);
  const [patientSearch, setPatientSearch] = useState("");
  const [patientResults, setPatientResults] = useState<PatientSearchResult[]>([]);
  const [companionResults, setCompanionResults] = useState<PatientSearchResult[]>([]);
  const [seats, setSeats] = useState<SeatAvailability[]>([]);
  const [companionSeats, setCompanionSeats] = useState<SeatAvailability[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]); 
  const [buses, setBuses] = useState<Bus[]>([]);
  const [availableSeatsCount, setAvailableSeatsCount] = useState(0); 

  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [isInfant, setIsInfant] = useState(false);
  const [infantNeedsSeat, setInfantNeedsSeat] = useState(false);

  const [availableSeatsForSelectedDate, setAvailableSeatsForSelectedDate] = useState<number | null>(null);
  const [loadingSeatsCount, setLoadingSeatsCount] = useState(false);

  const [formData, setFormData] = useState({
    patientId: "",
    patientName: "",
    medicalRecordNumber: "",
    destinationHospital: "",
    busId: "",
    treatmentType: "1",
    treatmentTypeOther: "",
    isPriority: false,
    seatNumber: 0, 
    appointmentDate: "",
    appointmentTime: "08:00",
    hasCompanion: false,
    companionId: "",
    companionName: "",
    companionSeatNumber: 0,
  });

  useEffect(() => {
    loadPatients();
    loadBuses();
  }, []);

  const loadPatients = async () => {
    const result = await getPatients();
    if (result) {
      setPatients(result);
    }
  };

  const loadBuses = async () => {
    const result = await getBuses();
    if (result) {
      setBuses(result);
    }
  };

  const handlePatientSearch = async (term: string) => {
    setPatientSearch(term);
    if (term.length >= 3) {
      const results = await searchPatients(term);
      setPatientResults(results);
    } else {
      setPatientResults([]);
    }
  };

  const handlePatientChange = (patientId: string) => {
    const patient = patients.find((p) => p.id === patientId);
    setSelectedPatient(patient || null);

    if (patient && patient.age <= 7) {
      setIsInfant(true);
      setInfantNeedsSeat(false);
      setFormData({
        ...formData,
        patientId,
        patientName: patient.fullName,
        seatNumber: 0,
      });
      toast.info("Criança de 0 a 7 anos detectada. Configure o modo de viagem.");
    } else {
      setIsInfant(false);
      setInfantNeedsSeat(false);
      setFormData({
        ...formData,
        patientId,
        patientName: patient?.fullName || "",
      });
    }
  };

  const checkAvailableSeatsForDate = async (date: string) => {
    console.log("🔍 checkAvailableSeatsForDate chamada com:", date);
    
    if (!date) {
      setAvailableSeatsForSelectedDate(null);
      return;
    }

    if (!formData.busId) {
      console.log("⚠️ Ônibus não selecionado ainda");
      setAvailableSeatsForSelectedDate(null);
      return;
    }

    setLoadingSeatsCount(true);
    console.log("⏳ Iniciando busca...");
    console.log("🚌 BusId:", formData.busId);
    
    try {
      const seats = await getSeatAvailability(
        date,
        formData.busId,
        false // isPriority sempre false para contagem
      );
      
      const availableCount = seats.filter(seat => seat.isAvailable).length;
      
      console.log("📊 Total de poltronas:", seats.length);
      console.log("✅ Poltronas disponíveis:", availableCount);
      console.log("❌ Poltronas ocupadas:", seats.length - availableCount);
      
      setAvailableSeatsForSelectedDate(availableCount);
    } catch (error) {
      console.error("💥 Erro ao buscar poltronas disponíveis:", error);
      setAvailableSeatsForSelectedDate(null);
    } finally {
      setLoadingSeatsCount(false);
      console.log("🏁 Busca finalizada");
    }
  };

  const handleInfantSeatToggle = (checked: boolean) => {
    setInfantNeedsSeat(checked);

    if (!checked) {
      // Criança vai no colo (sem poltrona)
      setFormData({ ...formData, seatNumber: 0 });
    }
  };

  const handleCompanionSearch = async (term: string) => {
    if (term.length >= 3) {
      const results = await searchPatients(term);
      // FILTRAR: Remover o paciente selecionado dos resultados
      const filteredResults = results.filter(r => r.id !== formData.patientId);
      setCompanionResults(filteredResults);
    } else {
      setCompanionResults([]);
    }
  };

  const selectPatient = (patient: PatientSearchResult) => {
    handlePatientChange(patient.id);
    setPatientResults([]);
    setPatientSearch("");
  };

  const selectCompanion = (patient: PatientSearchResult) => {
    // VALIDAR: Não pode ser o mesmo que o paciente
    if (patient.id === formData.patientId) {
      toast.error("O acompanhante não pode ser o mesmo que o paciente");
      return;
    }

    setFormData({
      ...formData,
      companionId: patient.id,
      companionName: patient.fullName,
    });
    setCompanionResults([]);
  };

  const loadSeats = async () => {
    if (formData.appointmentDate && formData.busId) {
      const dateTime = `${formData.appointmentDate}T${formData.appointmentTime}:00`;
      //const result = await getSeatAvailability(dateTime, formData.isPriority);
      const result = await getSeatAvailability(
        formData.appointmentDate, 
        formData.busId,
        formData.isPriority);
      
      if (result) {
        setSeats(result);
        setCompanionSeats(result);

        // Contar poltronas disponíveis
        const available = result.filter(seat => seat.isAvailable).length;
        setAvailableSeatsCount(available);
      }
    }
  };

  useEffect(() => {
    if (step === 2) {
      loadSeats();
    }
  }, [step]);

  useEffect(() => {
    if (formData.appointmentDate && formData.busId) {
      loadSeats();
      checkAvailableSeatsForDate(formData.appointmentDate); 
    }
  }, [formData.appointmentDate, formData.isPriority, formData.busId]);


  const handleSubmit = async () => {
    if (!user) {
      toast.error("Usuário não autenticado");
      return;
    }

    //Validação de busId
    if (!formData.busId) {
      toast.error("Selecione o ônibus");
      return;
    }

    // Validações para criança de colo
    if (isInfant) {
      if (!formData.companionId) {
        toast.error("Crianças de 0 a 7 anos devem ter um acompanhante");
        return;
      }

      if (infantNeedsSeat && formData.seatNumber === 0) {
        toast.error("Selecione a poltrona para a cadeirinha da criança");
        return;
      }
    }

    // Validação para pacientes normais
    if (!isInfant && formData.seatNumber === 0) {
      toast.error("Selecione a poltrona do paciente");
      return;
    }

    // Validação de acompanhante
    if (formData.hasCompanion && !formData.companionSeatNumber) {
      toast.error("Selecione a poltrona do acompanhante");
      return;
    }

    // Validação: acompanhante não pode ser o mesmo que o paciente
    if (formData.companionId === formData.patientId) {
      toast.error("O acompanhante não pode ser o mesmo que o paciente");
      return;
    }

    const appointmentDateTime = `${formData.appointmentDate}T${formData.appointmentTime}:00`;

    const result = await createAppointment({
      patientId: formData.patientId,
      busId: formData.busId,
      medicalRecordNumber: formData.medicalRecordNumber,
      destinationHospital: formData.destinationHospital,
      treatmentType: parseInt(formData.treatmentType),
      treatmentTypeOther:
        formData.treatmentType === "4" ? formData.treatmentTypeOther : undefined,
      isPriority: formData.isPriority,
      seatNumber: formData.seatNumber,
      appointmentDate: appointmentDateTime,
      companionId: formData.hasCompanion ? formData.companionId : undefined,
      companionSeatNumber:
        formData.hasCompanion && formData.companionSeatNumber > 0
          ? formData.companionSeatNumber
          : undefined,
      createdByUserId: user.userId, 
    });

    if (result) {
      router.push("/appointments");
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
          <p className="text-muted-foreground">
            Crie um novo agendamento do transporte
          </p>
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
                    onClick={() => {
                      setFormData({
                        ...formData,
                        patientId: "",
                        patientName: "",
                      });
                      setSelectedPatient(null);
                      setIsInfant(false);
                      setInfantNeedsSeat(false);
                    }}
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
                            CPF: {patient.cpf || "Não informado"} | SUS:{" "}
                            {patient.susCardNumber || "Não informado"}
                          </p>
                        </button>
                      ))}
                    </div>
                  )}
                </>
              )}
            </div>

            <div className="flex items-center space-x-2">
              <Button
                //variant="outline"
                onClick={() =>
                  window.open(
                    "https://www.tse.jus.br/servicos-eleitorais/autoatendimento-eleitoral#/atendimento-eleitor",
                    "_blank",
                    "noopener,noreferrer"
                  )
                }
              >
                Consultar Título de Eleitor
              </Button>  
            </div>

            <div className="space-y-2">
              <Label htmlFor="busId">Selecione o Ônibus *</Label>
              <Select
                value={formData.busId}
                onValueChange={(value) =>
                  setFormData({ ...formData, busId: value })
                }
                required
              >
                <SelectTrigger>
                  <SelectValue placeholder="Escolha o ônibus" />
                </SelectTrigger>
                <SelectContent>
                  {buses.map((bus) => (
                    <SelectItem key={bus.id} value={bus.id}>
                      {bus.name} - {bus.destination} ({bus.totalSeats} lugares)
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Alerta de criança de colo */}
            {isInfant && (
              <div className="p-4 bg-blue-50 dark:bg-blue-950 border border-blue-200 dark:border-blue-800 rounded-lg">
                <div className="flex items-center gap-2">
                  <span className="text-2xl">👶</span>
                  <div>
                    <p className="font-semibold text-blue-900 dark:text-blue-100">
                      Criança de 0 a 7 anos detectada
                    </p>
                    <p className="text-sm text-blue-700 dark:text-blue-300">
                      Você poderá configurar o modo de viagem na próxima etapa
                    </p>
                  </div>
                </div>
              </div>
            )}

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="medicalRecordNumber">
                  Número do Prontuário *
                </Label>
                <Input
                  id="medicalRecordNumber"
                  value={formData.medicalRecordNumber}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      medicalRecordNumber: e.target.value,
                    })
                  }
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="destinationHospital">
                  Hospital de Destino *
                </Label>
                <Input
                  id="destinationHospital"
                  value={formData.destinationHospital}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      destinationHospital: e.target.value,
                    })
                  }
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="treatmentType">Tipo de Tratamento *</Label>
                <Select
                  value={formData.treatmentType}
                  onValueChange={(value) =>
                    setFormData({ ...formData, treatmentType: value })
                  }
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

              {formData.treatmentType === "4" && (
                <div className="space-y-2">
                  <Label htmlFor="treatmentTypeOther">
                    Especificar Tratamento *
                  </Label>
                  <Input
                    id="treatmentTypeOther"
                    value={formData.treatmentTypeOther}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        treatmentTypeOther: e.target.value,
                      })
                    }
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
                  onChange={(e) => {
                    setFormData({ ...formData, appointmentDate: e.target.value });
                    checkAvailableSeatsForDate(e.target.value);
                  }}
                  min={new Date().toISOString().split("T")[0]}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="appointmentTime">Horário *</Label>
                <Input
                  id="appointmentTime"
                  type="time"
                  value={formData.appointmentTime}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      appointmentTime: e.target.value,
                    })
                  }
                  required
                />
              </div>
            </div>

            {/* Indicador de poltronas disponíveis - FORA DA GRID */}
            {formData.appointmentDate && (
              <div className="mt-4">
                {loadingSeatsCount ? (
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-primary"></div>
                    <span>Verificando disponibilidade...</span>
                  </div>
                ) : availableSeatsForSelectedDate !== null ? (
                  <div
                    className={`flex items-center gap-2 p-3 rounded-lg border ${
                      availableSeatsForSelectedDate > 20
                        ? "bg-green-50 border-green-200 dark:bg-green-950 dark:border-green-800"
                        : availableSeatsForSelectedDate > 10
                        ? "bg-yellow-50 border-yellow-200 dark:bg-yellow-950 dark:border-yellow-800"
                        : "bg-red-50 border-red-200 dark:bg-red-950 dark:border-red-800"
                    }`}
                  >
                    <span className="text-2xl">
                      {availableSeatsForSelectedDate > 20
                        ? "✅"
                        : availableSeatsForSelectedDate > 10
                        ? "⚠️"
                        : "❌"}
                    </span>
                    <div>
                      <p
                        className={`font-semibold ${
                          availableSeatsForSelectedDate > 20
                            ? "text-green-900 dark:text-green-100"
                            : availableSeatsForSelectedDate > 10
                            ? "text-yellow-900 dark:text-yellow-100"
                            : "text-red-900 dark:text-red-100"
                        }`}
                      >
                        {availableSeatsForSelectedDate}{" "}
                        {availableSeatsForSelectedDate === 1
                          ? "poltrona disponível"
                          : "poltronas disponíveis"}
                      </p>
                      <p
                        className={`text-sm ${
                          availableSeatsForSelectedDate > 20
                            ? "text-green-700 dark:text-green-300"
                            : availableSeatsForSelectedDate > 10
                            ? "text-yellow-700 dark:text-yellow-300"
                            : "text-red-700 dark:text-red-300"
                        }`}
                      >
                        {availableSeatsForSelectedDate > 20
                          ? "Muitas poltronas disponíveis para esta data"
                          : availableSeatsForSelectedDate > 10
                          ? "Disponibilidade moderada para esta data"
                          : availableSeatsForSelectedDate > 0
                          ? "Poucas poltronas disponíveis para esta data"
                          : "Nenhuma poltrona disponível para esta data"}
                      </p>
                    </div>
                  </div>
                ) : null}
              </div>
            )}

            <div className="flex items-center space-x-2">   
              <Checkbox
                id="isPriority"
                checked={formData.isPriority}
                onCheckedChange={(checked) =>
                  setFormData({ ...formData, isPriority: checked as boolean })
                }
              />
              <Label htmlFor="isPriority" className="cursor-pointer">
                Poltrona com elevador
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="hasCompanion"
                checked={formData.hasCompanion}
                onCheckedChange={(checked) =>
                  setFormData({ ...formData, hasCompanion: checked as boolean })
                }
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
                      onClick={() =>
                        setFormData({
                          ...formData,
                          companionId: "",
                          companionName: "",
                        })
                      }
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
                              CPF: {patient.cpf || "Não informado"} | SUS:{" "}
                              {patient.susCardNumber || "Não informado"}
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
                disabled={
                  !formData.patientId ||
                  !formData.medicalRecordNumber ||
                  !formData.destinationHospital ||
                  !formData.appointmentDate ||
                  !formData.appointmentTime ||
                  !formData.busId ||
                  (formData.treatmentType === "4" && !formData.treatmentTypeOther) ||
                  (formData.hasCompanion && !formData.companionId)
                }
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
            <CardTitle className="flex items-center justify-between">
              <span>Selecionar Poltronas</span>
              <span className="text-sm font-normal text-muted-foreground">
                {availableSeatsCount} poltronas disponíveis
              </span>
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Configuração para criança de colo */}
            {isInfant && (
              <div className="space-y-4 p-4 bg-blue-50 dark:bg-blue-950 border border-blue-200 dark:border-blue-800 rounded-lg">
                <div className="flex items-center gap-2">
                  <span className="text-2xl">👶</span>
                  <div>
                    <p className="font-semibold text-blue-900 dark:text-blue-100">
                      Criança de 0 a 7 anos
                    </p>
                    <p className="text-sm text-blue-700 dark:text-blue-300">
                      Selecione como a criança viajará
                    </p>
                  </div>
                </div>

                <div className="flex items-center space-x-3 p-3 bg-white dark:bg-gray-900 rounded-lg border border-blue-300 dark:border-blue-700">
                  <Checkbox
                    id="infantNeedsSeat"
                    checked={infantNeedsSeat}
                    onCheckedChange={handleInfantSeatToggle}
                  />
                  <label
                    htmlFor="infantNeedsSeat"
                    className="flex items-center gap-2 cursor-pointer"
                  >
                    <span className="text-2xl">🪑</span>
                    <div>
                      <p className="font-medium">Criança usará cadeirinha</p>
                      <p className="text-sm text-muted-foreground">
                        {infantNeedsSeat
                          ? "A criança ocupará uma poltrona com cadeirinha"
                          : "A criança irá no colo do acompanhante"}
                      </p>
                    </div>
                  </label>
                </div>

                <div className="text-sm text-blue-700 dark:text-blue-300">
                  {infantNeedsSeat ? (
                    <p>
                      ✓ Selecione <strong>2 poltronas</strong>: uma para a
                      criança (com cadeirinha) e uma para o acompanhante
                    </p>
                  ) : (
                    <p>
                      ✓ Selecione apenas <strong>1 poltrona</strong> para o
                      acompanhante (a criança irá no colo)
                    </p>
                  )}
                </div>
              </div>
            )}

            {/* Seletor de poltrona do paciente */}
            {(!isInfant || infantNeedsSeat) && (
              <div className="space-y-2">
                <Label>
                  Selecione a Poltrona do Paciente *
                  {isInfant && infantNeedsSeat && (
                    <span className="ml-2 text-sm text-muted-foreground">
                      (Poltrona para cadeirinha)
                    </span>
                  )}
                </Label>
                {buses.find(b => b.id === formData.busId)?.seatLayout === "Microbus" ? (
                  <SeatSelectorMicrobus
                    seats={seats.map((seat) => ({
                      ...seat,
                      isAvailable:
                        seat.isAvailable &&
                        seat.seatNumber !== formData.companionSeatNumber,
                    }))}
                    selectedSeat={formData.seatNumber}
                    onSelectSeat={(seatNumber) =>
                      setFormData({ ...formData, seatNumber })
                    }
                    isPriority={formData.isPriority}
                  />
                ) : (
                  <SeatSelector
                    seats={seats.map((seat) => ({
                      ...seat,
                      isAvailable:
                        seat.isAvailable &&
                        seat.seatNumber !== formData.companionSeatNumber,
                    }))}
                    selectedSeat={formData.seatNumber}
                    onSelectSeat={(seatNumber) =>
                      setFormData({ ...formData, seatNumber })
                    }
                    isPriority={formData.isPriority}
                  />
                )}
              </div>
            )}

            {/* Mensagem quando criança vai no colo */}
            {isInfant && !infantNeedsSeat && (
              <div className="p-4 bg-green-50 dark:bg-green-950 border border-green-200 dark:border-green-800 rounded-lg">
                <p className="text-sm text-green-800 dark:text-green-200 flex items-center gap-2">
                  <span className="text-xl">✓</span>
                  <span>
                    Criança irá no colo. Selecione apenas a poltrona do
                    acompanhante abaixo.
                  </span>
                </p>
              </div>
            )}

            {/* Seletor de poltrona do acompanhante */}
            {formData.hasCompanion && (
              (!isInfant && formData.seatNumber > 0) || 
              (isInfant && formData.companionId)
            ) && (
              <>
                <div className="border-t pt-6">
                  <h3 className="text-lg font-semibold mb-4">
                    Selecione a Poltrona do Acompanhante
                  </h3>
                  {buses.find(b => b.id === formData.busId)?.seatLayout === "Microbus" ? (
                    <SeatSelectorMicrobus
                      seats={companionSeats.map((seat) => ({
                        ...seat,
                        isAvailable:
                          seat.isAvailable &&
                          seat.seatNumber !== formData.seatNumber,
                      }))}
                      selectedSeat={formData.companionSeatNumber}
                      onSelectSeat={(seatNumber) =>
                        setFormData({
                          ...formData,
                          companionSeatNumber: seatNumber,
                        })
                      }
                      isPriority={false}
                    />
                  ) : (
                    <SeatSelector
                      seats={companionSeats.map((seat) => ({
                        ...seat,
                        isAvailable:
                          seat.isAvailable &&
                          seat.seatNumber !== formData.seatNumber,
                      }))}
                      selectedSeat={formData.companionSeatNumber}
                      onSelectSeat={(seatNumber) =>
                        setFormData({
                          ...formData,
                          companionSeatNumber: seatNumber,
                        })
                      }
                      isPriority={false}
                    />
                  )}
                </div>
              </>
            )}
            <div className="flex gap-4 justify-end">
              <Button
                type="button"
                variant="outline"
                onClick={() => setStep(1)}
              >
                Voltar
              </Button>
              <Button
                onClick={handleSubmit}
                disabled={
                  loading ||
                  (!isInfant && formData.seatNumber === 0) ||
                  (isInfant && infantNeedsSeat && formData.seatNumber === 0) ||
                  (formData.hasCompanion && formData.companionSeatNumber === 0)
                }
              >
                {loading ? "Criando..." : "Confirmar Agendamento"}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}