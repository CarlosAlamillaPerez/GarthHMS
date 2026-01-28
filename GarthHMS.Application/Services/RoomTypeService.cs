using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio para gestión de tipos de habitación
    /// </summary>
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly ILogger<RoomTypeService> _logger;

        public RoomTypeService(
            IRoomTypeRepository roomTypeRepository,
            ILogger<RoomTypeService> logger)
        {
            _roomTypeRepository = roomTypeRepository;
            _logger = logger;
        }

        // CRUD

        public async Task<(bool Success, int RoomTypeId, string? ErrorMessage)> CreateRoomTypeAsync(
            int hotelId,
            string typeName,
            string? description,
            int maxOccupancy,
            decimal basePrice,
            decimal? weekendPrice,
            string? amenitiesJson,
            int createdBy)
        {
            try
            {
                // Validar que el nombre no exista
                var nameExists = await _roomTypeRepository.TypeNameExistsAsync(hotelId, typeName);
                if (nameExists)
                {
                    return (false, 0, "Ya existe un tipo de habitación con ese nombre");
                }

                // Validar precio base
                if (basePrice <= 0)
                {
                    return (false, 0, "El precio base debe ser mayor a 0");
                }

                var roomType = new RoomType
                {
                    HotelId = hotelId,
                    TypeName = typeName,
                    Description = description,
                    MaxOccupancy = maxOccupancy,
                    BasePrice = basePrice,
                    WeekendPrice = weekendPrice,
                    AmenitiesJson = amenitiesJson ?? "[]",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                var roomTypeId = await _roomTypeRepository.CreateAsync(roomType);

                if (roomTypeId > 0)
                {
                    _logger.LogInformation("Room type created: {TypeName} for hotel {HotelId} by user {CreatedBy}", 
                        typeName, hotelId, createdBy);
                    return (true, roomTypeId, null);
                }

                return (false, 0, "Error al crear el tipo de habitación");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room type {TypeName} for hotel {HotelId}", typeName, hotelId);
                return (false, 0, "Error al crear el tipo de habitación");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateRoomTypeAsync(
            int roomTypeId,
            string typeName,
            string? description,
            int maxOccupancy,
            decimal basePrice,
            decimal? weekendPrice,
            string? amenitiesJson,
            int updatedBy)
        {
            try
            {
                var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
                if (roomType == null)
                {
                    return (false, "Tipo de habitación no encontrado");
                }

                // Validar nombre único (excluyendo el actual)
                var nameExists = await _roomTypeRepository.TypeNameExistsAsync(
                    roomType.HotelId, typeName, roomTypeId);
                if (nameExists)
                {
                    return (false, "Ya existe un tipo de habitación con ese nombre");
                }

                // Validar precio
                if (basePrice <= 0)
                {
                    return (false, "El precio base debe ser mayor a 0");
                }

                roomType.TypeName = typeName;
                roomType.Description = description;
                roomType.MaxOccupancy = maxOccupancy;
                roomType.BasePrice = basePrice;
                roomType.WeekendPrice = weekendPrice;
                roomType.AmenitiesJson = amenitiesJson ?? "[]";
                roomType.UpdatedAt = DateTime.UtcNow;
                roomType.UpdatedBy = updatedBy;

                var result = await _roomTypeRepository.UpdateAsync(roomType);

                if (result)
                {
                    _logger.LogInformation("Room type {RoomTypeId} updated by user {UpdatedBy}", roomTypeId, updatedBy);
                    return (true, null);
                }

                return (false, "Error al actualizar el tipo de habitación");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room type {RoomTypeId}", roomTypeId);
                return (false, "Error al actualizar el tipo de habitación");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteRoomTypeAsync(int roomTypeId, int deletedBy)
        {
            try
            {
                // Verificar si hay habitaciones asociadas
                var totalRooms = await _roomTypeRepository.GetTotalRoomsCountAsync(roomTypeId);
                if (totalRooms > 0)
                {
                    return (false, $"No se puede eliminar. Hay {totalRooms} habitaciones asociadas a este tipo");
                }

                // Desactivar en lugar de eliminar
                var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
                if (roomType == null)
                {
                    return (false, "Tipo de habitación no encontrado");
                }

                roomType.IsActive = false;
                roomType.UpdatedAt = DateTime.UtcNow;
                roomType.UpdatedBy = deletedBy;

                var result = await _roomTypeRepository.UpdateAsync(roomType);

                if (result)
                {
                    _logger.LogInformation("Room type {RoomTypeId} deleted by user {DeletedBy}", roomTypeId, deletedBy);
                    return (true, null);
                }

                return (false, "Error al eliminar el tipo de habitación");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room type {RoomTypeId}", roomTypeId);
                return (false, "Error al eliminar el tipo de habitación");
            }
        }

        // CONSULTAS

        public async Task<RoomTypeDto?> GetRoomTypeByIdAsync(int roomTypeId)
        {
            try
            {
                var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
                return roomType == null ? null : MapToDto(roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room type {RoomTypeId}", roomTypeId);
                return null;
            }
        }

        public async Task<List<RoomTypeDto>> GetRoomTypesByHotelAsync(int hotelId)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetByHotelIdAsync(hotelId);
                return roomTypes.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room types for hotel {HotelId}", hotelId);
                return new List<RoomTypeDto>();
            }
        }

        public async Task<List<RoomTypeDto>> GetActiveRoomTypesAsync(int hotelId)
        {
            try
            {
                var roomTypes = await _roomTypeRepository.GetAllActiveAsync(hotelId);
                return roomTypes.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active room types for hotel {HotelId}", hotelId);
                return new List<RoomTypeDto>();
            }
        }

        // ESTADO

        public async Task<bool> ActivateRoomTypeAsync(int roomTypeId, int updatedBy)
        {
            try
            {
                var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
                if (roomType == null)
                    return false;

                roomType.IsActive = true;
                roomType.UpdatedAt = DateTime.UtcNow;
                roomType.UpdatedBy = updatedBy;

                return await _roomTypeRepository.UpdateAsync(roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating room type {RoomTypeId}", roomTypeId);
                return false;
            }
        }

        public async Task<bool> DeactivateRoomTypeAsync(int roomTypeId, int updatedBy)
        {
            try
            {
                var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
                if (roomType == null)
                    return false;

                roomType.IsActive = false;
                roomType.UpdatedAt = DateTime.UtcNow;
                roomType.UpdatedBy = updatedBy;

                return await _roomTypeRepository.UpdateAsync(roomType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating room type {RoomTypeId}", roomTypeId);
                return false;
            }
        }

        // VALIDACIONES

        public async Task<bool> TypeNameExistsAsync(int hotelId, string typeName, int? excludeRoomTypeId = null)
        {
            try
            {
                return await _roomTypeRepository.TypeNameExistsAsync(hotelId, typeName, excludeRoomTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if type name exists {TypeName}", typeName);
                return false;
            }
        }

        // ESTADÍSTICAS

        public async Task<(int Total, int Available)> GetRoomCountsAsync(int roomTypeId)
        {
            try
            {
                var total = await _roomTypeRepository.GetTotalRoomsCountAsync(roomTypeId);
                var available = await _roomTypeRepository.GetAvailableRoomsCountAsync(roomTypeId);
                return (total, available);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room counts for type {RoomTypeId}", roomTypeId);
                return (0, 0);
            }
        }

        // HELPERS

        private static RoomTypeDto MapToDto(RoomType roomType)
        {
            return new RoomTypeDto
            {
                RoomTypeId = roomType.RoomTypeId,
                TypeName = roomType.TypeName,
                Description = roomType.Description,
                MaxOccupancy = roomType.MaxOccupancy,
                TotalRooms = roomType.TotalRooms,
                BasePrice = roomType.BasePrice,
                WeekendPrice = roomType.WeekendPrice,
                IsActive = roomType.IsActive
            };
        }
    }
}
