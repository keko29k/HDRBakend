using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class ReporteService : IReporteService
    {
        private const string VentaAnulada = "ANULADA";
        private const string CompraAnulada = "ANULADA";
        private const int DiasAlertaVencimiento = 30;

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HerederosDelReyContext _context;

        public ReporteService(IUnitOfWork unitOfWork, IMapper mapper, HerederosDelReyContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        // =====================================================
        // REPORTE GENERAL
        // Solo lectura: no modifica ningún dato.
        // =====================================================

        public async Task<ReporteGeneralDto> GetReporteGeneral(DateOnly desde, DateOnly hasta, int? idSucursal)
        {
            // Rango [desde 00:00, hasta + 1 día 00:00) para incluir todo el último día.
            var inicio = desde.ToDateTime(TimeOnly.MinValue);
            var fin = hasta.AddDays(1).ToDateTime(TimeOnly.MinValue);
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var reporte = new ReporteGeneralDto
            {
                Desde = desde,
                Hasta = hasta,
                IdSucursal = idSucursal
            };

            if (idSucursal.HasValue)
            {
                reporte.Sucursal = await _context.Sucursales
                    .AsNoTracking()
                    .Where(s => s.Id == idSucursal.Value)
                    .Select(s => s.Nombre)
                    .FirstOrDefaultAsync() ?? $"Sucursal {idSucursal}";
            }

            // ---------------- VENTAS ----------------

            var ventas = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    !v.Estado &&
                    v.EstadoVenta != VentaAnulada &&
                    v.FechaVenta >= inicio &&
                    v.FechaVenta < fin &&
                    (!idSucursal.HasValue || v.IdSucursal == idSucursal.Value))
                .Select(v => new
                {
                    v.Id,
                    v.FechaVenta,
                    v.Total,
                    v.Acredito,
                    v.IdUsuario,
                    Vendedor = v.IdUsuarioNavigation.Nombres + " " + (v.IdUsuarioNavigation.Apellidos ?? "")
                })
                .ToListAsync();

            // Costo: el del lote cuando existe; si no, el precio de compra del producto.
            var detalles = await _context.DetalleVentas
                .AsNoTracking()
                .Where(d =>
                    !d.Estado &&
                    !d.IdVentaNavigation.Estado &&
                    d.IdVentaNavigation.EstadoVenta != VentaAnulada &&
                    d.IdVentaNavigation.FechaVenta >= inicio &&
                    d.IdVentaNavigation.FechaVenta < fin &&
                    (!idSucursal.HasValue || d.IdVentaNavigation.IdSucursal == idSucursal.Value))
                .Select(d => new
                {
                    d.IdProducto,
                    d.IdProductoNavigation.Codigo,
                    d.IdProductoNavigation.Nombre,
                    d.Cantidad,
                    d.SubTotal,
                    CostoUnitario = d.IdLote != null
                        ? d.IdLoteNavigation!.CostoCompra
                        : d.IdProductoNavigation.PrecioCompra,
                    Fecha = d.IdVentaNavigation.FechaVenta
                })
                .ToListAsync();

            // ---------------- COBROS (PAGOS) ----------------

            var pagos = await _context.Pagos
                .AsNoTracking()
                .Where(p =>
                    !p.Estado &&
                    !p.IdVentaNavigation.Estado &&
                    p.IdVentaNavigation.EstadoVenta != VentaAnulada &&
                    p.FechaRegistro >= inicio &&
                    p.FechaRegistro < fin &&
                    (!idSucursal.HasValue || p.IdCajaNavigation.IdSucursal == idSucursal.Value))
                .Select(p => new { p.MetodoPago, p.Monto })
                .ToListAsync();

            // ---------------- COMPRAS ----------------

            var compras = await _context.Compras
                .AsNoTracking()
                .Where(c =>
                    !c.Estado &&
                    c.EstadoCompra != CompraAnulada &&
                    c.FechaCompra >= inicio &&
                    c.FechaCompra < fin &&
                    (!idSucursal.HasValue || c.IdSucursal == idSucursal.Value))
                .Select(c => new
                {
                    c.IdProveedor,
                    Proveedor = c.IdProveedorNavigation.Nombre,
                    c.Total
                })
                .ToListAsync();

            // ---------------- GASTOS ----------------

            var gastos = await _context.Gastos
                .AsNoTracking()
                .Where(g =>
                    !g.Estado &&
                    g.FechaRegistro >= inicio &&
                    g.FechaRegistro < fin &&
                    (!idSucursal.HasValue || g.IdCajaNavigation.IdSucursal == idSucursal.Value))
                .Select(g => new { g.Concepto, g.Monto })
                .ToListAsync();

            // ---------------- RESUMEN ----------------

            var totalVentas = ventas.Sum(v => v.Total);
            var costoVentas = Redondear(detalles.Sum(d => d.CostoUnitario * d.Cantidad));
            var totalGastos = gastos.Sum(g => g.Monto);
            var utilidadBruta = totalVentas - costoVentas;
            var utilidadNeta = utilidadBruta - totalGastos;

            reporte.Resumen = new ReporteResumenDto
            {
                TotalVentas = totalVentas,
                CantidadVentas = ventas.Count,
                TicketPromedio = ventas.Count > 0 ? Redondear(totalVentas / ventas.Count) : 0,
                UnidadesVendidas = detalles.Sum(d => d.Cantidad),

                CostoVentas = costoVentas,
                UtilidadBruta = utilidadBruta,
                MargenBruto = Porcentaje(utilidadBruta, totalVentas),

                TotalGastos = totalGastos,
                UtilidadNeta = utilidadNeta,
                MargenNeto = Porcentaje(utilidadNeta, totalVentas),

                TotalCobrado = pagos.Sum(p => p.Monto),
                VentasContado = ventas.Where(v => !v.Acredito).Sum(v => v.Total),
                VentasCredito = ventas.Where(v => v.Acredito).Sum(v => v.Total),

                TotalCompras = compras.Sum(c => c.Total),
                CantidadCompras = compras.Count
            };

            // ---------------- VENTAS POR DÍA ----------------

            var costoPorDia = detalles
                .GroupBy(d => DateOnly.FromDateTime(d.Fecha))
                .ToDictionary(g => g.Key, g => g.Sum(d => d.CostoUnitario * d.Cantidad));

            var ventasPorFecha = ventas
                .GroupBy(v => DateOnly.FromDateTime(v.FechaVenta))
                .ToDictionary(g => g.Key, g => new { Cantidad = g.Count(), Total = g.Sum(v => v.Total) });

            // Se incluyen los días sin ventas para que el gráfico sea continuo.
            for (var dia = desde; dia <= hasta; dia = dia.AddDays(1))
            {
                ventasPorFecha.TryGetValue(dia, out var datos);
                var costo = Redondear(costoPorDia.GetValueOrDefault(dia));
                var total = datos?.Total ?? 0;

                reporte.VentasPorDia.Add(new ReporteVentaDiaDto
                {
                    Fecha = dia,
                    CantidadVentas = datos?.Cantidad ?? 0,
                    Total = total,
                    Costo = costo,
                    Utilidad = total - costo
                });
            }

            // ---------------- AGRUPACIONES ----------------

            reporte.VentasPorMetodo = pagos
                .GroupBy(p => (p.MetodoPago ?? "").Trim().ToUpper())
                .Select(g => new ReporteMetodoPagoDto
                {
                    Metodo = string.IsNullOrEmpty(g.Key) ? "SIN MÉTODO" : g.Key,
                    CantidadPagos = g.Count(),
                    Monto = g.Sum(p => p.Monto)
                })
                .OrderByDescending(x => x.Monto)
                .ToList();

            reporte.VentasPorVendedor = ventas
                .GroupBy(v => new { v.IdUsuario, v.Vendedor })
                .Select(g => new ReporteVendedorDto
                {
                    IdUsuario = g.Key.IdUsuario,
                    Vendedor = g.Key.Vendedor.Trim(),
                    CantidadVentas = g.Count(),
                    Total = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            reporte.Productos = detalles
                .GroupBy(d => new { d.IdProducto, d.Codigo, d.Nombre })
                .Select(g =>
                {
                    var ventasProducto = g.Sum(d => d.SubTotal);
                    var costoProducto = Redondear(g.Sum(d => d.CostoUnitario * d.Cantidad));
                    var utilidad = ventasProducto - costoProducto;

                    return new ReporteProductoDto
                    {
                        IdProducto = g.Key.IdProducto,
                        Codigo = g.Key.Codigo,
                        Nombre = g.Key.Nombre,
                        Cantidad = g.Sum(d => d.Cantidad),
                        Ventas = ventasProducto,
                        Costo = costoProducto,
                        Utilidad = utilidad,
                        Margen = Porcentaje(utilidad, ventasProducto)
                    };
                })
                .OrderByDescending(x => x.Ventas)
                .ToList();

            reporte.ComprasPorProveedor = compras
                .GroupBy(c => new { c.IdProveedor, c.Proveedor })
                .Select(g => new ReporteProveedorDto
                {
                    IdProveedor = g.Key.IdProveedor,
                    Proveedor = g.Key.Proveedor,
                    CantidadCompras = g.Count(),
                    Total = g.Sum(c => c.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            reporte.GastosPorConcepto = gastos
                .GroupBy(g => (g.Concepto ?? "").Trim())
                .Select(g => new ReporteGastoConceptoDto
                {
                    Concepto = string.IsNullOrEmpty(g.Key) ? "Sin concepto" : g.Key,
                    Cantidad = g.Count(),
                    Monto = g.Sum(x => x.Monto)
                })
                .OrderByDescending(x => x.Monto)
                .ToList();

            // ---------------- CUENTAS POR COBRAR (estado actual) ----------------

            var creditos = await _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    !v.Estado &&
                    v.EstadoVenta != VentaAnulada &&
                    v.Acredito &&
                    v.SaldoPendiente > 0 &&
                    (!idSucursal.HasValue || v.IdSucursal == idSucursal.Value))
                .Select(v => new
                {
                    v.Id,
                    Cliente = v.IdClienteNavigation == null
                        ? "Consumidor final"
                        : v.IdClienteNavigation.Nombres + " " + (v.IdClienteNavigation.Apellidos ?? ""),
                    v.FechaVenta,
                    v.Total,
                    v.SaldoPendiente,
                    v.FechaLimitePago
                })
                .ToListAsync();

            var creditosDto = creditos
                .Select(c => new ReporteCreditoDto
                {
                    IdVenta = c.Id,
                    Cliente = c.Cliente.Trim(),
                    FechaVenta = c.FechaVenta,
                    Total = c.Total,
                    SaldoPendiente = c.SaldoPendiente,
                    FechaLimitePago = c.FechaLimitePago,
                    DiasVencido = c.FechaLimitePago.HasValue && c.FechaLimitePago.Value < hoy
                        ? hoy.DayNumber - c.FechaLimitePago.Value.DayNumber
                        : 0
                })
                .OrderByDescending(c => c.DiasVencido)
                .ThenBy(c => c.FechaLimitePago ?? DateOnly.MaxValue)
                .ToList();

            reporte.CuentasPorCobrar = new ReporteCuentasPorCobrarDto
            {
                TotalPendiente = creditosDto.Sum(c => c.SaldoPendiente),
                CreditosPendientes = creditosDto.Count,
                MontoVencido = creditosDto.Where(c => c.DiasVencido > 0).Sum(c => c.SaldoPendiente),
                CreditosVencidos = creditosDto.Count(c => c.DiasVencido > 0),
                Creditos = creditosDto.Take(50).ToList()
            };

            // ---------------- INVENTARIO (estado actual) ----------------

            var inventario = await _context.InventarioSucursals
                .AsNoTracking()
                .Where(i =>
                    !i.Estado &&
                    !i.IdProductoNavigation.Estado &&
                    (!idSucursal.HasValue || i.IdSucursal == idSucursal.Value))
                .Select(i => new
                {
                    i.IdProducto,
                    Producto = i.IdProductoNavigation.Nombre,
                    Sucursal = i.IdSucursalNavigation.Nombre,
                    i.StockActual,
                    i.StockMinimo,
                    i.IdProductoNavigation.PrecioCompra
                })
                .ToListAsync();

            var stockBajo = inventario
                .Where(i => i.StockActual <= i.StockMinimo)
                .OrderBy(i => i.StockActual - i.StockMinimo)
                .Select(i => new ReporteStockBajoDto
                {
                    IdProducto = i.IdProducto,
                    Producto = i.Producto,
                    Sucursal = i.Sucursal,
                    StockActual = i.StockActual,
                    StockMinimo = i.StockMinimo
                })
                .ToList();

            var limiteVencimiento = hoy.AddDays(DiasAlertaVencimiento);

            var lotes = await _context.Lotes
                .AsNoTracking()
                .Where(l =>
                    !l.Estado &&
                    l.CantidadDisponible > 0 &&
                    l.FechaVencimiento.HasValue &&
                    l.FechaVencimiento.Value <= limiteVencimiento &&
                    (!idSucursal.HasValue || l.IdSucursal == idSucursal.Value))
                .OrderBy(l => l.FechaVencimiento)
                .Select(l => new
                {
                    l.Id,
                    Producto = l.IdProductoNavigation.Nombre,
                    l.NumeroLote,
                    Sucursal = l.IdSucursalNavigation.Nombre,
                    FechaVencimiento = l.FechaVencimiento!.Value,
                    l.CantidadDisponible
                })
                .ToListAsync();

            reporte.Inventario = new ReporteInventarioDto
            {
                ValorInventario = Redondear(inventario.Sum(i => i.StockActual * i.PrecioCompra)),
                ProductosStockBajo = stockBajo.Count,
                LotesPorVencer = lotes.Count,
                StockBajo = stockBajo.Take(50).ToList(),
                ProximosVencimientos = lotes
                    .Take(50)
                    .Select(l => new ReporteLoteVencimientoDto
                    {
                        IdLote = l.Id,
                        Producto = l.Producto,
                        NumeroLote = l.NumeroLote,
                        Sucursal = l.Sucursal,
                        FechaVencimiento = l.FechaVencimiento,
                        CantidadDisponible = l.CantidadDisponible,
                        DiasRestantes = l.FechaVencimiento.DayNumber - hoy.DayNumber
                    })
                    .ToList()
            };

            return reporte;
        }

        private static decimal Redondear(decimal valor) =>
            decimal.Round(valor, 2, MidpointRounding.AwayFromZero);

        private static decimal Porcentaje(decimal parte, decimal total) =>
            total == 0 ? 0 : Redondear(parte / total * 100);
        public async Task<IEnumerable<ReporteDto>> GetReport(DateTime fechaInicial, DateTime fechaFinal)
        {
            var Objeto = new ReporteDto();
            //var Gastos = await _unitOfWork.Gastos.GetAllAsync();
            //var detalles = await _unitOfWork.DetalleVentas.GetDetallesFecha(fechaInicial, fechaFinal);
            //Objeto.FechaInicio = fechaInicial;
            //Objeto.FechaFinal = fechaFinal;

            //Objeto.InversionTotal = (double)detalles.Sum(x => x.PrecioCompra * x.Cantidad);
            //Objeto.TotalGen = (double)detalles.Sum(x => x.PrecioUnitario * x.Cantidad);
        
            //Objeto.GastoTotal = Gastos.Where(x => x.FechaCreacion.Date >= fechaInicial.Date && x.FechaCreacion.Date <= fechaFinal.Date).Sum(x => x.Monto);
            //Objeto.GanaciaNeta = Objeto.TotalGen - (Objeto.GastoTotal + Objeto.InversionTotal);
                           
            //Objeto.Productos = detalles
            //    .Select(x => new ProductoReporteDto
            //    {
            //        Nombre = x.Producto.Nombre ?? "Desconocido",
            //        Cantidad = x.Cantidad,
            //        PrecioCompra = x.PrecioCompra,
            //        PrecioVenta = x.PrecioUnitario
            //    }).ToList();
            
            return new List<ReporteDto> { Objeto };



        }

    }
}
