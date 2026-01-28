using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Enums
{
    /// <summary>
    /// Estados de una reservación
    /// </summary>
    public enum ReservationStatus
    {
        Pending = 1,              // Pendiente - Esperando anticipo
        Confirmed = 2,            // Confirmada - Anticipo validado
        CheckedIn = 3,            // En casa - Guest ya hizo check-in
        CheckedOut = 4,           // Completada - Guest salió
        Cancelled = 5,            // Cancelada - Por guest o hotel
        NoShow = 6,               // No show - Guest nunca llegó
        PendingValidation = 7     // Pendiente validación - Anticipo subido
    }
}
