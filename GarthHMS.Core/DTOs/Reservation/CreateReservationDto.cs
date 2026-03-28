// GarthHMS.Core/DTOs/Reservation/CreateReservationDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.Reservation
{
    public class CreateReservationDto
    {
        // ─── Huésped ───────────────────────────────────────────────────────
        [Required(ErrorMessage = "El huésped es requerido")]
        public Guid GuestId { get; set; }

        // ─── Origen ────────────────────────────────────────────────────────
        [Required(ErrorMessage = "El canal de reserva es requerido")]
        public string Source { get; set; } = "direct";

        // ─── Fechas ────────────────────────────────────────────────────────
        [Required(ErrorMessage = "La fecha de check-in es requerida")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "La fecha de check-out es requerida")]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 365, ErrorMessage = "El número de noches debe ser entre 1 y 365")]
        public int NumNights { get; set; }

        // ─── Huéspedes ─────────────────────────────────────────────────────
        [Range(1, 20, ErrorMessage = "Mínimo 1 adulto")]
        public int TotalAdults { get; set; } = 1;

        [Range(0, 10, ErrorMessage = "Valor inválido para niños")]
        public int TotalChildren { get; set; } = 0;

        [Range(0, 5, ErrorMessage = "Valor inválido para bebés")]
        public int TotalBabies { get; set; } = 0;

        // ─── Servicios ─────────────────────────────────────────────────────
        public string? TravelReason { get; set; }

        // ─── Finanzas ──────────────────────────────────────────────────────
        [Range(0, 9999999.99, ErrorMessage = "Subtotal inválido")]
        public decimal Subtotal { get; set; }

        [Range(0, 9999999.99, ErrorMessage = "Descuento inválido")]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "El porcentaje de descuento debe ser entre 0 y 100")]
        public decimal DiscountPercent { get; set; } = 0;

        public string? DiscountReason { get; set; }

        [Range(0, 9999999.99, ErrorMessage = "Impuestos inválidos")]
        public decimal TaxesAmount { get; set; } = 0;

        [Range(0, 9999999.99, ErrorMessage = "Total inválido")]
        public decimal Total { get; set; }

        public bool RequiresInvoice { get; set; } = false;

        // ─── Anticipo ──────────────────────────────────────────────────────
        public bool RequiresDeposit { get; set; } = true;

        [Range(0, 9999999.99, ErrorMessage = "Monto de anticipo inválido")]
        public decimal DepositAmount { get; set; } = 0;

        public string? DepositPaymentMethod { get; set; }
        public string? DepositReference { get; set; }
        public string? DepositProofUrl { get; set; }
        public DateTime? DepositDueDate { get; set; }

        // ─── Notas ─────────────────────────────────────────────────────────
        public string? GuestNotes { get; set; }
        public string? InternalNotes { get; set; }

        // ─── Estado ────────────────────────────────────────────────────────
        [Required]
        public string Status { get; set; } = "pending";

        // ─── Habitaciones ──────────────────────────────────────────────────
        [MinLength(1, ErrorMessage = "Debes agregar al menos una habitación")]
        public List<CreateReservationRoomDto> Rooms { get; set; } = new();
        public List<CreateCompanionDto> Companions { get; set; } = new();
    }

    public class CreateCompanionDto
    {
        public string FullName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Phone { get; set; }
        public string? Relationship { get; set; }
    }

    public class CreateReservationRoomDto
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        public Guid RoomTypeId { get; set; }

        public string? RoomNumber { get; set; }
        public string? RoomTypeName { get; set; }
        public int? Floor { get; set; }

        [Range(1, 20, ErrorMessage = "Mínimo 1 adulto por habitación")]
        public int NumAdults { get; set; } = 1;

        [Range(0, 10)]
        public int NumChildren { get; set; } = 0;

        [Range(0, 5)]
        public int NumBabies { get; set; } = 0;

        public bool HasPets { get; set; } = false;

        [Range(0, 10)]
        public int NumPets { get; set; } = 0;

        [Range(0, 99999.99)]
        public decimal PetChargeApplied { get; set; } = 0;

        public string? VehiclePlate { get; set; }
        public string? VehicleDescription { get; set; }

        [Range(0, 9999999.99)]
        public decimal PricePerNight { get; set; }

        [Range(0, 9999999.99)]
        public decimal ExtraPersonCharge { get; set; } = 0;

        [Range(0, 9999999.99)]
        public decimal Subtotal { get; set; }

        public List<CreateCompanionDto> Companions { get; set; } = new();
    }
}