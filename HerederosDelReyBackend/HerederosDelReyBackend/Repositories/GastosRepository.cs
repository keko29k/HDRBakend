using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.Repositories
{
    public class GastosRepository : GenericRepository<Gasto>, IGastosRepository
    {
        public GastosRepository(HerederosDelReyContext context) : base(context)
        {

        }
        public async Task<PagedList<Gasto>> GetAllAsync(PostQueryFilter filter)
        {
            var query = GetAllAsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Buscar))
            {
                var buscar = filter.Buscar.Trim().ToLower();

                query = query.Where(x =>
                    x.Concepto.ToLower().Contains(buscar) ||
                    (x.NumeroComprobante != null && x.NumeroComprobante.ToLower().Contains(buscar)) ||
                    (x.Observacion != null && x.Observacion.ToLower().Contains(buscar)) ||
                    x.FechaRegistro.ToString().Contains(buscar));
            }

            // Más recientes primero.
            query = query.OrderByDescending(x => x.FechaRegistro).ThenByDescending(x => x.Id);

            return await PagedList<Gasto>.CreateAsync(query, filter.PageNumber, filter.PageSize);
        }
    }
}
