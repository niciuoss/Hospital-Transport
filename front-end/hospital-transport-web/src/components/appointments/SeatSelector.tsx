'use client';

import { cn } from '@/lib/utils';
import { SeatAvailability } from '@/types/appointment';

interface SeatSelectorProps {
  seats: SeatAvailability[];
  selectedSeat: number | null;
  onSelectSeat: (seatNumber: number) => void;
  isPriority: boolean;
}

export function SeatSelector({ seats, selectedSeat, onSelectSeat, isPriority }: SeatSelectorProps) {
  const getSeatColor = (seat: SeatAvailability) => {
    if (seat.seatNumber === selectedSeat) {
      return 'bg-yellow-500 text-white border-yellow-600';
    }
    if (!seat.isAvailable) {
      return 'bg-gray-400 text-white cursor-not-allowed';
    }
    if (seat.isPriorityOnly && !isPriority) {
      return 'bg-gray-200 text-gray-500 cursor-not-allowed';
    }
    return 'bg-green-500 text-white hover:bg-green-600 cursor-pointer';
  };

  const getSeatLabel = (seat: SeatAvailability) => {
    if (seat.seatNumber === selectedSeat) return 'Selecionado';
    if (!seat.isAvailable) return 'Ocupado';
    if (seat.isPriorityOnly && !isPriority) return 'Prioritário';
    return 'Disponível';
  };

  // Poltronas em fileiras
  const rows = [
    seats.slice(0, 11),  // Fileira 1: 3-43
    seats.slice(11, 22), // Fileira 2: 4-44
    seats.slice(22, 34), // Fileira 3: 2-46
    seats.slice(34, 46), // Fileira 4: 1-45
  ];

  return (
    <div className="space-y-6">
      {/* Legenda */}
      <div className="flex gap-4 justify-center flex-wrap">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-green-500 rounded border-2"></div>
          <span className="text-sm">Disponível</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-yellow-500 rounded border-2"></div>
          <span className="text-sm">Selecionado</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-gray-400 rounded border-2"></div>
          <span className="text-sm">Ocupado</span>
        </div>
        {!isPriority && (
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-gray-200 rounded border-2"></div>
            <span className="text-sm">Prioritário</span>
          </div>
        )}
      </div>

      {/* Layout do ônibus */}
      <div className="max-w-4xl mx-auto bg-gray-50 p-6 rounded-lg border-2">
        <div className="text-center mb-4 font-bold">FRENTE DO ÔNIBUS</div>
        
        <div className="space-y-3">
          {rows.map((row, rowIndex) => (
            <div key={rowIndex} className="flex gap-2 justify-center">
              {row.map((seat) => (
                <button
                  key={seat.seatNumber}
                  onClick={() => seat.isAvailable && !(seat.isPriorityOnly && !isPriority) && onSelectSeat(seat.seatNumber)}
                  disabled={!seat.isAvailable || (seat.isPriorityOnly && !isPriority)}
                  className={cn(
                    'w-12 h-12 rounded border-2 font-bold text-sm transition-all',
                    getSeatColor(seat)
                  )}
                  title={getSeatLabel(seat)}
                >
                  {seat.seatNumber}
                </button>
              ))}
            </div>
          ))}
        </div>
      </div>

      {selectedSeat && (
        <div className="text-center p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
          <p className="font-bold text-lg">Poltrona Selecionada: {selectedSeat}</p>
        </div>
      )}
    </div>
  );
}