using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;

namespace HerederosDelReyBackend.Interfaces
{
    public interface ICompraService
    {
        Task<IEnumerable<CompraDto>> GetAllAsync();
        Task<CompraDto?> GetByIdAsync(int id);
        Task<CompraDto> AddAsync(CompraCreateDto dto);
        Task<bool> UpdateAsync(int id, CompraUpdateDto dto);
        Task<bool> DeleteAsync(int id);

        Task<ApiResponse<IEnumerable<CompraDto>>> GetAllAsync(PostQueryFilter filter);
    }
}
