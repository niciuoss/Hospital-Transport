"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { usePatients } from "@/hooks/usePatients";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";

export default function EditPatientPage({
  params,
}: {
  params: { id: string };
}) {
  const router = useRouter();
  const { getPatientById, updatePatient, loading } = usePatients();
  const [formData, setFormData] = useState({
    fullName: "",
    rg: "",
    cpf: "",
    age: "",
    birthDate: "",
    susCardNumber: "",
    phoneNumber: "",
    motherName: "",
    address: "",
  });

  useEffect(() => {
    loadPatient();
  }, []);

  const loadPatient = async () => {
    const patient = await getPatientById(params.id);
    if (patient) {
      setFormData({
        fullName: patient.fullName,
        rg: patient.rg,
        cpf: patient.cpf,
        age: patient.age.toString(),
        birthDate: patient.birthDate,
        susCardNumber: patient.susCardNumber,
        phoneNumber: patient.phoneNumber,
        motherName: patient.motherName,
        address: patient.address,
      });
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const calculateAge = (birthDate: string): number => {
    if (!birthDate) return 0;
    
    const today = new Date();
    const birth = new Date(birthDate);
    let age = today.getFullYear() - birth.getFullYear();
    const monthDiff = today.getMonth() - birth.getMonth();
    
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
      age--;
    }
    
    return age < 0 ? 0 : age;
  };

  const handleBirthDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const birthDate = e.target.value;
    const age = calculateAge(birthDate);
    
    setFormData({
      ...formData,
      birthDate,
      age: age.toString()
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const result = await updatePatient(params.id, {
      ...formData,
      age: parseInt(formData.age),
    });

    if (result) {
      router.push("/patients");
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/patients">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold">Editar Paciente</h1>
          <p className="text-muted-foreground">Atualize os dados do paciente</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Dados do Paciente</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="fullName">Nome Completo *</Label>
                <Input
                  id="fullName"
                  name="fullName"
                  value={formData.fullName}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="motherName">Nome da Mãe *</Label>
                <Input
                  id="motherName"
                  name="motherName"
                  value={formData.motherName}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="rg">RG </Label>
                <Input
                  id="rg"
                  name="rg"
                  value={formData.rg}
                  onChange={handleChange}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="cpf">CPF </Label>
                <Input
                  id="cpf"
                  name="cpf"
                  value={formData.cpf}
                  onChange={handleChange}
                  placeholder="00000000000"
                  maxLength={11}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="birthDate">Data de Nascimento *</Label>
                <Input
                  id="birthDate"
                  name="birthDate"
                  type="date"
                  value={formData.birthDate}
                  onChange={handleBirthDateChange}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="age">Idade *</Label>
                <Input
                  id="age"
                  name="age"
                  type="number"
                  value={formData.age}
                  onChange={handleChange}
                  required
                  readOnly
                  className="bg-gray-100 dark:bg-gray-800"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="susCardNumber">Cartão SUS </Label>
                <Input
                  id="susCardNumber"
                  name="susCardNumber"
                  value={formData.susCardNumber}
                  onChange={handleChange}
                  placeholder="000000000000000"
                  maxLength={15}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="phoneNumber">Telefone </Label>
                <Input
                  id="phoneNumber"
                  name="phoneNumber"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  placeholder="(00) 00000-0000"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="address">Endereço Completo *</Label>
                <textarea
                  id="address"
                  name="address"
                  value={formData.address}
                  onChange={(e) =>
                    setFormData({ ...formData, address: e.target.value })
                  }
                  className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  placeholder="Rua, número, bairro, cidade..."
                  required
                />
              </div>
            </div>

            <div className="flex gap-4 justify-end">
              <Link href="/patients">
                <Button type="button" variant="outline">
                  Cancelar
                </Button>
              </Link>
              <Button type="submit" disabled={loading}>
                {loading ? "Salvando..." : "Atualizar Paciente"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
