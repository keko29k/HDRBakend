using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;

namespace HerederosDelReyBackend.Interfaces
{
    public interface ILoteService
    {
        Task<IEnumerable<LoteDto>> GetAllAsync();
        Task<LoteDto> AddAsync(LoteCreateDto dto);

        Task<bool> DeleteAsync(int id);

        Task<LoteDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, LoteUpdateDto dto);
        Task<ApiResponse<IEnumerable<LoteDto>>> GetAllAsync(PostQueryFilter filter);

        Task<List<LoteVencimientoDto>> GetVencimientosAsync(int? idSucursal, int dias);

        Task RetirarAsync(int idLote, decimal cantidad);

    }
}
