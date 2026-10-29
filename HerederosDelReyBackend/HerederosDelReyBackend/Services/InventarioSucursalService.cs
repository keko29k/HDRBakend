using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class InventarioSucursalService : IInventarioSucursalService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;


        public InventarioSucursalService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<IEnumerable<InventarioSucursalDto>> GetAllAsync()
        {
            var lista = await _unitOfWork.InventarioSucursal.GetAllAsync();
            return _mapper.Map<IEnumerable<InventarioSucursalDto>>(lista);
        }


        public async Task<InventarioSucursalDto> AddAsync(InventarioSucursalCreateDto dto)
        {
            var Objeto = _mapper.Map<InventarioSucursal>(dto);

            await _unitOfWork.InventarioSucursal.AddAsync(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<InventarioSucursalDto>(Objeto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var objeto = await _unitOfWork.InventarioSucursal.GetByIdAsync(id);
            if (objeto == null)
                return false;

            await _unitOfWork.InventarioSucursal.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }



        public async Task<InventarioSucursalDto?> GetByIdAsync(int id)
        {
            var objeto = await _unitOfWork.InventarioSucursal.GetByIdAsync(id);
            if (objeto == null)
                return null;

            return _mapper.Map<InventarioSucursalDto>(objeto);
        }

        public async Task<bool> UpdateAsync(int id, InventarioSucursalUpdateDto dto)
        {
            if (id == null)
                return false;

            var Objeto = _unitOfWork.InventarioSucursal.GetByIdAsync(id).Result;
            if (Objeto == null)
                return false;

            Objeto.IdProducto = dto.IdProducto;
            Objeto.IdSucursal = dto.IdSucursal;
            Objeto.StockActual = dto.StockActual;
            Objeto.StockMinimo = dto.StockMinimo;
            Objeto.StockMaximo = dto.StockMaximo;
            Objeto.PuntoReorden = dto.PuntoReorden;
            Objeto.Ubicacion = dto.Ubicacion;


            _unitOfWork.InventarioSucursal.Update(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ApiResponse<IEnumerable<InventarioSucursalListadoDto>>> GetAllAsync(
            PostQueryFilter filter,
            int? idSucursal,
            string? estadoStock)
        {
            var estado = string.IsNullOrWhiteSpace(estadoStock) ? null : estadoStock.Trim().ToUpperInvariant();
            if (estado is not null && estado is not ("BAJO" or "SIN_EXISTENCIAS" or "NORMAL" or "SOBRESTOCK"))
                throw new ArgumentException("estadoStock debe ser BAJO, SIN_EXISTENCIAS, NORMAL o SOBRESTOCK");

            IQueryable<InventarioSucursal> query = _unitOfWork.InventarioSucursal.GetAllAsQueryable()
                .Include(x => x.IdProductoNavigation)
                .Include(x => x.IdSucursalNavigation);

            if (idSucursal.HasValue)
                query = query.Where(x => x.IdSucursal == idSucursal.Value);

            if (!string.IsNullOrWhiteSpace(filter.Buscar))
            {
                var buscar = filter.Buscar.Trim().ToLower();
                query = query.Where(x =>
                    x.IdProductoNavigation.Nombre.ToLower().Contains(buscar) ||
                    (x.IdProductoNavigation.NombreGenerico != null && x.IdProductoNavigation.NombreGenerico.ToLower().Contains(buscar)) ||
                    x.IdSucursalNavigation.Nombre.ToLower().Contains(buscar) ||
                    (x.Ubicacion != null && x.Ubicacion.ToLower().Contains(buscar)));
            }

            query = estado switch
            {
                "BAJO" => query.Where(x => x.StockActual > 0 && x.StockActual <= x.StockMinimo),
                "SIN_EXISTENCIAS" => query.Where(x => x.StockActual <= 0),
                "NORMAL" => query.Where(x => x.StockActual > x.StockMinimo &&
                    (!x.StockMaximo.HasValue || x.StockActual <= x.StockMaximo.Value)),
                "SOBRESTOCK" => query.Where(x => x.StockMaximo.HasValue && x.StockActual > x.StockMaximo.Value),
                _ => query
            };

            var inventarios = await PagedList<InventarioSucursal>.CreateAsync(
                query, filter.PageNumber, filter.PageSize);
            var datos = inventarios.Select(x => new InventarioSucursalListadoDto
            {
                Id = x.Id,
                IdSucursal = x.IdSucursal,
                IdProducto = x.IdProducto,
                NombreProducto = x.IdProductoNavigation.Nombre,
                NombreSucursal = x.IdSucursalNavigation.Nombre,
                Ubicacion = x.Ubicacion,
                StockActual = x.StockActual,
                StockMinimo = x.StockMinimo,
                StockMaximo = x.StockMaximo,
                EstadoStock = ObtenerEstadoStock(x)
            });

            return new ApiResponse<IEnumerable<InventarioSucursalListadoDto>>(datos, inventarios.MetaData);
        }

        public async Task<InventarioSucursalResumenDto> GetResumenAsync(int? idSucursal)
        {
            var diasPorVencer = int.TryParse(
                _configuration["INVENTARIO_ALERTAS_DIAS_POR_VENCER"], out var diasConfigurados) && diasConfigurados >= 0
                    ? diasConfigurados
                    : 30;
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var fechaLimite = hoy.AddDays(diasPorVencer);

            var inventarioQuery = _unitOfWork.InventarioSucursal.GetAllAsQueryable();
            if (idSucursal.HasValue)
                inventarioQuery = inventarioQuery.Where(x => x.IdSucursal == idSucursal.Value);

            var unidadesEnInventario = await inventarioQuery.SumAsync(x => (decimal?)x.StockActual) ?? 0;
            var inventarioBajoQuery = inventarioQuery.Where(x => x.StockActual > 0 && x.StockActual <= x.StockMinimo);
            var stockBajo = await inventarioBajoQuery.CountAsync();
            var sinExistencias = await inventarioQuery.CountAsync(x => x.StockActual <= 0);

            var productosStockBajo = await inventarioBajoQuery
                .Include(x => x.IdProductoNavigation)
                .Include(x => x.IdSucursalNavigation)
                .Select(x => new InventarioSucursalListadoDto
                {
                    Id = x.Id,
                    IdSucursal = x.IdSucursal,
                    IdProducto = x.IdProducto,
                    NombreProducto = x.IdProductoNavigation.Nombre,
                    NombreSucursal = x.IdSucursalNavigation.Nombre,
                    Ubicacion = x.Ubicacion,
                    StockActual = x.StockActual,
                    StockMinimo = x.StockMinimo,
                    StockMaximo = x.StockMaximo,
                    EstadoStock = "BAJO"
                })
                .ToListAsync();

            var lotesQuery = _unitOfWork.Lote.GetAllAsQueryable()
                .Where(x => x.CantidadDisponible > 0 &&
                    x.FechaVencimiento.HasValue &&
                    x.FechaVencimiento.Value >= hoy &&
                    x.FechaVencimiento.Value <= fechaLimite);
            if (idSucursal.HasValue)
                lotesQuery = lotesQuery.Where(x => x.IdSucursal == idSucursal.Value);

            var lotes = await lotesQuery
                .Include(x => x.IdProductoNavigation)
                .Include(x => x.IdSucursalNavigation)
                .ToListAsync();
            var detalleLotesPorVencer = lotes.Select(x =>
            {
                var fechaVencimiento = x.FechaVencimiento!.Value;
                return new LotePorVencerDto
                {
                    Id = x.Id,
                    IdProducto = x.IdProducto,
                    NombreProducto = x.IdProductoNavigation.Nombre,
                    IdSucursal = x.IdSucursal,
                    NombreSucursal = x.IdSucursalNavigation.Nombre,
                    NumeroLote = x.NumeroLote,
                    FechaVencimiento = fechaVencimiento,
                    CantidadDisponible = x.CantidadDisponible,
                    DiasParaVencer = fechaVencimiento.DayNumber - hoy.DayNumber
                };
            }).ToList();

            return new InventarioSucursalResumenDto
            {
                UnidadesEnInventario = unidadesEnInventario,
                StockBajo = stockBajo,
                SinExistencias = sinExistencias,
                LotesPorVencer = detalleLotesPorVencer.Count,
                ProductosStockBajo = productosStockBajo,
                DetalleLotesPorVencer = detalleLotesPorVencer
            };
        }

        private static string ObtenerEstadoStock(InventarioSucursal inventario)
        {
            if (inventario.StockActual <= 0)
                return "SIN_EXISTENCIAS";
            if (inventario.StockActual <= inventario.StockMinimo)
                return "BAJO";
            if (inventario.StockMaximo.HasValue && inventario.StockActual > inventario.StockMaximo.Value)
                return "SOBRESTOCK";
            return "NORMAL";
        }

    }
}
