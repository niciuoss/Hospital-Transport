'use client';

import { useEffect, useState } from 'react';
import { useUsers } from '@/hooks/useUsers';
import { UserData } from '@/types/user';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Plus, UserCog, Pencil } from 'lucide-react';
import Link from 'next/link';
import { formatDateTime } from '@/lib/utils';

export default function UsersPage() {
  const [users, setUsers] = useState<UserData[]>([]);
  const { getUsers, loading } = useUsers();

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    const data = await getUsers();
    setUsers(data);
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Usuários do Sistema</h1>
          <p className="text-muted-foreground">Gerencie os funcionários do hospital</p>
        </div>
        <Link href="/users/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Novo Usuário
          </Button>
        </Link>
      </div>

      {loading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Carregando usuários...</p>
        </div>
      ) : users.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <UserCog className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
            <p className="text-muted-foreground">Nenhum usuário encontrado</p>
            <Link href="/users/new">
              <Button className="mt-4">Criar Primeiro Usuário</Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {users.map((user) => (
            <Card key={user.id}>
              <CardHeader>
                <div className="flex justify-between items-start">
                  <CardTitle className="text-lg">{user.fullName}</CardTitle>
                  <Badge variant={user.role === 'Admin' ? 'default' : 'secondary'}>
                    {user.role}
                  </Badge>
                </div>
              </CardHeader>
              <div className="mt-4">
                <Link href={`/users/${user.id}/edit`}>
                  <Button variant="outline" size="sm" className="w-full">
                    <Pencil className="h-4 w-4 mr-2" />
                    Editar
                  </Button>
                </Link>
              </div>
              <CardContent className="space-y-2">
                <div className="text-sm">
                  <span className="font-medium">Usuário:</span> @{user.username}
                </div>
                <div className="text-sm">
                  <span className="font-medium">Cadastrado em:</span>{' '}
                  {formatDateTime(user.createdAt)}
                </div>
                <div className="text-sm">
                  <span className="font-medium">Status:</span>{' '}
                  <Badge variant={user.isActive ? 'default' : 'destructive'}>
                    {user.isActive ? 'Ativo' : 'Inativo'}
                  </Badge>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}