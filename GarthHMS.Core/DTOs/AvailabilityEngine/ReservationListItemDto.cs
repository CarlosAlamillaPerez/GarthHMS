// GarthHMS.Core/DTOs/AvailabilityEngine/ReservationListItemDto.cs
namespace GarthHMS.Core.DTOs.AvailabilityEngine
{
    /// <summary>
    /// Ítem de reserva para el panel derecho del calendario
    /// SP: sp_availability_get_reservations_by_date,
    ///     sp_availability_get_upcoming_reservations,
    ///     sp_availability_get_today_reservations
    /// </summary>
    public class ReservationListItemDto
    {
        public Guid ReservationId { get; set; }
        public string Folio { get; set; } = string.Empty;

        // Estado y tipo
        public string Status { get; set; } = string.Empty;
        public string ReservationType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;

        // Fechas
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int? NumNights { get; set; }

        // Financiero
        public decimal Total { get; set; }
        public decimal DepositAmount { get; set; }
        public DateTime? DepositPaidAt { get; set; }
        public decimal BalancePending { get; set; }
        public bool HasUnverifiedPayments { get; set; }

        // Huésped
        public Guid GuestId { get; set; }
        public string GuestFirstName { get; set; } = string.Empty;
        public string GuestLastName { get; set; } = string.Empty;
        public string GuestPhone { get; set; } = string.Empty;
        public bool IsVip { get; set; }

        // Habitaciones
        public string RoomsSummary { get; set; } = string.Empty;
        public long RoomCount { get; set; }

        // Propiedades calculadas
        public string GuestFullName => $"{GuestFirstName} {GuestLastName}".Trim();

        /// <summary>
        /// True si el anticipo fue pagado
        /// </summary>
        public bool HasDeposit => DepositPaidAt.HasValue;

        /// <summary>
        /// True si el check-in es hoy
        /// </summary>
        public bool IsCheckInToday => CheckInDate.Date == DateTime.Today;

        /// <summary>
        /// Etiqueta de canal en español
        /// </summary>
        public string SourceLabel => Source?.ToLower() switch
        {
            "direct" => "Directo",
            "whatsapp" => "WhatsApp",
            "booking" => "Booking",
            "airbnb" => "Airbnb",
            "expedia" => "Expedia",
            "walkin" => "Walk-in",
            _ => "Otro"
        };

        /// <summary>
        /// Etiqueta de estado en español
        /// </summary>
        public string StatusLabel => Status?.ToLower() switch
        {
            "pending" => "Pendiente",
            "confirmed" => "Confirmada",
            "checked_in" => "Check-in",
            "checked_out" => "Check-out",
            "cancelled" => "Cancelada",
            "no_show" => "No Show",
            _ => Status
        };
    }
}