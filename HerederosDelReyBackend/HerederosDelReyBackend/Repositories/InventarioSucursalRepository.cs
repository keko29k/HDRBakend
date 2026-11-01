using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Repositories
{
    public class InventarioSucursalRepository : GenericRepository<InventarioSucursal>, IInventarioSucursalRepository
    {
        public InventarioSucursalRepository(HerederosDelReyContext context) : base(context)
        {
        }
        public async Task<PagedList<InventarioSucursal>> GetAllAsync(PostQueryFilter filter)
        {
            var query = GetAllAsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Buscar))
            {
                var buscar = filter.Buscar.ToLower();

                query = query.Where(x =>
                    x.IdProductoNavigation.NombreGenerico.ToString().ToLower().Contains(buscar));


            }

            return await PagedList<InventarioSucursal>.CreateAsync(query, filter.PageNumber, filter.PageSize);
        }

        public async Task<InventarioSucursal?> GetBySucursalProductoAsync(
    int idSucursal,
    int idProducto)
        {
            return await _entities.FirstOrDefaultAsync(x =>
                x.IdSucursal == idSucursal &&
                x.IdProducto == idProducto &&
                x.Estado == false
            );
        }
    }
}
