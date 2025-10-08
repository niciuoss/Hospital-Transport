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

  // Organização das poltronas conforme o layout real do ônibus
  const layout = [
    // Fileira 1 (lado esquerdo) - Ímpares da frente
    [3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47],
    // Fileira 2 (lado esquerdo-meio) - Pares da frente exceto 4
    [8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48],
    // Fileira 3 (lado direito-meio) - Pares de trás
    [2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46],
    // Fileira 4 (lado direito) - Ímpares de trás
    [1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45],
  ];

  const getSeatByNumber = (seatNumber: number) => {
    return seats.find(s => s.seatNumber === seatNumber);
  };

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
      <div className="max-w-5xl mx-auto bg-gray-50 dark:bg-gray-900 p-6 rounded-lg border-2">
        <div className="text-center mb-4 font-bold">FRENTE DO ÔNIBUS</div>
        
        <div className="space-y-3">
          {layout.map((row, rowIndex) => (
            <div key={rowIndex} className="flex gap-2 justify-center">
              {row.map((seatNumber) => {
                const seat = getSeatByNumber(seatNumber);
                if (!seat) return null;

                return (
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
                );
              })}
            </div>
          ))}
        </div>

        {/* Poltrona 3 separada */}
        <div className="mt-4 pt-4 border-t-2 border-dashed">
          <div className="text-center text-sm text-muted-foreground mb-2">Poltrona Isolada</div>
          <div className="flex justify-center">
            {(() => {
              const seat = getSeatByNumber(3);
              if (!seat) return null;

              return (
                <button
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
              );
            })()}
          </div>
        </div>
      </div>

      {selectedSeat && (
        <div className="text-center p-4 bg-yellow-50 dark:bg-yellow-950 border border-yellow-200 dark:border-yellow-800 rounded-lg">
          <p className="font-bold text-lg">Poltrona Selecionada: {selectedSeat}</p>
        </div>
      )}
    </div>
  );
}