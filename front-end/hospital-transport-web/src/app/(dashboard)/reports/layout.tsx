'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { cn } from '@/lib/utils';
import { BarChart2, Users, MapPin, Calendar } from 'lucide-react';

const subMenuItems = [
  { icon: Calendar, label: 'Relatórios por Data', href: '/reports/date' },
  { icon: Users, label: 'Relatório de Pacientes', href: '/reports/patients' },
  { icon: MapPin, label: 'Destinos', href: '/reports/destinations' },
];

export default function ReportsLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();

  return (
    <div className="flex gap-6">
      {/* Sub-navegação lateral */}
      <aside className="w-56 shrink-0">
        <div className="sticky top-4">
          <div className="flex items-center gap-2 px-3 py-2 mb-3">
            <BarChart2 className="h-5 w-5 text-primary" />
            <span className="font-semibold text-sm">Relatórios</span>
          </div>
          <nav className="space-y-1">
            {subMenuItems.map((item) => {
              const Icon = item.icon;
              const isActive = pathname === item.href || pathname.startsWith(item.href + '/');
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={cn(
                    'flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors',
                    isActive
                      ? 'bg-primary text-primary-foreground font-medium'
                      : 'hover:bg-muted text-muted-foreground hover:text-foreground'
                  )}
                >
                  <Icon className="h-4 w-4" />
                  <span>{item.label}</span>
                </Link>
              );
            })}
          </nav>
        </div>
      </aside>

      {/* Conteúdo principal */}
      <div className="flex-1 min-w-0">{children}</div>
    </div>
  );
}
