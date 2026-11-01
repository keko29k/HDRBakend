using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.Interfaces
{
    public interface IProductoRepository : IGenericRepository<Producto>
    {
                        Task<PagedList<Producto>> GetAllAsync(PostQueryFilter filter);
        Task<PagedList<Producto>> GetBySucursalAsync(
          PostQueryFilter filter,
          int idSucursal
      );
    }
}
