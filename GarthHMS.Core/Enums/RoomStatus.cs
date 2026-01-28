using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Enums
{
    /// <summary>
    /// Estados de una habitación
    /// </summary>
    public enum RoomStatus
    {
        Available = 1,       // Disponible - Limpia y lista
        Occupied = 2,        // Ocupada - Guest activo
        Dirty = 3,           // Sucia - Necesita limpieza
        Cleaning = 4,        // En limpieza - Camarista trabajando
        Maintenance = 5,     // Mantenimiento - Bloqueada por reparación
        OutOfOrder = 6,      // Fuera de servicio - Bloqueada permanentemente
        Reserved = 7         // Reservada - Asignada a reserva futura
    }
}