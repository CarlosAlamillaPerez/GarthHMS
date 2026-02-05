// GarthHMS.Application/Services/HourPackageService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarthHMS.Core.DTOs.HourPackage;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Core.Models;
using Microsoft.Extensions.Logging;
using RoomTypeEntity = GarthHMS.Core.Entities.RoomType;
using HourPackageEntity = GarthHMS.Core.Entities.HourPackage;

namespace GarthHMS.Application.Services
{
    public class HourPackageService : IHourPackageService
    {
        private readonly IHourPackageRepository _hourPackageRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly ILogger<HourPackageService> _logger;

        public HourPackageService(
            IHourPackageRepository hourPackageRepository,
            IRoomTypeRepository roomTypeRepository,
            ILogger<HourPackageService> logger)
        {
            _hourPackageRepository = hourPackageRepository;
            _roomTypeRepository = roomTypeRepository;
            _logger = logger;
        }

        // ====================================================================
        // CRUD BÁSICO
        // ====================================================================

        public async Task<ServiceResult<HourPackageResponseDto>> GetByIdAsync(Guid hourPackageId)
        {
            try
            {
                var hourPackage = await _hourPackageRepository.GetByIdAsync(hourPackageId);

                if (hourPackage == null)
                {
                    return ServiceResult<HourPackageResponseDto>.Failure("Paquete de horas no encontrado");
                }

                // Obtener información del tipo de habitación
                var roomType = await _roomTypeRepository.GetByIdAsync(hourPackage.RoomTypeId);

                var dto = MapToResponseDto(hourPackage, roomType);

                return ServiceResult<HourPackageResponseDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paquete de horas {HourPackageId}", hourPackageId);
                return ServiceResult<HourPackageResponseDto>.Failure("Error al obtener el paquete de horas");
            }
        }

        public async Task<ServiceResult<Guid>> CreateAsync(CreateHourPackageDto dto, Guid hotelId, Guid userId)
        {
            try
            {
                // Validar que el tipo de habitación exista y pertenezca al hotel
                var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
                if (roomType == null)
                {
                    return ServiceResult<Guid>.Failure("El tipo de habitación no existe");
                }

                if (roomType.HotelId != hotelId)
                {
                    return ServiceResult<Guid>.Failure("El tipo de habitación no pertenece a este hotel");
                }

                // Validar que no exista un paquete duplicado
                var exists = await _hourPackageRepository.ExistsAsync(hotelId, dto.RoomTypeId, dto.Hours);
                if (exists)
                {
                    return ServiceResult<Guid>.Failure($"Ya existe un paquete de {dto.Hours} horas para este tipo de habitación");
                }

                // Crear entidad
                var hourPackage = new HourPackageEntity
                {
                    HotelId = hotelId,
                    RoomTypeId = dto.RoomTypeId,
                    Name = dto.Name.Trim(),
                    Hours = dto.Hours,
                    Price = dto.Price,
                    ExtraHourPrice = dto.ExtraHourPrice,
                    AllowsExtensions = dto.AllowsExtensions,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = true,
                    CreatedBy = userId
                };

                var newId = await _hourPackageRepository.CreateAsync(hourPackage);

                _logger.LogInformation("Paquete de horas creado exitosamente: {HourPackageId} por usuario {UserId}", newId, userId);

                return ServiceResult<Guid>.Success(newId, "Paquete de horas creado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear paquete de horas para hotel {HotelId}", hotelId);
                return ServiceResult<Guid>.Failure("Error al crear el paquete de horas");
            }
        }

        public async Task<ServiceResult<bool>> UpdateAsync(UpdateHourPackageDto dto, Guid hotelId)
        {
            try
            {
                // Verificar que el paquete exista
                var hourPackage = await _hourPackageRepository.GetByIdAsync(dto.HourPackageId);
                if (hourPackage == null)
                {
                    return ServiceResult<bool>.Failure("Paquete de horas no encontrado");
                }

                // Verificar que pertenezca al hotel
                if (hourPackage.HotelId != hotelId)
                {
                    return ServiceResult<bool>.Failure("El paquete no pertenece a este hotel");
                }

                // Validar que no exista un paquete duplicado (excluyendo el actual)
                var exists = await _hourPackageRepository.ExistsAsync(
                    hotelId,
                    hourPackage.RoomTypeId,
                    dto.Hours,
                    dto.HourPackageId
                );

                if (exists)
                {
                    return ServiceResult<bool>.Failure($"Ya existe un paquete de {dto.Hours} horas para este tipo de habitación");
                }

                // Actualizar entidad
                hourPackage.Name = dto.Name.Trim();
                hourPackage.Hours = dto.Hours;
                hourPackage.Price = dto.Price;
                hourPackage.ExtraHourPrice = dto.ExtraHourPrice;
                hourPackage.AllowsExtensions = dto.AllowsExtensions;
                hourPackage.DisplayOrder = dto.DisplayOrder;

                await _hourPackageRepository.UpdateAsync(hourPackage);

                _logger.LogInformation("Paquete de horas actualizado: {HourPackageId}", dto.HourPackageId);

                return ServiceResult<bool>.Success(true, "Paquete de horas actualizado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar paquete de horas {HourPackageId}", dto.HourPackageId);
                return ServiceResult<bool>.Failure("Error al actualizar el paquete de horas");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(Guid hourPackageId, Guid hotelId)
        {
            try
            {
                // Verificar que el paquete exista
                var hourPackage = await _hourPackageRepository.GetByIdAsync(hourPackageId);
                if (hourPackage == null)
                {
                    return ServiceResult<bool>.Failure("Paquete de horas no encontrado");
                }

                // Verificar que pertenezca al hotel
                if (hourPackage.HotelId != hotelId)
                {
                    return ServiceResult<bool>.Failure("El paquete no pertenece a este hotel");
                }

                // TODO: Validar que no tenga reservaciones activas (implementar cuando exista módulo de reservas)

                await _hourPackageRepository.DeleteAsync(hourPackageId);

                _logger.LogInformation("Paquete de horas eliminado: {HourPackageId}", hourPackageId);

                return ServiceResult<bool>.Success(true, "Paquete de horas eliminado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar paquete de horas {HourPackageId}", hourPackageId);
                return ServiceResult<bool>.Failure("Error al eliminar el paquete de horas");
            }
        }

        // ====================================================================
        // CONSULTAS
        // ====================================================================

        public async Task<ServiceResult<IEnumerable<HourPackageResponseDto>>> GetAllByHotelAsync(Guid hotelId)
        {
            try
            {
                var hourPackages = await _hourPackageRepository.GetByHotelAsync(hotelId);
                var roomTypes = await _roomTypeRepository.GetByHotelAsync(hotelId);

                var dtos = hourPackages.Select(hp =>
                {
                    var roomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeId == hp.RoomTypeId);
                    return MapToResponseDto(hp, roomType);
                }).ToList();

                return ServiceResult<IEnumerable<HourPackageResponseDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paquetes de horas del hotel {HotelId}", hotelId);
                return ServiceResult<IEnumerable<HourPackageResponseDto>>.Failure("Error al obtener los paquetes de horas");
            }
        }

        public async Task<ServiceResult<IEnumerable<HourPackageResponseDto>>> GetActiveByHotelAsync(Guid hotelId)
        {
            try
            {
                var hourPackages = await _hourPackageRepository.GetAllActiveAsync(hotelId);
                var roomTypes = await _roomTypeRepository.GetByHotelAsync(hotelId);

                var dtos = hourPackages.Select(hp =>
                {
                    var roomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeId == hp.RoomTypeId);
                    return MapToResponseDto(hp, roomType);
                }).ToList();

                return ServiceResult<IEnumerable<HourPackageResponseDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paquetes activos del hotel {HotelId}", hotelId);
                return ServiceResult<IEnumerable<HourPackageResponseDto>>.Failure("Error al obtener los paquetes activos");
            }
        }

        public async Task<ServiceResult<IEnumerable<HourPackageResponseDto>>> GetByRoomTypeAsync(Guid roomTypeId)
        {
            try
            {
                var hourPackages = await _hourPackageRepository.GetByRoomTypeAsync(roomTypeId);
                var roomType = await _roomTypeRepository.GetByIdAsync(roomTypeId);

                var dtos = hourPackages.Select(hp => MapToResponseDto(hp, roomType)).ToList();

                return ServiceResult<IEnumerable<HourPackageResponseDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paquetes del tipo de habitación {RoomTypeId}", roomTypeId);
                return ServiceResult<IEnumerable<HourPackageResponseDto>>.Failure("Error al obtener los paquetes");
            }
        }

        // ====================================================================
        // VALIDACIONES
        // ====================================================================

        public async Task<bool> PackageExistsAsync(Guid hotelId, Guid roomTypeId, int hours, Guid? excludeHourPackageId = null)
        {
            return await _hourPackageRepository.ExistsAsync(hotelId, roomTypeId, hours, excludeHourPackageId);
        }

        // ====================================================================
        // MAPEO
        // ====================================================================

        private HourPackageResponseDto MapToResponseDto(HourPackageEntity hourPackage, RoomTypeEntity? roomType)
        {
            return new HourPackageResponseDto
            {
                HourPackageId = hourPackage.HourPackageId,
                HotelId = hourPackage.HotelId,
                RoomTypeId = hourPackage.RoomTypeId,
                Name = hourPackage.Name,
                Hours = hourPackage.Hours,
                Price = hourPackage.Price,
                ExtraHourPrice = hourPackage.ExtraHourPrice,
                AllowsExtensions = hourPackage.AllowsExtensions,
                DisplayOrder = hourPackage.DisplayOrder,
                IsActive = hourPackage.IsActive,
                CreatedAt = hourPackage.CreatedAt,
                UpdatedAt = hourPackage.UpdatedAt,
                CreatedBy = hourPackage.CreatedBy,
                RoomTypeName = roomType?.Name,
                RoomTypeCode = roomType?.Code
            };
        }
    }
}