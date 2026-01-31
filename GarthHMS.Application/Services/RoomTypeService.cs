using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.RoomType;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio de tipos de habitación con lógica de negocio y validaciones
    /// </summary>
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly ILogger<RoomTypeService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoomTypeService(
            IRoomTypeRepository roomTypeRepository,
            ILogger<RoomTypeService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _roomTypeRepository = roomTypeRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid CurrentHotelId => GetCurrentHotelId();
        private Guid CurrentUserId => GetCurrentUserId();

        public async Task<IEnumerable<RoomTypeResponseDto>> GetAllAsync()
        {
            var roomTypes = await _roomTypeRepository.GetByHotelAsync(CurrentHotelId);
            return roomTypes.Select(rt => MapToResponseDto(rt));
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetAllActiveAsync()
        {
            var roomTypes = await _roomTypeRepository.GetAllActiveAsync(CurrentHotelId);
            return roomTypes.Select(rt => MapToResponseDto(rt));
        }

        public async Task<RoomTypeResponseDto?> GetByIdAsync(Guid roomTypeId)
        {
            var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);

            if (roomType == null)
                return null;

            // Validar multi-tenancy
            if (roomType.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para acceder a este tipo de habitación");

            return MapToResponseDto(roomType);
        }

        public async Task<Guid> CreateAsync(CreateRoomTypeDto dto)
        {
            // 1. Validar que el código no exista
            if (await _roomTypeRepository.CodeExistsAsync(CurrentHotelId, dto.Code))
                throw new InvalidOperationException($"Ya existe un tipo de habitación con el código '{dto.Code}'");

            // 2. Validar que el nombre no exista
            if (await _roomTypeRepository.NameExistsAsync(CurrentHotelId, dto.Name))
                throw new InvalidOperationException($"Ya existe un tipo de habitación con el nombre '{dto.Name}'");

            // 3. Validar capacidades
            if (dto.MaxCapacity < dto.BaseCapacity)
                throw new InvalidOperationException("La capacidad máxima debe ser mayor o igual a la capacidad base");

            // 4. Validar precios (por ahora validación básica, después agregaremos validación según operation_mode)
            if (dto.BasePriceNightly <= 0 && dto.BasePriceHourly <= 0)
                throw new InvalidOperationException("Debe especificar al menos un precio (por noche o por hora)");

            // 5. Validar cargo de mascota
            if (dto.AllowsPets && dto.PetCharge < 0)
                throw new InvalidOperationException("El cargo por mascota no puede ser negativo");

            // 6. Crear el modelo
            var roomType = new RoomType
            {
                HotelId = CurrentHotelId,
                Name = dto.Name,
                Code = dto.Code.ToUpper(),
                Description = dto.Description,
                BaseCapacity = dto.BaseCapacity,
                MaxCapacity = dto.MaxCapacity,
                BasePriceNightly = dto.BasePriceNightly,
                BasePriceHourly = dto.BasePriceHourly,
                ExtraPersonCharge = dto.ExtraPersonCharge,
                AllowsPets = dto.AllowsPets,
                PetCharge = dto.PetCharge,
                SizeSqm = dto.SizeSqm,
                BedType = dto.BedType,
                ViewType = dto.ViewType,
                DisplayOrder = dto.DisplayOrder,
                CreatedBy = CurrentUserId
            };

            // Asignar amenidades y fotos
            roomType.Amenities = dto.Amenities ?? new List<string>();
            roomType.PhotoUrls = dto.PhotoUrls ?? new List<string>();

            // 7. Guardar
            var roomTypeId = await _roomTypeRepository.CreateAsync(roomType);

            _logger.LogInformation("Tipo de habitación creado: {Name} ({Code}) - ID: {RoomTypeId}",
                dto.Name, dto.Code, roomTypeId);

            return roomTypeId;
        }

        public async Task UpdateAsync(UpdateRoomTypeDto dto)
        {
            // 1. Validar que exista
            var existingRoomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
            if (existingRoomType == null)
                throw new KeyNotFoundException($"No se encontró el tipo de habitación con ID {dto.RoomTypeId}");

            // 2. Validar multi-tenancy
            if (existingRoomType.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para modificar este tipo de habitación");

            // 3. Validar que el código no exista (excluyendo el actual)
            if (await _roomTypeRepository.CodeExistsAsync(CurrentHotelId, dto.Code, dto.RoomTypeId))
                throw new InvalidOperationException($"Ya existe otro tipo de habitación con el código '{dto.Code}'");

            // 4. Validar que el nombre no exista (excluyendo el actual)
            if (await _roomTypeRepository.NameExistsAsync(CurrentHotelId, dto.Name, dto.RoomTypeId))
                throw new InvalidOperationException($"Ya existe otro tipo de habitación con el nombre '{dto.Name}'");

            // 5. Validar capacidades
            if (dto.MaxCapacity < dto.BaseCapacity)
                throw new InvalidOperationException("La capacidad máxima debe ser mayor o igual a la capacidad base");

            // 6. Validar precios
            if (dto.BasePriceNightly <= 0 && dto.BasePriceHourly <= 0)
                throw new InvalidOperationException("Debe especificar al menos un precio (por noche o por hora)");

            // 7. Actualizar el modelo
            existingRoomType.Name = dto.Name;
            existingRoomType.Code = dto.Code.ToUpper();
            existingRoomType.Description = dto.Description;
            existingRoomType.BaseCapacity = dto.BaseCapacity;
            existingRoomType.MaxCapacity = dto.MaxCapacity;
            existingRoomType.BasePriceNightly = dto.BasePriceNightly;
            existingRoomType.BasePriceHourly = dto.BasePriceHourly;
            existingRoomType.ExtraPersonCharge = dto.ExtraPersonCharge;
            existingRoomType.AllowsPets = dto.AllowsPets;
            existingRoomType.PetCharge = dto.PetCharge;
            existingRoomType.SizeSqm = dto.SizeSqm;
            existingRoomType.BedType = dto.BedType;
            existingRoomType.ViewType = dto.ViewType;
            existingRoomType.DisplayOrder = dto.DisplayOrder;
            existingRoomType.Amenities = dto.Amenities ?? new List<string>();
            existingRoomType.PhotoUrls = dto.PhotoUrls ?? new List<string>();
            existingRoomType.UpdatedAt = DateTime.UtcNow;

            // 8. Guardar
            await _roomTypeRepository.UpdateAsync(existingRoomType);

            _logger.LogInformation("Tipo de habitación actualizado: {Name} ({Code}) - ID: {RoomTypeId}",
                dto.Name, dto.Code, dto.RoomTypeId);
        }

        public async Task DeleteAsync(Guid roomTypeId)
        {
            // 1. Validar que exista
            var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);
            if (roomType == null)
                throw new KeyNotFoundException($"No se encontró el tipo de habitación con ID {roomTypeId}");

            // 2. Validar multi-tenancy
            if (roomType.HotelId != CurrentHotelId)
                throw new UnauthorizedAccessException("No tiene permiso para eliminar este tipo de habitación");

            // 3. TODO: Validar que no tenga habitaciones ocupadas
            // Esta validación la implementaremos cuando tengamos el módulo de habitaciones

            // 4. Eliminar (soft delete)
            await _roomTypeRepository.DeleteAsync(roomTypeId);

            _logger.LogInformation("Tipo de habitación eliminado: {RoomTypeId}", roomTypeId);
        }

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeRoomTypeId = null)
        {
            return await _roomTypeRepository.CodeExistsAsync(CurrentHotelId, code, excludeRoomTypeId);
        }

        public async Task<bool> NameExistsAsync(string name, Guid? excludeRoomTypeId = null)
        {
            return await _roomTypeRepository.NameExistsAsync(CurrentHotelId, name, excludeRoomTypeId);
        }

        public async Task ReorderAsync(Dictionary<Guid, int> newOrders)
        {
            foreach (var order in newOrders)
            {
                await _roomTypeRepository.UpdateDisplayOrderAsync(order.Key, order.Value);
            }

            _logger.LogInformation("Tipos de habitación reordenados: {Count} elementos", newOrders.Count);
        }

        #region Helper Methods

        private RoomTypeResponseDto MapToResponseDto(RoomType roomType)
        {
            return new RoomTypeResponseDto
            {
                RoomTypeId = roomType.RoomTypeId,
                HotelId = roomType.HotelId,
                Name = roomType.Name,
                Code = roomType.Code,
                Description = roomType.Description,
                BaseCapacity = roomType.BaseCapacity,
                MaxCapacity = roomType.MaxCapacity,
                BasePriceNightly = roomType.BasePriceNightly,
                BasePriceHourly = roomType.BasePriceHourly,
                ExtraPersonCharge = roomType.ExtraPersonCharge,
                AllowsPets = roomType.AllowsPets,
                PetCharge = roomType.PetCharge,
                SizeSqm = roomType.SizeSqm,
                BedType = roomType.BedType,
                ViewType = roomType.ViewType,
                Amenities = roomType.Amenities,
                PhotoUrls = roomType.PhotoUrls,
                DisplayOrder = roomType.DisplayOrder,
                IsActive = roomType.IsActive,
                CreatedAt = roomType.CreatedAt,
                UpdatedAt = roomType.UpdatedAt
            };
        }

        private Guid GetCurrentHotelId()
        {
            var hotelIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("HotelId")?.Value;
            if (string.IsNullOrEmpty(hotelIdClaim) || !Guid.TryParse(hotelIdClaim, out var hotelId))
                throw new UnauthorizedAccessException("No se pudo obtener el HotelId del usuario actual");

            return hotelId;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("No se pudo obtener el UserId del usuario actual");

            return userId;
        }

        #endregion
    }
}