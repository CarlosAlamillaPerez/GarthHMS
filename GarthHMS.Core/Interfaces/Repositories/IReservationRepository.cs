// GarthHMS.Core/Interfaces/Repositories/IReservationRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Reservation;
using GarthHMS.Core.Models;

namespace GarthHMS.Core.Interfaces.Repositories
{
    public interface IReservationRepository
    {
        /// <summary>Crea una reserva nightly y sus habitaciones. Retorna (reservationId, folio).</summary>
        Task<(Guid ReservationId, string Folio)> CreateNightlyAsync(
            Guid hotelId,
            CreateReservationDto dto,
            Guid createdBy);

        /// <summary>Obtiene detalle completo de una reserva.</summary>
        Task<ReservationDetailDto?> GetByIdAsync(Guid hotelId, Guid reservationId);

        /// <summary>Lista reservas con filtros y paginación.</summary>
        Task<(IEnumerable<ReservationListDto> Items, long TotalCount)> GetListAsync(
            Guid hotelId,
            string? search = null,
            string? status = null,
            string? source = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int pageNumber = 1,
            int pageSize = 20);

        /// <summary>Cancela una reserva.</summary>
        Task<bool> CancelAsync(Guid hotelId, Guid reservationId, Guid cancelledBy, string? reason = null);

        /// <summary>Actualiza el estado de una reserva.</summary>
        Task<bool> UpdateStatusAsync(Guid hotelId, Guid reservationId, string newStatus, Guid changedBy);

        /// <summary>Obtiene configuración del hotel para el formulario.</summary>
        Task<ReservationFormConfigDto?> GetFormConfigAsync(Guid hotelId);

        /// <summary>Actualiza una reserva nightly existente.</summary>
        Task<bool> UpdateNightlyAsync(Guid hotelId, UpdateReservationDto dto, Guid updatedBy);
    }
}