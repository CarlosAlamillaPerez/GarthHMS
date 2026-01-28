using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GarthHMS.Core.DTOs;
using GarthHMS.Core.Entities;
using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GarthHMS.Application.Services
{
    /// <summary>
    /// Servicio para gestión de hoteles
    /// </summary>
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly ILogger<HotelService> _logger;

        public HotelService(
            IHotelRepository hotelRepository,
            ILogger<HotelService> logger)
        {
            _hotelRepository = hotelRepository;
            _logger = logger;
        }

        // CRUD

        public async Task<(bool Success, int HotelId, string? ErrorMessage)> CreateHotelAsync(
            string hotelName,
            string legalName,
            string? rfc,
            string email,
            string phone,
            bool isMotel,
            int createdBy)
        {
            try
            {
                // Validar RFC único si se proporciona
                if (!string.IsNullOrWhiteSpace(rfc))
                {
                    var rfcExists = await _hotelRepository.RFCExistsAsync(rfc);
                    if (rfcExists)
                    {
                        return (false, 0, "El RFC ya está registrado");
                    }
                }

                var hotel = new Hotel
                {
                    HotelName = hotelName,
                    LegalName = legalName,
                    RFC = rfc,
                    Email = email,
                    Phone = phone,
                    IsMotel = isMotel,
                    IsActive = true,
                    SubscriptionPlan = "Basic",
                    SubscriptionStartDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                var hotelId = await _hotelRepository.CreateAsync(hotel);

                if (hotelId > 0)
                {
                    _logger.LogInformation("Hotel created successfully: {HotelName} by user {CreatedBy}", hotelName, createdBy);
                    return (true, hotelId, null);
                }

                return (false, 0, "Error al crear el hotel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel {HotelName}", hotelName);
                return (false, 0, "Error al crear el hotel");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateHotelAsync(
            int hotelId,
            string hotelName,
            string? phone,
            string? email,
            string? city,
            string? state,
            int updatedBy)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(hotelId);
                if (hotel == null)
                {
                    return (false, "Hotel no encontrado");
                }

                hotel.HotelName = hotelName;
                hotel.Phone = phone;
                hotel.Email = email;
                hotel.City = city;
                hotel.State = state;
                hotel.UpdatedAt = DateTime.UtcNow;
                hotel.UpdatedBy = updatedBy;

                var result = await _hotelRepository.UpdateAsync(hotel);

                if (result)
                {
                    _logger.LogInformation("Hotel {HotelId} updated successfully by user {UpdatedBy}", hotelId, updatedBy);
                    return (true, null);
                }

                return (false, "Error al actualizar el hotel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel {HotelId}", hotelId);
                return (false, "Error al actualizar el hotel");
            }
        }

        // CONSULTAS

        public async Task<HotelDto?> GetHotelByIdAsync(int hotelId)
        {
            try
            {
                var hotel = await _hotelRepository.GetByIdAsync(hotelId);
                return hotel == null ? null : MapToDto(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel {HotelId}", hotelId);
                return null;
            }
        }

        public async Task<List<HotelDto>> GetAllHotelsAsync()
        {
            try
            {
                var hotels = await _hotelRepository.GetAllAsync();
                return hotels.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all hotels");
                return new List<HotelDto>();
            }
        }

        public async Task<List<HotelDto>> GetActiveHotelsAsync()
        {
            try
            {
                var hotels = await _hotelRepository.GetAllActiveAsync();
                return hotels.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active hotels");
                return new List<HotelDto>();
            }
        }

        // CONFIGURACIÓN

        public async Task<bool> UpdateHotelSettingsAsync(int hotelId, string settingsJson, int updatedBy)
        {
            try
            {
                var result = await _hotelRepository.UpdateSettingsAsync(hotelId, settingsJson);

                if (result)
                {
                    _logger.LogInformation("Hotel {HotelId} settings updated by user {UpdatedBy}", hotelId, updatedBy);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel settings {HotelId}", hotelId);
                return false;
            }
        }

        public async Task<string?> GetHotelSettingsAsync(int hotelId)
        {
            try
            {
                return await _hotelRepository.GetSettingsAsync(hotelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel settings {HotelId}", hotelId);
                return null;
            }
        }

        // VALIDACIONES

        public async Task<bool> RFCExistsAsync(string rfc, int? excludeHotelId = null)
        {
            try
            {
                return await _hotelRepository.RFCExistsAsync(rfc, excludeHotelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if RFC exists {RFC}", rfc);
                return false;
            }
        }

        // HELPERS

        private static HotelDto MapToDto(Hotel hotel)
        {
            return new HotelDto
            {
                HotelId = hotel.HotelId,
                HotelName = hotel.HotelName,
                Phone = hotel.Phone,
                Email = hotel.Email,
                City = hotel.City,
                State = hotel.State,
                IsMotel = hotel.IsMotel,
                IsActive = hotel.IsActive,
                SubscriptionPlan = hotel.SubscriptionPlan
            };
        }
    }
}
