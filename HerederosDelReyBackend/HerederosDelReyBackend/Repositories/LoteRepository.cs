
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Repositories
{
    public class LoteRepository : GenericRepository<Lote>, ILoteRepository
    {
        public LoteRepository(HerederosDelReyContext context)
            : base(context)
        {
        }

        // =====================================================
        // LISTAR LOTES CON PAGINACIÓN
        // =====================================================

        public async Task<PagedList<Lote>> GetAllAsync(
            PostQueryFilter filter)
        {
            var query = GetAllAsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Buscar))
            {
                var buscar = filter.Buscar.ToLower();

                query = query.Where(x =>
                    x.Id.ToString().Contains(buscar)
                );
            }

            return await PagedList<Lote>.CreateAsync(
                query,
                filter.PageNumber,
                filter.PageSize
            );
        }

        // =====================================================
        // OBTENER LOTES DISPONIBLES PARA UNA VENTA
        // =====================================================

        public async Task<List<Lote>>
            ObtenerDisponiblesPorProductoSucursalAsync(
                int idProducto,
                int idSucursal,
                bool tieneVencimiento)
        {
            DateOnly fechaActual =
                DateOnly.FromDateTime(DateTime.Today);

            var query = _entities
                .AsTracking()
                .Where(l =>
                    l.Estado == false &&
                    l.IdProducto == idProducto &&
                    l.IdSucursal == idSucursal &&
                    l.CantidadDisponible > 0
                );

            // -------------------------------------------------
            // PRODUCTOS QUE MANEJAN FECHA DE VENCIMIENTO
            // FEFO: primero vence, primero sale.
            // -------------------------------------------------

            if (tieneVencimiento)
            {
                query = query.Where(l =>
                    l.FechaVencimiento.HasValue &&
                    l.FechaVencimiento.Value >= fechaActual
                );

                return await query
                    .OrderBy(l => l.FechaVencimiento)
                    .ThenBy(l => l.FechaRegistro)
                    .ThenBy(l => l.Id)
                    .ToListAsync();
            }

            // -------------------------------------------------
            // PRODUCTOS SIN FECHA DE VENCIMIENTO
            // FIFO: primero registrado, primero sale.
            // -------------------------------------------------

            return await query
                .OrderBy(l => l.FechaRegistro)
                .ThenBy(l => l.Id)
                .ToListAsync();
        }
    }
}
