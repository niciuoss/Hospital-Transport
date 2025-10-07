'use client';

import { useAuth } from '@/context/AuthContext';
import { useTheme } from 'next-themes';
import { Button } from '@/components/ui/button';
import { LogOut, User, Moon, Sun } from 'lucide-react';

export function Header() {
  const { user, logout } = useAuth();
  const { theme, setTheme } = useTheme();

  return (
    <header className="border-b bg-background">
      <div className="flex h-16 items-center px-6 justify-between">
        <div>
          <h2 className="text-2xl font-bold text-primary">Sistema de Agendamento para o Transporte Hospitalar</h2>
          <p className="text-sm text-muted-foreground">Hospital e Maternidade Dr. Cicero Ferreira Filho</p>
        </div>
        
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
          >
            <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
            <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
            <span className="sr-only">Toggle theme</span>
          </Button>
          
          <div className="flex items-center gap-2">
            <User className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-sm font-medium">Olá, {user?.fullName}</p>
              <p className="text-xs text-muted-foreground">@{user?.username}</p>
            </div>
          </div>
          <Button variant="outline" size="sm" onClick={logout}>
            <LogOut className="h-4 w-4 mr-2" />
            Sair
          </Button>
        </div>
      </div>
    </header>
  );
}