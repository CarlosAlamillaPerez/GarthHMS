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

        /// <summary>Registra un abono o devolución en una reserva.</summary>
        Task<ServiceResult<(Guid PaymentId, decimal NewBalance, string NewStatus, bool HasUnverified)>> AddPaymentAsync(
            Guid hotelId,
            Guid reservationId,
            decimal amount,
            string paymentMethod,
            string paymentType,
            string? reference,
            Guid registeredBy,
            bool isManagerOrAdmin);

        /// <summary>Lista los pagos de una reserva.</summary>
        Task<IEnumerable<ReservationPaymentDto>> GetPaymentsAsync(Guid hotelId,Guid reservationId);

        /// <summary>Realiza el check-in de una reserva.</summary>
        Task<ServiceResult<bool>> CheckInAsync(Guid hotelId,CheckInDto dto,Guid checkedInBy);

        Task<ServiceResult<bool>> UpdateVehicleAsync(Guid hotelId, Guid reservationRoomId,string? vehiclePlate, string? vehicleDescription);

        Task<ServiceResult<bool>> CheckOutAsync(Guid hotelId, Guid reservationId, Guid checkedOutBy);


    }
}
