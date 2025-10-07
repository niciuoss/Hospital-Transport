'use client';

import Image from 'next/image';
import { Header } from './Header';
import capa from '@/assets/capa.png';

export function HeaderWithCover() {
  return (
    <div>
      {/* Capa */}
      <div className="w-full relative">
        <Image
          src={capa}
          alt="Capa do Sistema"
          className="w-full h-85 object-contain bg-background"
          priority
        />
      </div>
      <Header />
    </div>
  );
}