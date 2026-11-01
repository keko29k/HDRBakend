
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;

namespace HerederosDelReyBackend.Interfaces
{
    public interface ICajaService
    {
        Task<IEnumerable<CajaDto>> GetAllAsync();

        Task<CajaDto?> GetByIdAsync(int id);

        Task<CajaDto> AddAsync(CajaCreateDto dto);

        Task<CajaDto> AbrirCajaAsync(
            CajaCreateDto dto,
            int idUsuario,
            int idSucursal
        );

        Task<CajaDto?> ObtenerMiCajaAbiertaAsync(
            int idUsuario,
            int idSucursal
        );

        Task<bool> UpdateAsync(int id, CajaUpdateDto dto);

        Task<bool> DeleteAsync(int id);

        Task<ApiResponse<IEnumerable<CajaDto>>> GetAllAsync(
            PostQueryFilter filter
        );

        Task<bool> CerrarCajaAsync(int id);
    }
}
