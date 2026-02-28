'use client';

import { cn } from '@/lib/utils';
import { SeatAvailability } from '@/types/appointment';

interface SeatSelectorMicrobusProps {
  seats: SeatAvailability[];
  selectedSeat: number | null;
  onSelectSeat: (seatNumber: number) => void;
  isPriority: boolean;
}

export function SeatSelectorMicrobus({
  seats,
  selectedSeat,
  onSelectSeat,
  isPriority,
}: SeatSelectorMicrobusProps) {
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

  // Layout padrão (poltronas 1-26) - IGUAL ao ônibus grande mas com poltrona 4
  const standardLayout = [
    [3, 7, 11, 15, 19, 23], // Lado esquerdo (ímpares)
    [4, 8, 12, 16, 20, 24], // Lado esquerdo-meio (pares) - POLTRONA 4 EXISTE!
    [2, 6, 10, 14, 18, 22, 26], // Lado direito-meio (pares)
    [1, 5, 9, 13, 17, 21, 25], // Lado direito (ímpares)
  ];

  // Poltronas extras do fundo (27-31)
  // Layout: 29  27  30
  //         28      31
  const backSeatsFirstRow = [28, 27, 29];
  const backSeatsSecondRow = [31, null, 30]; // null = espaço vazio

  const getSeatByNumber = (seatNumber: number) => {
    return seats.find((s) => s.seatNumber === seatNumber);
  };

  const renderSeat = (seatNumber: number | null) => {
    if (seatNumber === null) {
      return <div className="w-12 h-12"></div>; // Espaço vazio
    }

    const seat = getSeatByNumber(seatNumber);
    if (!seat) return null;

    return (
      <button
        key={seat.seatNumber}
        onClick={() =>
          seat.isAvailable &&
          !(seat.isPriorityOnly && !isPriority) &&
          onSelectSeat(seat.seatNumber)
        }
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

      {/* Layout do microônibus */}
      <div className="max-w-5xl mx-auto bg-gray-50 dark:bg-gray-900 p-6 rounded-lg border-2">
        <div className="text-center mb-4 font-bold">FRENTE DO MICROÔNIBUS</div>

        {/* Poltronas padrão (1-26) */}
        <div className="space-y-3">
          {standardLayout.map((row, rowIndex) => (
            <div key={rowIndex} className="flex gap-2 justify-center">
              {row.map((seatNumber) => renderSeat(seatNumber))}
            </div>
          ))}
        </div>

        {/* Separador */}
        <div className="my-6 border-t-2 border-dashed border-gray-400"></div>

        {/* Poltronas extras do fundo */}
        <div>
          <div className="text-center text-sm text-amber-700 dark:text-amber-300 mb-3 font-semibold">
            Poltronas do Fundo
          </div>
          <div className="space-y-2">
            {/* Primeira fileira: 29, 27, 30 */}
            <div className="flex gap-2 justify-center">
              {backSeatsFirstRow.map((seatNumber) => renderSeat(seatNumber))}
            </div>

            {/* Segunda fileira: 28, vazio, 31 */}
            <div className="flex gap-2 justify-center">
              {backSeatsSecondRow.map((seatNumber) => renderSeat(seatNumber))}
            </div>
          </div>
        </div>
      </div>

      {/* Poltrona selecionada */}
      {selectedSeat && (
        <div className="text-center p-4 bg-yellow-50 dark:bg-yellow-950 border border-yellow-200 dark:border-yellow-800 rounded-lg">
          <p className="font-bold text-lg">Poltrona Selecionada: {selectedSeat}</p>
        </div>
      )}
    </div>
  );
}