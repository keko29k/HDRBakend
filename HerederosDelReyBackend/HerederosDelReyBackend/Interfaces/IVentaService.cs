using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;

namespace HerederosDelReyBackend.Interfaces
{
    public interface IVentaService
    {
        Task<IEnumerable<VentaDto>> GetAllAsync();
        Task<VentaDto?> GetByIdAsync(int id);
        Task<VentaDto> AddAsync(VentaCreateDto dto);
        Task<bool> UpdateAsync(int id, VentaUpdateDto dto);

        Task<bool> VentaDetalle(VentaDetalleDto dto, int idUsuario, int idSucursal);


        Task<bool> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<VentaDto>>> GetAllAsync(PostQueryFilter filter);
    }
}
