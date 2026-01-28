using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Enums
{
    /// <summary>
    /// Métodos de pago aceptados
    /// </summary>
    public enum PaymentMethod
    {
        Cash = 1,           // Efectivo
        Card = 2,           // Tarjeta débito/crédito
        Transfer = 3,       // Transferencia bancaria
        Deposit = 4,        // Depósito bancario
        MercadoPago = 5,    // MercadoPago
        PayPal = 6,         // PayPal
        Other = 99          // Otro método
    }
}
