using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Interfaces.Repositories
{
    public interface IRoomTypeRepository
    {
        Task<RoomType?> GetByIdAsync(Guid roomTypeId);
        Task<IEnumerable<RoomType>> GetByHotelAsync(Guid hotelId);
        Task<IEnumerable<RoomType>> GetAllActiveAsync(Guid hotelId);
        Task<RoomType?> GetByCodeAsync(Guid hotelId, string code);
        Task<Guid> CreateAsync(RoomType roomType);
        Task UpdateAsync(RoomType roomType);
        Task DeleteAsync(Guid roomTypeId);
        Task<bool> CodeExistsAsync(Guid hotelId, string code, Guid? excludeRoomTypeId = null);
        Task<bool> NameExistsAsync(Guid hotelId, string name, Guid? excludeRoomTypeId = null);
        Task UpdateDisplayOrderAsync(Guid roomTypeId, int newOrder);
        Task UpdatePricesAsync(Guid roomTypeId, decimal basePriceNightly, decimal basePriceHourly);
    }
}