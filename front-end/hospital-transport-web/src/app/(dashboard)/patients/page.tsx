'use client';

import { useEffect, useState } from 'react';
import { usePatients } from '@/hooks/usePatients';
import { Patient } from '@/types/patient';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Pagination, PaginationContent, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from '@/components/ui/pagination';
import { Plus, Search, Pencil } from 'lucide-react';
import Link from 'next/link';
import { formatCPF, formatPhone, formatDate } from '@/lib/utils';

const ITEMS_PER_PAGE = 9;

export default function PatientsPage() {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [filteredPatients, setFilteredPatients] = useState<Patient[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const { getPatients, loading } = usePatients();

  useEffect(() => {
    loadPatients();
  }, []);

  useEffect(() => {
    if (searchTerm) {
      const filtered = patients.filter(p =>
        p.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.cpf.includes(searchTerm) ||
        p.susCardNumber.includes(searchTerm)
      );
      setFilteredPatients(filtered);
    } else {
      setFilteredPatients(patients);
    }
    setCurrentPage(1);
  }, [searchTerm, patients]);

  const loadPatients = async () => {
    const data = await getPatients();
    setPatients(data);
    setFilteredPatients(data);
  };

  const totalPages = Math.ceil(filteredPatients.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const endIndex = startIndex + ITEMS_PER_PAGE;
  const currentPatients = filteredPatients.slice(startIndex, endIndex);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Pacientes</h1>
          <p className="text-muted-foreground">Gerencie os pacientes cadastrados</p>
        </div>
        <Link href="/patients/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Novo Paciente
          </Button>
        </Link>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Buscar Paciente</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex gap-2">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Buscar por nome, CPF ou cartão SUS..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </div>
          <p className="text-sm text-muted-foreground mt-2">
            Mostrando {currentPatients.length} de {filteredPatients.length} pacientes
          </p>
        </CardContent>
      </Card>

      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Carregando pacientes...</p>
        </div>
      ) : currentPatients.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground">Nenhum paciente encontrado</p>
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {currentPatients.map((patient) => (
              <Card key={patient.id}>
                <CardHeader>
                  <CardTitle className="text-lg">{patient.fullName}</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <div className="text-sm">
                    <span className="font-medium">CPF:</span> {formatCPF(patient.cpf)}
                  </div>
                  <div className="text-sm">
                    <span className="font-medium">Cartão SUS:</span> {patient.susCardNumber}
                  </div>
                  <div className="text-sm">
                    <span className="font-medium">Telefone:</span> {formatPhone(patient.phoneNumber)}
                  </div>
                  <div className="text-sm">
                    <span className="font-medium">Nascimento:</span> {formatDate(patient.birthDate)}
                  </div>
                  <div className="text-sm">
                    <span className="font-medium">Idade:</span> {patient.age} anos
                  </div>
                  <div className="mt-4">
                    <Link href={`/patients/${patient.id}/edit`}>
                      <Button variant="outline" size="sm" className="w-full">
                        <Pencil className="h-4 w-4 mr-2" />
                        Editar
                      </Button>
                    </Link>
                  </div>
                </CardContent>
              </Card>
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
                
                {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                  <PaginationItem key={page}>
                    <PaginationLink
                      onClick={() => setCurrentPage(page)}
                      isActive={currentPage === page}
                      className="cursor-pointer"
                    >
                      {page}
                    </PaginationLink>
                  </PaginationItem>
                ))}
                
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