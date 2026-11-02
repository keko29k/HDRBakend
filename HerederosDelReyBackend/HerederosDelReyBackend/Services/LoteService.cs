using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class LoteService : ILoteService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        // =====================================================
        // RETIRAR UNIDADES DE UN LOTE
        // Descuenta del lote y del stock de la sucursal en una sola
        // transacción. No guarda historial de la baja.
        // =====================================================

        public async Task RetirarAsync(int idLote, decimal cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad a retirar debe ser mayor que cero.");

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var lote = await _unitOfWork.Lote.GetByIdAsync(idLote);

                if (lote == null)
                    throw new KeyNotFoundException("El lote no existe.");

                if (cantidad > lote.CantidadDisponible)
                    throw new ArgumentException(
                        $"No puedes retirar más de lo disponible en el lote ({lote.CantidadDisponible}).");

                lote.CantidadDisponible -= cantidad;
                lote.FechaModificacion = DateTime.Now;

                var inventario = await _unitOfWork.InventarioSucursal
                    .GetAllAsQueryable()
                    .FirstOrDefaultAsync(i =>
                        i.IdProducto == lote.IdProducto &&
                        i.IdSucursal == lote.IdSucursal);

                if (inventario != null)
                {
                    inventario.StockActual = Math.Max(inventario.StockActual - cantidad, 0);
                    inventario.FechaModificacion = DateTime.Now;
                    _unitOfWork.InventarioSucursal.Update(inventario);
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =====================================================
        // VENCIMIENTOS (solo lectura)
        // Lotes con stock que ya vencieron o vencen dentro de "dias".
        // =====================================================

        public async Task<List<LoteVencimientoDto>> GetVencimientosAsync(int? idSucursal, int dias)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var limite = hoy.AddDays(dias);

            var lotes = await _unitOfWork.Lote
                .GetAllAsQueryable()
                .Where(l =>
                    !l.IdProductoNavigation.Estado &&
                    l.CantidadDisponible > 0 &&
                    l.FechaVencimiento.HasValue &&
                    l.FechaVencimiento.Value <= limite &&
                    (!idSucursal.HasValue || l.IdSucursal == idSucursal.Value))
                .Select(l => new
                {
                    l.Id,
                    l.NumeroLote,
                    l.IdProducto,
                    l.IdProductoNavigation.Codigo,
                    Producto = l.IdProductoNavigation.Nombre,
                    l.IdSucursal,
                    Sucursal = l.IdSucursalNavigation.Nombre,
                    l.FechaFabricacion,
                    FechaVencimiento = l.FechaVencimiento!.Value,
                    l.CantidadInicial,
                    l.CantidadDisponible,
                    l.CostoCompra,
                    l.IdProductoNavigation.PrecioVenta
                })
                .ToListAsync();

            return lotes
                .Select(l => new LoteVencimientoDto
                {
                    IdLote = l.Id,
                    NumeroLote = l.NumeroLote,
                    IdProducto = l.IdProducto,
                    Codigo = l.Codigo,
                    Producto = l.Producto,
                    IdSucursal = l.IdSucursal,
                    Sucursal = l.Sucursal,
                    FechaFabricacion = l.FechaFabricacion,
                    FechaVencimiento = l.FechaVencimiento,
                    DiasRestantes = l.FechaVencimiento.DayNumber - hoy.DayNumber,
                    CantidadInicial = l.CantidadInicial,
                    CantidadDisponible = l.CantidadDisponible,
                    CostoCompra = l.CostoCompra,
                    PrecioVenta = l.PrecioVenta,
                    ValorCosto = decimal.Round(l.CantidadDisponible * l.CostoCompra, 2, MidpointRounding.AwayFromZero),
                    ValorVenta = decimal.Round(l.CantidadDisponible * l.PrecioVenta, 2, MidpointRounding.AwayFromZero)
                })
                .OrderBy(l => l.FechaVencimiento)
                .ThenBy(l => l.Producto)
                .ToList();
        }


        public async Task<IEnumerable<LoteDto>> GetAllAsync()
        {
            var lista = await _unitOfWork.Lote.GetAllAsync();
            return _mapper.Map<IEnumerable<LoteDto>>(lista);
        }

        public LoteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<LoteDto> AddAsync(LoteCreateDto dto)
        {
            var Objeto = _mapper.Map<Lote>(dto);

            await _unitOfWork.Lote.AddAsync(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<LoteDto>(Objeto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var objeto = await _unitOfWork.Lote.GetByIdAsync(id);
            if (objeto == null)
                return false;

            await _unitOfWork.Lote.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }



        public async Task<LoteDto?> GetByIdAsync(int id)
        {
            var objeto = await _unitOfWork.Lote.GetByIdAsync(id);
            if (objeto == null)
                return null;

            return _mapper.Map<LoteDto>(objeto);
        }

        public async Task<bool> UpdateAsync(int id, LoteUpdateDto dto)
        {
            if (id == null)
                return false;

            var Objeto = _unitOfWork.Lote.GetByIdAsync(id).Result;
            if (Objeto == null)
                return false;

            Objeto.NumeroLote = dto.NumeroLote;
            Objeto.IdProducto = dto.IdProducto;
            Objeto.FechaFabricacion = dto.FechaFabricacion;
            Objeto.FechaVencimiento = dto.FechaVencimiento; 
            Objeto.CantidadInicial = dto.CantidadInicial;
            Objeto.CantidadDisponible = dto.CantidadDisponible;

         


            _unitOfWork.Lote.Update(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ApiResponse<IEnumerable<LoteDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var lotes = await _unitOfWork.Lote.GetAllAsync(filter);
            var lotesDto = _mapper.Map<IEnumerable<LoteDto>>(lotes);

            return new ApiResponse<IEnumerable<LoteDto>>(lotesDto, lotes.MetaData);
        }
    }
}
