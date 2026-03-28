// GarthHMS.Core/DTOs/Reservation/ReservationDetailDto.cs
using System;
using System.Collections.Generic;

namespace GarthHMS.Core.DTOs.Reservation
{
    public class ReservationDetailDto
    {
        public Guid ReservationId { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ReservationType { get; set; } = "nightly";
        public string Source { get; set; } = string.Empty;

        public Guid GuestId { get; set; }
        public string GuestFirstName { get; set; } = string.Empty;
        public string GuestLastName { get; set; } = string.Empty;
        public string? GuestPhone { get; set; }
        public string? GuestEmail { get; set; }
        public bool GuestIsVip { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumNights { get; set; }

        public int TotalAdults { get; set; }
        public int TotalChildren { get; set; }
        public int TotalBabies { get; set; }
        public string? TravelReason { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? DiscountReason { get; set; }
        public decimal TaxesAmount { get; set; }
        public decimal Total { get; set; }

        public decimal DepositAmount { get; set; }
        public DateTime? DepositPaidAt { get; set; }
        public string? DepositPaymentMethod { get; set; }
        public string? DepositReference { get; set; }
        public string? DepositProofUrl { get; set; }
        public DateTime? DepositDueDate { get; set; }
        public Guid? DepositValidatedBy { get; set; }
        public decimal BalancePending { get; set; }

        public string? GuestNotes { get; set; }
        public string? InternalNotes { get; set; }

        public List<ReservationRoomDetailDto> Rooms { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        // ── Propiedades calculadas ──────────────────────────────────────────
        public string GuestFullName => $"{GuestFirstName} {GuestLastName}".Trim();
        public bool HasDeposit => DepositPaidAt.HasValue;

        public string StatusLabel => Status switch
        {
            "draft" => "Borrador",
            "pending" => "Pendiente de anticipo",
            "confirmed" => "Confirmada",
            "checked_in" => "Hospedado",
            "checked_out" => "Finalizada",
            "cancelled" => "Cancelada",
            "no_show" => "No se presentó",
            _ => Status
        };

        public string SourceLabel => Source switch
        {
            "direct" => "Directo",
            "phone" => "Teléfono",
            "whatsapp" => "WhatsApp",
            "walk_in" => "Walk-in",
            "ota_booking" => "Booking.com",
            "ota_airbnb" => "Airbnb",
            "ota_expedia" => "Expedia",
            _ => Source
        };

        public string TravelReasonLabel => TravelReason switch
        {
            "leisure" => "Placer",
            "business" => "Negocios",
            "event" => "Evento",
            "other" => "Otro",
            _ => TravelReason ?? ""
        };
    }

    public class ReservationRoomDetailDto
    {
        public Guid ReservationRoomId { get; set; }
        public Guid RoomId { get; set; }
        public Guid RoomTypeId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int? Floor { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string RoomTypeCode { get; set; } = string.Empty;
        public int NumAdults { get; set; }
        public int NumChildren { get; set; }
        public int NumBabies { get; set; }
        public bool HasPets { get; set; }
        public int NumPets { get; set; }
        public decimal PetChargeApplied { get; set; }
        public string? VehiclePlate { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal ExtraPersonCharge { get; set; }
        public decimal Subtotal { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ReservationListDto
    {
        public Guid ReservationId { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumNights { get; set; }
        public int TotalAdults { get; set; }
        public int TotalChildren { get; set; }
        public int TotalBabies { get; set; }
        public Guid GuestId { get; set; }
        public string GuestFullName { get; set; } = string.Empty;
        public string? GuestPhone { get; set; }
        public bool GuestIsVip { get; set; }
        public string RoomsSummary { get; set; } = string.Empty;
        public long RoomCount { get; set; }
        public decimal Total { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal BalancePending { get; set; }
        public DateTime? DepositPaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public long TotalCount { get; set; }

        public string StatusLabel => Status switch
        {
            "draft" => "Borrador",
            "pending" => "Pend. Anticipo",
            "confirmed" => "Confirmada",
            "checked_in" => "Hospedado",
            "checked_out" => "Finalizada",
            "cancelled" => "Cancelada",
            "no_show" => "No Show",
            _ => Status
        };

        public bool HasDeposit => DepositPaidAt.HasValue;
        public bool IsCheckInToday => CheckInDate.Date == DateTime.Today;
    }

    public class ReservationFormConfigDto
    {
        public string OperationMode { get; set; } = "hotel";
        public bool CanInvoice { get; set; } = false;
        public bool ChargesTaxes { get; set; } = true;
        public decimal TaxIvaPercent { get; set; } = 16;
        public decimal TaxIshPercent { get; set; } = 3;
        public int MinDepositPercent { get; set; } = 50;
        public int MaxPetsPerRoom { get; set; } = 3;
        public bool HasParking { get; set; } = false;
        public int ParkingCapacity { get; set; } = 0;
        public bool RequireCompanionDetails { get; set; } = false;
        public bool CompanionsOptional { get; set; } = false;
    }
}