using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO_CREATE;

namespace HerederosDelReyBackend.Interfaces
{
    public interface IVentaService
    {
        Task<IEnumerable<VentaDto>> GetAllAsync();
        Task<VentaDto?> GetByIdAsync(int id);
        Task<VentaDto> AddAsync(VentaCreateDto dto);
        Task<bool> UpdateAsync(int id, VentaUpdateDto dto);

        Task<int> VentaDetalle(VentaDetalleDto dto, int idUsuario, int idSucursal);


        Task<bool> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<VentaDto>>> GetAllAsync(PostQueryFilter filter);


        public Task<bool> RegistrarAbonoCreditoAsync(
            int idVenta,
            RegistrarAbonoCreditoDto dto,
            int idUsuario,
            int idSucursal);
    }
}
