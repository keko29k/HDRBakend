using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class VentaService : IVentaService
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
                Id = v.Id,
                IdSucursal = v.IdSucursal,
                IdCliente = v.IdCliente,
                IdUsuario = v.IdUsuario,

                Acredito = v.Acredito,
                MontoPagado = v.MontoPagado,
                SaldoPendiente = v.SaldoPendiente,
                FechaLimitePago = v.FechaLimitePago,

                FechaVenta = v.FechaVenta,
                Total = v.Total,
                Observacion = v.Observacion,
                EstadoVenta = v.EstadoVenta,

                ClienteNombre = v.IdClienteNavigation == null
                    ? "Consumidor final"
                    : $"{v.IdClienteNavigation.Nombres} {v.IdClienteNavigation.Apellidos}".Trim(),

                UsuarioNombre = $"{v.IdUsuarioNavigation.Nombres} {v.IdUsuarioNavigation.Apellidos}".Trim(),

                MetodoPago = string.Join(", ",
                    v.Pagos.Select(p => p.MetodoPago).Distinct())
            });

            return new ApiResponse<IEnumerable<VentaDto>>(objetoDto, objeto.MetaData);
        }




        public async Task<int> VentaDetalle(
            VentaDetalleDto dto,
            int idUsuario,
            int idSucursal)
        {
            if (dto?.Venta == null)
                throw new ArgumentException("Datos de venta inválidos");

            if (dto.Detalle == null || dto.Detalle.Count == 0)
                throw new ArgumentException(
                    "La venta debe incluir al menos un detalle");

            // Iniciar transacción
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                // =====================================================
                // 1. CREAR CABECERA DE VENTA
                // =====================================================

                var venta = _mapper.Map<Venta>(dto.Venta);

                venta.IdUsuario = idUsuario;
                venta.IdSucursal = idSucursal;
                venta.FechaVenta = DateTime.Now;
                venta.EstadoVenta = "COMPLETADA";

                venta.DetalleVenta.Clear();
                venta.Pagos.Clear();

                if (venta.IdCliente.HasValue &&
                    await _unitOfWork.Clientes.GetByIdAsync(
                        venta.IdCliente.Value) == null)
                {
                    throw new ArgumentException(
                        "El cliente indicado no existe");
                }

                // =====================================================
                // 2. AGRUPAR PRODUCTOS REPETIDOS
                // =====================================================

                var productosAgrupados = dto.Detalle
                    .GroupBy(x => x.IdProducto)
                    .ToList();

                var inventarios = new Dictionary<int, InventarioSucursal>();

                foreach (var grupo in productosAgrupados)
                {
                    int idProducto = grupo.Key;

                    if (idProducto <= 0)
                        throw new ArgumentException(
                            "El producto indicado no es válido");

                    if (grupo.Any(x => x.Cantidad <= 0))
                        throw new ArgumentException(
                            "Las cantidades deben ser mayores que cero");

                    decimal cantidadTotal = grupo.Sum(x => x.Cantidad);

                    // =================================================
                    // 3. OBTENER PRODUCTO
                    // =================================================

                    var producto = await _unitOfWork.Productos
                        .GetByIdAsync(idProducto);

                    if (producto == null)
                        throw new ArgumentException(
                            $"No se encontró el producto con ID {idProducto}");

                    if (!producto.PermiteVenta)
                        throw new ArgumentException(
                            $"El producto {producto.Nombre} no está habilitado para la venta");

                    if (producto.ManejaLotes && !producto.ControlaInventario)
                        throw new ArgumentException(
                            $"El producto {producto.Nombre} maneja lotes, pero no controla inventario");

                    if (producto.TieneVencimiento && !producto.ManejaLotes)
                        throw new ArgumentException(
                            $"El producto {producto.Nombre} tiene vencimiento, pero no maneja lotes");

                    // =================================================
                    // 4. VALIDAR Y DESCONTAR INVENTARIO
                    // =================================================

                    if (producto.ControlaInventario)
                    {
                        var inventario = await _unitOfWork.InventarioSucursal
                            .GetAllAsQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.IdProducto == producto.Id &&
                                x.IdSucursal == idSucursal);

                        if (inventario == null)
                            throw new ArgumentException(
                                $"No existe inventario de {producto.Nombre} en esta sucursal");

                        if (inventario.StockActual < cantidadTotal)
                            throw new ArgumentException(
                                $"Stock insuficiente para {producto.Nombre}. " +
                                $"Disponible: {inventario.StockActual}, solicitado: {cantidadTotal}");

                        inventario.StockActual -= cantidadTotal;
                        inventario.FechaModificacion = DateTime.Now;

                        inventarios[producto.Id] = inventario;
                    }

                    // =================================================
                    // 5. PRODUCTOS QUE MANEJAN LOTES
                    // =================================================

                    if (producto.ManejaLotes)
                    {
                        var lotes = await _unitOfWork.Lote
                            .ObtenerDisponiblesPorProductoSucursalAsync(
                                producto.Id,
                                idSucursal,
                                producto.TieneVencimiento);

                        decimal stockDisponible = lotes.Sum(
                            x => x.CantidadDisponible);

                        if (stockDisponible < cantidadTotal)
                            throw new ArgumentException(
                                $"Lotes insuficientes para {producto.Nombre}. " +
                                $"Disponible: {stockDisponible}, solicitado: {cantidadTotal}");

                        decimal cantidadPendiente = cantidadTotal;

                        foreach (var lote in lotes)
                        {
                            if (cantidadPendiente <= 0)
                                break;

                            decimal cantidadTomada = Math.Min(
                                lote.CantidadDisponible,
                                cantidadPendiente);

                            lote.CantidadDisponible -= cantidadTomada;
                            lote.FechaModificacion = DateTime.Now;

                            venta.DetalleVenta.Add(new DetalleVenta
                            {
                                Estado = false,
                                FechaRegistro = DateTime.Now,

                                IdProducto = producto.Id,
                                IdProductoNavigation = producto,

                                IdLote = lote.Id,
                                IdLoteNavigation = lote,

                                Cantidad = cantidadTomada,
                                PrecioVenta = producto.PrecioVenta,
                                Descuento = 0,
                                Impuesto = 0,

                                SubTotal = decimal.Round(
                                    producto.PrecioVenta * cantidadTomada,
                                    2,
                                    MidpointRounding.AwayFromZero)
                            });

                            cantidadPendiente -= cantidadTomada;
                        }

                        if (cantidadPendiente > 0)
                            throw new ArgumentException(
                                $"No se pudo completar la asignación de lotes para {producto.Nombre}");
                    }
                    else
                    {
                        if (grupo.Any(x => x.IdLote.HasValue))
                            throw new ArgumentException(
                                $"El producto {producto.Nombre} no maneja lotes");

                        // =============================================
                        // 6. PRODUCTOS SIN MANEJO DE LOTES
                        // =============================================

                        venta.DetalleVenta.Add(new DetalleVenta
                        {
                            Estado = false,
                            FechaRegistro = DateTime.Now,

                            IdProducto = producto.Id,
                            IdProductoNavigation = producto,

                            IdLote = null,
                            IdLoteNavigation = null,

                            Cantidad = cantidadTotal,
                            PrecioVenta = producto.PrecioVenta,
                            Descuento = 0,
                            Impuesto = 0,

                            SubTotal = decimal.Round(
                                producto.PrecioVenta * cantidadTotal,
                                2,
                                MidpointRounding.AwayFromZero)
                        });
                    }
                }

                // =====================================================
                // 7. CALCULAR TOTAL DE VENTA
                // =====================================================

                venta.Total = venta.DetalleVenta.Sum(x => x.SubTotal);

                if (venta.Total <= 0)
                    throw new ArgumentException(
                        "El total de la venta debe ser mayor que cero");

                // =====================================================
                // 8. REGISTRAR PAGOS
                // =====================================================

                var pagos = dto.Pagos ?? new List<PagoDto>();

                foreach (var pagoDto in pagos)
                {
                    if (pagoDto.IdCaja <= 0 ||
                        pagoDto.Monto <= 0 ||
                        string.IsNullOrWhiteSpace(pagoDto.MetodoPago))
                    {
                        throw new ArgumentException(
                            "Cada pago debe incluir una caja, método y monto mayor que cero");
                    }

                    var caja = await _unitOfWork.Caja
                        .GetByIdAsync(pagoDto.IdCaja);

                    if (caja == null ||
                        !string.Equals(
                            caja.EstadoCaja,
                            "ABIERTA",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException(
                            $"La caja {pagoDto.IdCaja} no existe o no está abierta");
                    }

                    if (caja.IdSucursal != idSucursal)
                        throw new ArgumentException(
                            "La caja del pago pertenece a otra sucursal");

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

                // =====================================================
                // 9. CALCULAR MONTOS Y VALIDAR PAGOS
                // =====================================================

                venta.MontoPagado = venta.Pagos.Sum(x => x.Monto);

                if (venta.MontoPagado > venta.Total)
                    throw new ArgumentException(
                        "El total de pagos no puede superar el total de la venta");

                venta.SaldoPendiente = venta.Total - venta.MontoPagado;

                // Venta al contado
                if (!venta.Acredito)
                {
                    if (venta.SaldoPendiente > 0)
                        throw new ArgumentException(
                            "La venta al contado debe estar pagada completamente");

                    venta.FechaLimitePago = null;
                }

                // Venta a crédito con saldo pendiente
                if (venta.Acredito && venta.SaldoPendiente > 0)
                {
                    if (!venta.FechaLimitePago.HasValue)
                        throw new ArgumentException(
                            "Debe establecer una fecha límite para el pago del crédito");

                    if (venta.FechaLimitePago.Value <
                        DateOnly.FromDateTime(DateTime.Today))
                    {
                        throw new ArgumentException(
                            "La fecha límite de pago no puede ser anterior a hoy");
                    }
                }

                // Crédito completamente pagado
                if (venta.Acredito && venta.SaldoPendiente == 0)
                {
                    venta.FechaLimitePago = null;
                }

                // =====================================================
                // 10. GUARDAR CAMBIOS
                // =====================================================

                foreach (var inventario in inventarios.Values)
                {
                    _unitOfWork.InventarioSucursal.Update(inventario);
                }

                await _unitOfWork.Ventas.AddAsync(venta);

                await _unitOfWork.SaveChangesAsync();

                // Confirmar transacción
                await transaction.CommitAsync();

                return venta.Id;
            }
            catch
            {
                // Revertir cambios ante cualquier error
                await transaction.RollbackAsync();

                throw;
            }
        }



    



    public async Task<bool> RegistrarAbonoCreditoAsync(
    int idVenta,
    RegistrarAbonoCreditoDto dto,
    int idUsuario,
    int idSucursal)
        {
            if (dto.Monto <= 0)
                throw new ArgumentException("El monto del abono debe ser mayor a cero.");

            if (dto.MetodoPago != "EFECTIVO" && dto.MetodoPago != "QR")
                throw new ArgumentException("El método de pago debe ser EFECTIVO o QR.");

            await using var transaction = await _unitOfWork.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            try
            {
                // 1. Buscar la venta
                var venta = await _unitOfWork.Ventas.GetByIdAsync(idVenta);

                if (venta == null)
                    throw new ArgumentException("La venta no existe.");

                // 2. Verificar que pertenezca a la sucursal del usuario
                if (venta.IdSucursal != idSucursal)
                    throw new ArgumentException("No tienes permiso para modificar esta venta.");

                // 3. Verificar que sea una venta a crédito
                if (!venta.Acredito)
                    throw new ArgumentException("Esta venta no es a crédito.");

                // 4. Verificar que todavía tenga saldo pendiente
                if (venta.SaldoPendiente <= 0)
                    throw new ArgumentException("Esta venta ya está completamente pagada.");

                // 5. Validar que el abono no supere el saldo pendiente
                if (dto.Monto > venta.SaldoPendiente)
                    throw new ArgumentException("El abono no puede superar el saldo pendiente.");

                // 6. Buscar la caja abierta del usuario en su sucursal
                var caja = await _unitOfWork.Caja.GetAllAsQueryable()
                    .FirstOrDefaultAsync(c =>
                        c.IdUsuarioApertura == idUsuario &&
                        c.IdSucursal == idSucursal &&
                        c.EstadoCaja == "ABIERTA");

                if (caja == null)
                    throw new ArgumentException("Debes tener una caja abierta para registrar el abono.");

                // 7. Registrar el pago
                var pago = new Pago
                {
                    IdVenta = venta.Id,
                    IdCaja = caja.Id,
                    MetodoPago = dto.MetodoPago,
                    Monto = dto.Monto,
                    Referencia = dto.Referencia,
                    Observacion = dto.Observacion,
                    Estado = false,
                    FechaRegistro = DateTime.Now
                };

                venta.Pagos.Add(pago);

                // 8. Actualizar los montos de la venta
                venta.MontoPagado += dto.Monto;
                venta.SaldoPendiente -= dto.Monto;

                // 9. Si ya se canceló toda la deuda
                if (venta.SaldoPendiente == 0)
                {
                    venta.SaldoPendiente = 0;
                    venta.FechaLimitePago = null;
                }

                // 10. Guardar cambios
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }


}
