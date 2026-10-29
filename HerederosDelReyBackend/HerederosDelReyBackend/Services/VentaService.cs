using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class VentaService: IVentaService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public VentaService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<VentaDto> AddAsync(VentaCreateDto dto)
        {
            var Objeto = _mapper.Map<Venta>(dto);

            await _unitOfWork.Ventas.AddAsync(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<VentaDto>(Objeto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var objeto = await _unitOfWork.Ventas.GetByIdAsync(id);
            if (objeto == null)
                return false;

            await _unitOfWork.Ventas.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }   

        public async Task<IEnumerable<VentaDto>> GetAllAsync()
        {
            var lista = await _unitOfWork.Ventas.GetAllAsync();
            return _mapper.Map<IEnumerable<VentaDto>>(lista);
        }

        public async Task<VentaDto?> GetByIdAsync(int id)
        {
            var objeto = await _unitOfWork.Ventas.GetByIdAsync(id);
            if (objeto == null)
                return null;

            return _mapper.Map<VentaDto>(objeto);
        }

        public async Task<bool> UpdateAsync(int id, VentaUpdateDto dto)
        {
            if (id == null)
                return false;

            var venta = await _unitOfWork.Ventas.GetByIdAsync(id);
            if (venta == null)
                return false;

            venta.Acredito = dto.Acredito;
            venta.MontoPagado = dto.MontoPagado;
            venta.SaldoPendiente = dto.SaldoPendiente;
            venta.FechaLimitePago = dto.FechaLimitePago;
            venta.Observacion = dto.Observacion;
            venta.EstadoVenta = dto.EstadoVenta;    

            _unitOfWork.Ventas.Update(venta);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<ApiResponse<IEnumerable<VentaDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var objeto = await _unitOfWork.Ventas.GetAllAsync(filter);

            var objetoDto = objeto.Select(v => new VentaDto
            {
                //Id = v.Id,
                //Fecha = v.Fecha,
                //Total = v.Total,
                //ClienteId = v.ClienteId,
                //UsuarioId = v.UsuarioId,

                //UsuarioNombre = v.Usuario != null
                //    ? v.Usuario.NombreUsuario
                //    : "Sin usuario",
                //ClienteNombre = v.Cliente != null
                //    ? v.Cliente.Nombre
                //    : "Sin usuario"


            });

            return new ApiResponse<IEnumerable<VentaDto>>(objetoDto, objeto.MetaData);
        }

        public async Task<bool> VentaDetalle(VentaDetalleDto dto, int idUsuario, int idSucursal)
        {
            if (dto?.Venta == null)
                throw new ArgumentException("Datos de venta inválidos");
            if (dto.Detalle == null || dto.Detalle.Count == 0)
                throw new ArgumentException("La venta debe incluir al menos un detalle");

            var venta = _mapper.Map<Venta>(dto.Venta);
            venta.IdUsuario = idUsuario;
            venta.IdSucursal = idSucursal;
            venta.FechaVenta = DateTime.Now;
            venta.EstadoVenta = string.IsNullOrWhiteSpace(dto.Venta.EstadoVenta)
                ? "COMPLETADA"
                : dto.Venta.EstadoVenta.Trim();
            venta.DetalleVenta.Clear();
            venta.Pagos.Clear();

            if (venta.IdCliente.HasValue && await _unitOfWork.Clientes.GetByIdAsync(venta.IdCliente.Value) == null)
                throw new ArgumentException("El cliente indicado no existe");

            var inventarios = new Dictionary<int, InventarioSucursal>();
            var fechaHoy = DateOnly.FromDateTime(DateTime.Today);

            foreach (var detalleDto in dto.Detalle)
            {
                if (detalleDto.IdProducto <= 0 || detalleDto.Cantidad <= 0)
                    throw new ArgumentException("Cada detalle debe indicar un producto válido y una cantidad mayor que cero");

                var producto = await _unitOfWork.Productos.GetByIdAsync(detalleDto.IdProducto);
                if (producto == null)
                    throw new ArgumentException($"No se encontró el producto con ID {detalleDto.IdProducto}");
                if (!producto.PermiteVenta)
                    throw new ArgumentException($"El producto {producto.Nombre} no está habilitado para la venta");

                if (producto.ControlaInventario)
                {
                    if (!inventarios.TryGetValue(producto.Id, out var inventario))
                    {
                        inventario = await _unitOfWork.InventarioSucursal.GetAllAsQueryable()
                            .FirstOrDefaultAsync(x => x.IdProducto == producto.Id && x.IdSucursal == idSucursal);
                        if (inventario == null)
                            throw new ArgumentException($"No hay inventario del producto {producto.Nombre} en esta sucursal");

                        inventarios[producto.Id] = inventario;
                    }

                    if (inventario.StockActual < detalleDto.Cantidad)
                        throw new ArgumentException($"Stock insuficiente para {producto.Nombre}");

                    inventario.StockActual -= detalleDto.Cantidad;
                    inventario.FechaModificacion = DateTime.Now;
                }

                Lote? lote = null;
                if (producto.ManejaLotes && !detalleDto.IdLote.HasValue)
                    throw new ArgumentException($"Debes indicar un lote para el producto {producto.Nombre}");

                if (detalleDto.IdLote.HasValue)
                {
                    lote = await _unitOfWork.Lote.GetByIdAsync(detalleDto.IdLote.Value);
                    if (lote == null || lote.IdProducto != producto.Id || lote.IdSucursal != idSucursal)
                        throw new ArgumentException("El lote indicado no corresponde al producto y sucursal de la venta");
                    if (lote.FechaVencimiento.HasValue && lote.FechaVencimiento.Value < fechaHoy)
                        throw new ArgumentException($"El lote {lote.NumeroLote} está vencido");
                    if (lote.CantidadDisponible < detalleDto.Cantidad)
                        throw new ArgumentException($"Cantidad insuficiente en el lote {lote.NumeroLote}");

                    lote.CantidadDisponible -= detalleDto.Cantidad;
                    lote.FechaModificacion = DateTime.Now;
                }

                venta.DetalleVenta.Add(new DetalleVenta
                {
                    Estado = false,
                    FechaRegistro = DateTime.Now,
                    IdProducto = producto.Id,
                    IdProductoNavigation = producto,
                    IdLote = lote?.Id,
                    IdLoteNavigation = lote,
                    Cantidad = detalleDto.Cantidad,
                    PrecioVenta = producto.PrecioVenta,
                    Descuento = 0,
                    Impuesto = 0,
                    SubTotal = decimal.Round(producto.PrecioVenta * detalleDto.Cantidad, 2, MidpointRounding.AwayFromZero)
                });
            }

            venta.Total = venta.DetalleVenta.Sum(x => x.SubTotal);
            var pagos = dto.Pagos ?? new List<PagoDto>();
            foreach (var pagoDto in pagos)
            {
                if (pagoDto.IdCaja <= 0 || pagoDto.Monto <= 0 || string.IsNullOrWhiteSpace(pagoDto.MetodoPago))
                    throw new ArgumentException("Cada pago debe incluir una caja, método y monto mayor que cero");

                var caja = await _unitOfWork.Caja.GetByIdAsync(pagoDto.IdCaja);
                if (caja == null || !string.Equals(caja.EstadoCaja, "ABIERTA", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"La caja {pagoDto.IdCaja} no existe o no está abierta");
                if (caja.IdSucursal != idSucursal)
                    throw new ArgumentException("La caja del pago pertenece a otra sucursal");

                venta.Pagos.Add(new Pago
                {
                    Estado = false,
                    FechaRegistro = DateTime.Now,
                    IdCaja = caja.Id,
                    MetodoPago = pagoDto.MetodoPago.Trim(),
                    Monto = pagoDto.Monto,
                    Referencia = pagoDto.Referencia,
                    Observacion = pagoDto.Observacion
                });
            }

            venta.MontoPagado = venta.Pagos.Sum(x => x.Monto);
            if (venta.MontoPagado > venta.Total)
                throw new ArgumentException("El total de pagos no puede superar el total de la venta");

            venta.SaldoPendiente = venta.Total - venta.MontoPagado;

            foreach (var inventario in inventarios.Values)
                _unitOfWork.InventarioSucursal.Update(inventario);

            await _unitOfWork.Ventas.AddAsync(venta);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
