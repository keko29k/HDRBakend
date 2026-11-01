using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.Interfaces
{
    public interface IInventarioSucursalRepository: IGenericRepository<InventarioSucursal>
    {
        Task<PagedList<InventarioSucursal>> GetAllAsync(PostQueryFilter filter);

        Task<InventarioSucursal?> GetBySucursalProductoAsync(
            int idSucursal,
            int idProducto
        );
    }
}
