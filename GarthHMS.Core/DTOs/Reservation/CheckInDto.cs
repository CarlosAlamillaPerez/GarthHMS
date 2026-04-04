// GarthHMS.Core/DTOs/Reservation/CheckInDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GarthHMS.Core.DTOs.Reservation
{
    public class CheckInDto
    {
        [Required]
        public Guid ReservationId { get; set; }

        // Email del huésped — solo se envía si no tenía o se actualizó
        public string? GuestEmail { get; set; }

        // Placas por habitación
        public List<CheckInVehiclePlateDto> VehiclePlates { get; set; } = new();

        // Acompañantes por habitación
        public List<CheckInRoomCompanionsDto> Companions { get; set; } = new();
    }

    public class CheckInVehiclePlateDto
    {
        public Guid RoomId { get; set; }
        public string? Plate { get; set; }
    }

    public class CheckInRoomCompanionsDto
    {
        public Guid ReservationRoomId { get; set; }
        public List<CheckInCompanionDto> Companions { get; set; } = new();
    }

    public class CheckInCompanionDto
    {
        public string? FullName { get; set; }
        public string? Age { get; set; }
        public string? Phone { get; set; }
        public string? Relationship { get; set; }
    }
}