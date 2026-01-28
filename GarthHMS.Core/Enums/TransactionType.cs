using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Enums
{
    /// <summary>
    /// Tipos de transacciones financieras
    /// </summary>
    public enum TransactionType
    {
        // INGRESOS
        Deposit = 1,              // Anticipo de reserva
        RoomPayment = 2,          // Pago de hospedaje
        ExtraCharge = 3,          // Cargo extra (minibar, late checkout, etc)
        ServiceCharge = 4,        // Cargo por servicio

        // EGRESOS
        Refund = 10,              // Reembolso a guest
        Expense = 11,             // Gasto operativo
        HousekeepingVoucher = 12, // Voucher de camarista
        Salary = 13,              // Pago de nómina

        // AJUSTES
        Adjustment = 20,          // Ajuste manual
        Discount = 21             // Descuento aplicado
    }
}
