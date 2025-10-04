'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useUsers } from '@/hooks/useUsers';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { ArrowLeft } from 'lucide-react';
import Link from 'next/link';

export default function NewUserPage() {
  const router = useRouter();
  const { createUser, loading } = useUsers();
  const [formData, setFormData] = useState({
    fullName: '',
    username: '',
    password: '',
    confirmPassword: '',
    role: 'Employee',
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      alert('As senhas não conferem!');
      return;
    }

    const result = await createUser({
      fullName: formData.fullName,
      username: formData.username,
      password: formData.password,
      role: formData.role,
    });

    if (result) {
      router.push('/users');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/users">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-3xl font-bold">Novo Usuário</h1>
          <p className="text-muted-foreground">Cadastre um novo funcionário no sistema</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Dados do Usuário</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
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
              <Label htmlFor="username">Nome de Usuário *</Label>
              <Input
                id="username"
                name="username"
                value={formData.username}
                onChange={handleChange}
                placeholder="ex: maria.silva"
                required
              />
              <p className="text-xs text-muted-foreground">
                Apenas letras, números, ponto, hífen e underscore
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="role">Função *</Label>
              <Select
                value={formData.role}
                onValueChange={(value) => setFormData({ ...formData, role: value })}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Employee">Funcionário</SelectItem>
                  <SelectItem value="Admin">Administrador</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="password">Senha *</Label>
                <Input
                  id="password"
                  name="password"
                  type="password"
                  value={formData.password}
                  onChange={handleChange}
                  minLength={6}
                  required
                />
                <p className="text-xs text-muted-foreground">Mínimo de 6 caracteres</p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="confirmPassword">Confirmar Senha *</Label>
                <Input
                  id="confirmPassword"
                  name="confirmPassword"
                  type="password"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  minLength={6}
                  required
                />
              </div>
            </div>

            <div className="flex gap-4 justify-end">
              <Link href="/users">
                <Button type="button" variant="outline">
                  Cancelar
                </Button>
              </Link>
              <Button type="submit" disabled={loading}>
                {loading ? 'Salvando...' : 'Cadastrar Usuário'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}