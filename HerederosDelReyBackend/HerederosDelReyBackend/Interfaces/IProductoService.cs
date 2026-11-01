using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;

namespace HerederosDelReyBackend.Interfaces
{
    public interface IProductoService 
    {
        Task<IEnumerable<ProductoDto>> GetAllAsync();
        Task<ProductoDto?> GetByIdAsync(int id);
        Task<ProductoDto> AddAsync(ProductoCreateDto dto);
        Task<bool> UpdateAsync(int id, ProductoUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<ProductoDto>>> GetAllAsync(PostQueryFilter filter);
        Task<ApiResponse<IEnumerable<ProductoDto>>> GetBySucursalAsync(
    PostQueryFilter filter,
    int idSucursal
);
    }
}
