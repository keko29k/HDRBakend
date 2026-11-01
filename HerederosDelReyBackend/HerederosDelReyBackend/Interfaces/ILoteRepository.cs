using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.Interfaces
{
    public interface ILoteRepository:IGenericRepository<Lote>
    {
        Task<PagedList<Lote>> GetAllAsync(PostQueryFilter filter);

        // Obtener lotes disponibles de un producto en una sucursal.
        Task<List<Lote>> ObtenerDisponiblesPorProductoSucursalAsync(
            int idProducto,
            int idSucursal,
            bool tieneVencimiento
        );


    }
}
