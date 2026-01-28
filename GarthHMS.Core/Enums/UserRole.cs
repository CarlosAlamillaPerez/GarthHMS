using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Enums
{
    /// <summary>
    /// Roles predefinidos del sistema (solo para SuperAdmin)
    /// Los hoteles pueden crear roles personalizados
    /// </summary>
    public enum UserRole
    {
        SuperAdmin = 1,      // Carlos - Acceso total a todo
        HotelAdmin = 2,      // Gerente del hotel - Acceso total al hotel
        Manager = 3,         // Manager - Permisos configurables
        Receptionist = 4,    // Recepcionista - Operaciones diarias
        Housekeeping = 5,    // Camarista - Limpieza
        Maintenance = 6,     // Mantenimiento - Reparaciones
        Custom = 99          // Rol personalizado del hotel
    }
}
