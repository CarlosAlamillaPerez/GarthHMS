using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Enums
{
    /// <summary>
    /// Tipos de permisos del sistema
    /// Los permisos se asignan a roles personalizados
    /// </summary>
    public enum PermissionType
    {
        // DASHBOARD
        ViewDashboard = 1,

        // RESERVAS
        ViewReservations = 10,
        CreateReservation = 11,
        EditReservation = 12,
        CancelReservation = 13,
        ValidateDeposit = 14,

        // HOSPEDAJE
        ViewGuests = 20,
        DoCheckIn = 21,
        DoCheckOut = 22,
        AddExtraCharges = 23,

        // HABITACIONES
        ViewRooms = 30,
        ChangeRoomStatus = 31,
        AssignCleaning = 32,
        ReportMaintenance = 33,

        // FINANZAS
        ViewTransactions = 40,
        CreateTransaction = 41,
        ViewReports = 42,
        CloseCashRegister = 43,
        ViewPricing = 44,
        EditPricing = 45,
        ApproveDiscounts = 46,

        // CONFIGURACIÓN
        ViewConfiguration = 50,
        EditHotelSettings = 51,
        ManageRoomTypes = 52,
        ManageRooms = 53,
        ManageUsers = 54,
        ManageRoles = 55,
        ViewAuditLog = 56
    }
}
