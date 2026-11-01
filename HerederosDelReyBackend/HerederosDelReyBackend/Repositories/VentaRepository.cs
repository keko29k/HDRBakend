using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Repositories
{
    public class VentaRepository : GenericRepository<Venta>,
        IVentaRepository
    {
        public VentaRepository(HerederosDelReyContext context) : base(context)
        {
        }
        public async Task<PagedList<Venta>> GetAllAsync(PostQueryFilter filter)
        {
            var query = GetAllAsQueryable()
                      .Include(v => v.IdClienteNavigation)
                      .Include(v => v.IdUsuarioNavigation)
                      .Include(v => v.Pagos.Where(p => !p.Estado))
                      .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Buscar))
            {
                var buscar = filter.Buscar.ToLower();

                query = query.Where(x =>
                    x.IdClienteNavigation.Nombres.ToLower().Contains(buscar) ||
                    x.IdUsuarioNavigation.Usuario1.ToLower().Contains(buscar));


            }

            return await PagedList<Venta>.CreateAsync(query, filter.PageNumber, filter.PageSize);
        }
    }
}
