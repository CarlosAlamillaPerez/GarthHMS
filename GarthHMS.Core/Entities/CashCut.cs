using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarthHMS.Core.Entities
{
    /// <summary>
    /// Corte de caja (cierre de turno)
    /// </summary>
    public class CashCut
    {
        public int CashCutId { get; set; }
        public int HotelId { get; set; }

        // FOLIO
        public string CutNumber { get; set; } = string.Empty;  // "CUT-2025-00001"

        // TURNO
        public DateTime ShiftStartDate { get; set; }
        public DateTime ShiftEndDate { get; set; }
        public int CashierId { get; set; }  // Usuario que hizo el corte

        // MONTOS ESPERADOS
        public decimal ExpectedCash { get; set; }
        public decimal ExpectedCard { get; set; }
        public decimal ExpectedTransfer { get; set; }
        public decimal ExpectedTotal { get; set; }

        // MONTOS REALES
        public decimal ActualCash { get; set; }
        public decimal ActualCard { get; set; }
        public decimal ActualTransfer { get; set; }
        public decimal ActualTotal { get; set; }

        // DIFERENCIAS
        public decimal CashDifference { get; set; }
        public decimal CardDifference { get; set; }
        public decimal TransferDifference { get; set; }
        public decimal TotalDifference { get; set; }

        // JUSTIFICACIÓN
        public string? Notes { get; set; }
        public string? DifferenceJustification { get; set; }

        // VALIDACIÓN
        public bool IsApproved { get; set; } = false;
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // AUDITORÍA
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
