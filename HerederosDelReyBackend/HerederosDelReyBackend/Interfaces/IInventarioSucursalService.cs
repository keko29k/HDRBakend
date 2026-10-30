using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;

namespace HerederosDelReyBackend.Interfaces
{
    public interface IInventarioSucursalService
    {
        Task<IEnumerable<InventarioSucursalDto>> GetAllAsync();
        Task<InventarioSucursalDto?> GetByIdAsync(int id);
        Task<InventarioSucursalDto> AddAsync(InventarioSucursalCreateDto dto);
        Task<bool> UpdateAsync(int id, InventarioSucursalUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<InventarioSucursalDto>>> GetAllAsync(PostQueryFilter filter);
    }
}
