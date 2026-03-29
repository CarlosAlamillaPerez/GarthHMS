// GarthHMS.Core/Interfaces/Services/IReservationService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Reservation;
using GarthHMS.Core.Models;

namespace GarthHMS.Core.Interfaces.Services
{
    public interface IReservationService
    {
        /// <summary>Crea una reserva nightly (draft o pending).</summary>
        Task<ServiceResult<(Guid ReservationId, string Folio)>> CreateNightlyAsync(
            Guid hotelId,
            CreateReservationDto dto,
            Guid createdBy);

        /// <summary>Obtiene detalle completo de una reserva.</summary>
        Task<ReservationDetailDto?> GetByIdAsync(Guid hotelId, Guid reservationId);

        /// <summary>Lista reservas con filtros.</summary>
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
        Task<ServiceResult<bool>> CancelAsync(Guid hotelId, Guid reservationId, Guid cancelledBy, string? reason = null);

        /// <summary>Obtiene configuración del hotel para el formulario.</summary>
        Task<ReservationFormConfigDto?> GetFormConfigAsync(Guid hotelId);

        /// <summary>Edita una reserva nightly (draft/pending/confirmed).</summary>
        Task<ServiceResult<bool>> UpdateNightlyAsync(
            Guid hotelId, UpdateReservationDto dto, Guid updatedBy);
    }
}
