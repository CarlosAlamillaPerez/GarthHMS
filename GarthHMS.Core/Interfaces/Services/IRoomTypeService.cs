using GarthHMS.Core.DTOs.RoomType;

namespace GarthHMS.Core.Interfaces.Services
{
    public interface IRoomTypeService
    {
        Task<IEnumerable<RoomTypeResponseDto>> GetAllAsync();
        Task<IEnumerable<RoomTypeResponseDto>> GetAllActiveAsync();
        Task<RoomTypeResponseDto?> GetByIdAsync(Guid roomTypeId);
        Task<Guid> CreateAsync(CreateRoomTypeDto dto);
        Task UpdateAsync(UpdateRoomTypeDto dto);
        Task DeleteAsync(Guid roomTypeId);
        Task<bool> CodeExistsAsync(string code, Guid? excludeRoomTypeId = null);
        Task<bool> NameExistsAsync(string name, Guid? excludeRoomTypeId = null);
        Task ReorderAsync(Dictionary<Guid, int> newOrders);
    }
}