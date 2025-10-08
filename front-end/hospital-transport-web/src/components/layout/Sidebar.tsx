'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import { cn } from '@/lib/utils';
import { Home, Users, Calendar, UserCog, FileText, Settings } from 'lucide-react';

const menuItems = [
  { icon: Home, label: 'Dashboard', href: '/dashboard', adminOnly: false },
  { icon: Users, label: 'Pacientes', href: '/patients', adminOnly: false },
  { icon: Calendar, label: 'Agendamentos', href: '/appointments', adminOnly: false },
  { icon: UserCog, label: 'Usuários', href: '/users', adminOnly: false },
  { icon: FileText, label: 'Relatórios', href: '/reports', adminOnly: false },
  { icon: Settings, label: 'Administração', href: '/admin', adminOnly: true },
];

export function Sidebar() {
  const pathname = usePathname();
  const { user } = useAuth();

  const visibleItems = menuItems.filter(item => 
    !item.adminOnly || (user && JSON.parse(localStorage.getItem('user') || '{}').role === 'Admin')
  );

  return (
    <aside className="w-64 border-r bg-background sticky top-0 h-screen overflow-y-auto">
      <nav className="p-4 space-y-2">
        {visibleItems.map((item) => {
          const Icon = item.icon;
          const isActive = pathname === item.href || pathname.startsWith(item.href + '/');
          
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                'flex items-center gap-3 px-4 py-3 rounded-lg transition-colors',
                isActive
                  ? 'bg-primary text-primary-foreground'
                  : 'hover:bg-muted text-muted-foreground hover:text-foreground'
              )}
            >
              <Icon className="h-5 w-5" />
              <span className="font-medium">{item.label}</span>
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}