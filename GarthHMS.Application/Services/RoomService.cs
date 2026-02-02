// GarthHMS.Application/Services/RoomService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.Room;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Enums;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio de habitaciones con lógica de negocio y validaciones
    /// </summary>
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly ILogger<RoomService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoomService(
            IRoomRepository roomRepository,
            IRoomTypeRepository roomTypeRepository,
            ILogger<RoomService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _roomRepository = roomRepository;
            _roomTypeRepository = roomTypeRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid CurrentHotelId => GetCurrentHotelId();
        private Guid CurrentUserId => GetCurrentUserId();

        // ====================================================================
        // CRUD
        // ====================================================================

        public async Task<IEnumerable<RoomResponseDto>> GetAllAsync()
        {
            var rooms = await _roomRepository.GetByHotelAsync(CurrentHotelId);

            // Obtener tipos de habitación para enriquecer los datos
            var roomTypes = await _roomTypeRepository.GetByHotelAsync(CurrentHotelId);
            var roomTypeDict = roomTypes.ToDictionary(rt => rt.RoomTypeId);

            return rooms.Select(room => MapToResponseDto(room, roomTypeDict));
        }

        public async Task<IEnumerable<RoomResponseDto>> GetAllActiveAsync()
        {
            var rooms = await _roomRepository.GetAllActiveAsync(CurrentHotelId);

            var roomTypes = await _roomTypeRepository.GetByHotelAsync(CurrentHotelId);
            var roomTypeDict = roomTypes.ToDictionary(rt => rt.RoomTypeId);

            return rooms.Select(room => MapToResponseDto(room, roomTypeDict));
        }

        public async Task<RoomResponseDto?> GetByIdAsync(Guid roomId)
        {
            var room = await _roomRepository.GetByIdAsync(roomId);

            if (room == null)
                return null;

            // Validar multi-tenancy
            if (room.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para acceder a esta habitación");

            var roomTypes = await _roomTypeRepository.GetByHotelAsync(CurrentHotelId);
            var roomTypeDict = roomTypes.ToDictionary(rt => rt.RoomTypeId);

            return MapToResponseDto(room, roomTypeDict);
        }

        public async Task<Guid> CreateAsync(CreateRoomDto dto)
        {
            // 1. Validar que el tipo de habitación exista y pertenezca al mismo hotel
            var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
            if (roomType == null)
                throw new KeyNotFoundException("El tipo de habitación no existe");

            if (roomType.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("El tipo de habitación no pertenece a este hotel");

            if (!roomType.IsActive)
                throw new InvalidOperationException("El tipo de habitación está inactivo");

            // 2. Validar que el número de habitación no exista
            if (await _roomRepository.RoomNumberExistsAsync(CurrentHotelId, dto.RoomNumber))
                throw new InvalidOperationException($"Ya existe una habitación con el número '{dto.RoomNumber}'");

            // 3. Validar piso
            if (dto.Floor < 0)
                throw new InvalidOperationException("El piso no puede ser negativo");

            // 4. Crear la habitación
            var room = new Room
            {
                HotelId = CurrentHotelId,
                RoomTypeId = dto.RoomTypeId,
                RoomNumber = dto.RoomNumber.Trim().ToUpper(),
                Floor = dto.Floor,
                Notes = dto.Notes?.Trim(),
                Status = RoomStatus.Available,
                StatusChangedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = CurrentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var roomId = await _roomRepository.CreateAsync(room);

            _logger.LogInformation(
                "Habitación creada: {RoomNumber} (Tipo: {RoomTypeCode}) - ID: {RoomId}",
                room.RoomNumber, roomType.Code, roomId);

            return roomId;
        }

        public async Task UpdateAsync(UpdateRoomDto dto)
        {
            // 1. Validar que la habitación exista
            var existingRoom = await _roomRepository.GetByIdAsync(dto.RoomId);
            if (existingRoom == null)
                throw new KeyNotFoundException($"No se encontró la habitación con ID {dto.RoomId}");

            // 2. Validar multi-tenancy
            if (existingRoom.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para modificar esta habitación");

            // 3. Validar número de habitación único (excluyendo la actual)
            if (await _roomRepository.RoomNumberExistsAsync(CurrentHotelId, dto.RoomNumber, dto.RoomId))
                throw new InvalidOperationException($"Ya existe otra habitación con el número '{dto.RoomNumber}'");

            // 4. Validar piso
            if (dto.Floor < 0)
                throw new InvalidOperationException("El piso no puede ser negativo");

            // 5. Actualizar
            existingRoom.RoomNumber = dto.RoomNumber.Trim().ToUpper();
            existingRoom.Floor = dto.Floor;
            existingRoom.Notes = dto.Notes?.Trim();
            existingRoom.UpdatedAt = DateTime.UtcNow;

            await _roomRepository.UpdateAsync(existingRoom);

            _logger.LogInformation("Habitación actualizada: {RoomNumber} - ID: {RoomId}",
                existingRoom.RoomNumber, dto.RoomId);
        }

        public async Task DeleteAsync(Guid roomId)
        {
            // 1. Validar que exista
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null)
                throw new KeyNotFoundException($"No se encontró la habitación con ID {roomId}");

            // 2. Validar multi-tenancy
            if (room.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para eliminar esta habitación");

            // 3. Validar que no esté ocupada
            if (room.CurrentStayId.HasValue)
                throw new InvalidOperationException("No se puede eliminar una habitación ocupada");

            // 4. Validar estado
            if (room.Status == RoomStatus.Occupied || room.Status == RoomStatus.Reserved)
                throw new InvalidOperationException($"No se puede eliminar una habitación en estado {room.Status}");

            // 5. Soft delete
            await _roomRepository.DeleteAsync(roomId);

            _logger.LogInformation("Habitación eliminada (soft delete): {RoomNumber} - ID: {RoomId}",
                room.RoomNumber, roomId);
        }

        // ====================================================================
        // CONSULTAS POR FILTROS
        // ====================================================================

        public async Task<IEnumerable<RoomResponseDto>> GetByTypeAsync(Guid roomTypeId)
        {
            // Validar que el tipo pertenezca al hotel actual
            var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
            if (roomType == null || roomType.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("El tipo de habitación no pertenece a este hotel");

            var rooms = await _roomRepository.GetByTypeAsync(roomTypeId);

            var roomTypes = await _roomTypeRepository.GetByHotelAsync(CurrentHotelId);
            var roomTypeDict = roomTypes.ToDictionary(rt => rt.RoomTypeId);

            return rooms.Select(room => MapToResponseDto(room, roomTypeDict));
        }

        public async Task<IEnumerable<RoomResponseDto>> GetByStatusAsync(RoomStatus status)
        {
            var rooms = await _roomRepository.GetByStatusAsync(CurrentHotelId, status);

            var roomTypes = await _roomTypeRepository.GetByHotelAsync(CurrentHotelId);
            var roomTypeDict = roomTypes.ToDictionary(rt => rt.RoomTypeId);

            return rooms.Select(room => MapToResponseDto(room, roomTypeDict));
        }

        public async Task<IEnumerable<RoomResponseDto>> GetAvailableAsync()
        {
            var rooms = await _roomRepository.GetAvailableAsync(CurrentHotelId);

            var roomTypes = await _roomTypeRepository.GetByHotelAsync(CurrentHotelId);
            var roomTypeDict = roomTypes.ToDictionary(rt => rt.RoomTypeId);

            return rooms.Select(room => MapToResponseDto(room, roomTypeDict));
        }

        // ====================================================================
        // GESTIÓN DE ESTADOS
        // ====================================================================

        public async Task UpdateStatusAsync(Guid roomId, RoomStatus newStatus)
        {
            // 1. Validar que exista
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null)
                throw new KeyNotFoundException($"No se encontró la habitación con ID {roomId}");

            // 2. Validar multi-tenancy
            if (room.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para modificar esta habitación");

            // 3. COMENTAR O ELIMINAR ESTA VALIDACIÓN
            /*
            if (room.Status == newStatus)
                throw new InvalidOperationException($"La habitación ya está en estado {newStatus}");
            */

            // 4. Actualizar estado
            await _roomRepository.UpdateStatusAsync(roomId, newStatus, CurrentUserId);

            _logger.LogInformation("Estado de habitación actualizado: {RoomNumber} - {OldStatus} → {NewStatus}",
                room.RoomNumber, room.Status, newStatus);
        }

        public async Task SetMaintenanceAsync(Guid roomId, string? notes)
        {
            // 1. Validar que exista
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null)
                throw new KeyNotFoundException($"No se encontró la habitación con ID {roomId}");

            // 2. Validar multi-tenancy
            if (room.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para modificar esta habitación");

            // 3. No se puede poner en mantenimiento si está ocupada
            if (room.CurrentStayId.HasValue)
                throw new InvalidOperationException("No se puede poner en mantenimiento una habitación ocupada");

            // 4. Actualizar a mantenimiento
            await _roomRepository.SetMaintenanceAsync(roomId, notes, CurrentUserId);

            _logger.LogInformation("Habitación puesta en mantenimiento: {RoomNumber} - Razón: {Notes}",
                room.RoomNumber, notes ?? "Sin especificar");
        }

        public async Task SetAvailableAsync(Guid roomId)
        {
            // 1. Validar que exista
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null)
                throw new KeyNotFoundException($"No se encontró la habitación con ID {roomId}");

            // 2. Validar multi-tenancy
            if (room.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para modificar esta habitación");

            // 3. Marcar como disponible (limpia current_stay_id)
            await _roomRepository.SetAvailableAsync(roomId, CurrentUserId);

            _logger.LogInformation("Habitación marcada como disponible: {RoomNumber}", room.RoomNumber);
        }

        // ====================================================================
        // HELPERS PRIVADOS
        // ====================================================================

        private RoomResponseDto MapToResponseDto(Room room, Dictionary<Guid, RoomType> roomTypeDict)
        {
            var roomType = roomTypeDict.GetValueOrDefault(room.RoomTypeId);

            return new RoomResponseDto
            {
                RoomId = room.RoomId,
                HotelId = room.HotelId,
                RoomTypeId = room.RoomTypeId,
                RoomNumber = room.RoomNumber,
                Floor = room.Floor,
                Status = room.Status,
                StatusText = GetStatusText(room.Status),
                StatusChangedAt = room.StatusChangedAt,
                StatusChangedBy = room.StatusChangedBy,
                CurrentStayId = room.CurrentStayId,
                Notes = room.Notes,
                IsActive = room.IsActive,
                RoomTypeName = roomType?.Name ?? "Sin tipo",
                RoomTypeCode = roomType?.Code ?? "N/A",
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };
        }

        private string GetStatusText(RoomStatus status)
        {
            return status switch
            {
                RoomStatus.Available => "Disponible",
                RoomStatus.Occupied => "Ocupada",
                RoomStatus.Dirty => "Sucia",
                RoomStatus.Cleaning => "En Limpieza",
                RoomStatus.Maintenance => "Mantenimiento",
                RoomStatus.Reserved => "Reservada",
                _ => "Desconocido"
            };
        }

        private Guid GetCurrentHotelId()
        {
            var hotelIdClaim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("HotelId")?.Value;

            if (string.IsNullOrEmpty(hotelIdClaim) || !Guid.TryParse(hotelIdClaim, out var hotelId))
                throw new UnauthorizedAccessException("No se pudo obtener el HotelId del usuario actual");

            return hotelId;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("No se pudo obtener el UserId del usuario actual");

            return userId;
        }
    }
}