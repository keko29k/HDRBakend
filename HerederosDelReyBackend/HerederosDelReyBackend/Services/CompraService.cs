
using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.Services
{
    public class CompraService : ICompraService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CompraService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // =====================================================
        // REGISTRAR COMPRA SIMPLE
        // =====================================================

        public async Task<CompraDto> AddAsync(CompraCreateDto dto)
        {
            var compra = _mapper.Map<Compra>(dto);

            await _unitOfWork.Compra.AddAsync(compra);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CompraDto>(compra);
        }

        // =====================================================
        // ELIMINAR COMPRA
        // =====================================================

        public async Task<bool> DeleteAsync(int id)
        {
            if (await _unitOfWork.Compra.GetByIdAsync(id) == null)
                return false;

            await _unitOfWork.Compra.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // =====================================================
        // LISTAR COMPRAS
        // =====================================================

        public async Task<IEnumerable<CompraDto>> GetAllAsync()
        {
            var compras = await _unitOfWork.Compra.GetAllAsync();

            return _mapper.Map<IEnumerable<CompraDto>>(compras);
        }

        // =====================================================
        // OBTENER COMPRA POR ID
        // =====================================================

        public async Task<CompraDto?> GetByIdAsync(int id)
        {
            var compra = await _unitOfWork.Compra.GetByIdAsync(id);

            return compra == null
                ? null
                : _mapper.Map<CompraDto>(compra);
        }

        // =====================================================
        // ACTUALIZAR COMPRA
        // =====================================================

        public async Task<bool> UpdateAsync(int id, CompraUpdateDto dto)
        {
            var compra = await _unitOfWork.Compra.GetByIdAsync(id);

            if (compra == null)
                return false;

            compra.TipoDocumento = dto.TipoDocumento;
            compra.NumeroDocumento = dto.NumeroDocumento;
            compra.Descuento = dto.Descuento;
            compra.Observacion = dto.Observacion;
            compra.EstadoCompra = dto.EstadoCompra;

            _unitOfWork.Compra.Update(compra);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // =====================================================
        // LISTAR COMPRAS CON FILTROS
        // =====================================================

        public async Task<ApiResponse<IEnumerable<CompraDto>>> GetAllAsync(
            PostQueryFilter filter)
        {
            var compras = await _unitOfWork.Compra.GetAllAsync(filter);

            var comprasDto = _mapper.Map<IEnumerable<CompraDto>>(compras);

            return new ApiResponse<IEnumerable<CompraDto>>(
                comprasDto,
                compras.MetaData
            );
        }

        // =====================================================
        // REGISTRAR COMPRA CON DETALLES, LOTES E INVENTARIO
        // =====================================================

        public async Task<CompraDto> RegistrarAsync(
            RegistrarCompraDto dto,
            int idUsuario,
            int idSucursal,
            string? rol)
        {
            // -------------------------------------------------
            // 1. VALIDACIONES GENERALES
            // -------------------------------------------------

            if (dto == null)
                throw new ArgumentException(
                    "Los datos de la compra son obligatorios."
                );

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ArgumentException(
                    "La compra debe contener al menos un detalle."
                );

            if (idUsuario <= 0 || idSucursal <= 0)
                throw new ArgumentException(
                    "El usuario o la sucursal no son válidos."
                );

            if (dto.IdProveedor <= 0)
                throw new ArgumentException(
                    "Debe seleccionar un proveedor válido."
                );

            if (dto.FechaCompra == default)
                throw new ArgumentException(
                    "La fecha de compra es obligatoria."
                );

            // -------------------------------------------------
            // 2. DETERMINAR SUCURSAL
            // -------------------------------------------------

            int sucursalCompra = idSucursal;

            if (dto.IdSucursal.HasValue &&
                dto.IdSucursal.Value != idSucursal)
            {
                bool esAdministrador =
                    string.Equals(
                        rol,
                        "ADMIN",
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    string.Equals(
                        rol,
                        "ADMINISTRADOR",
                        StringComparison.OrdinalIgnoreCase
                    );

                if (!esAdministrador)
                {
                    throw new UnauthorizedAccessException(
                        "No tienes permiso para registrar compras en otra sucursal."
                    );
                }

                if (dto.IdSucursal.Value <= 0)
                    throw new ArgumentException(
                        "La sucursal seleccionada no es válida."
                    );

                sucursalCompra = dto.IdSucursal.Value;
            }

            // -------------------------------------------------
            // 3. VALIDAR PROVEEDOR, USUARIO Y SUCURSAL
            // -------------------------------------------------

            var proveedor =
                await _unitOfWork.Proveedores.GetByIdAsync(
                    dto.IdProveedor
                );

            if (proveedor == null)
                throw new ArgumentException(
                    "El proveedor seleccionado no existe."
                );

            var usuario =
                await _unitOfWork.Usuarios.GetByIdAsync(idUsuario);

            if (usuario == null)
                throw new ArgumentException(
                    "El usuario no existe."
                );

            var sucursal =
                await _unitOfWork.Sucursale.GetByIdAsync(
                    sucursalCompra
                );

            if (sucursal == null)
                throw new ArgumentException(
                    "La sucursal seleccionada no existe."
                );

            // -------------------------------------------------
            // 4. PREPARAR CABECERA DE COMPRA
            // -------------------------------------------------

            var compra = new Compra
            {
                IdSucursal = sucursalCompra,
                IdProveedor = dto.IdProveedor,
                IdUsuario = idUsuario,

                IdProveedorNavigation = proveedor,
                IdSucursalNavigation = sucursal,
                IdUsuarioNavigation = usuario,

                TipoDocumento = dto.TipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                FechaCompra = dto.FechaCompra,
                Observacion = dto.Observacion,

                EstadoCompra = "REGISTRADA",

                SubTotal = 0,
                Descuento = 0,
                Impuesto = 0,
                Total = 0
            };

            // -------------------------------------------------
            // 5. PREPARAR ACUMULADORES Y CACHÉS
            // -------------------------------------------------

            decimal subTotalGeneral = 0;
            decimal descuentoGeneral = 0;
            decimal impuestoGeneral = 0;

            // Evita consultar varias veces el mismo producto.
            var productosProcesados =
                new Dictionary<int, Producto>();

            // Evita crear dos inventarios para el mismo producto
            // cuando aparece repetido en los detalles.
            var inventariosProcesados =
                new Dictionary<int, InventarioSucursal>();

            // -------------------------------------------------
            // 6. PROCESAR DETALLES
            // -------------------------------------------------

            foreach (var item in dto.Detalles)
            {
                // ---------------------------------------------
                // 6.1 VALIDAR DATOS DEL DETALLE
                // ---------------------------------------------

                if (item.IdProducto <= 0)
                    throw new ArgumentException(
                        "Existe un producto con identificador inválido."
                    );

                if (item.Cantidad <= 0)
                    throw new ArgumentException(
                        "La cantidad debe ser mayor a cero."
                    );

                if (item.PrecioCompra <= 0)
                    throw new ArgumentException(
                        "El precio de compra debe ser mayor a cero."
                    );

                if (item.Descuento < 0 || item.Impuesto < 0)
                    throw new ArgumentException(
                        "El descuento y el impuesto no pueden ser negativos."
                    );

                decimal importeBruto =
                    item.Cantidad * item.PrecioCompra;

                if (item.Descuento > importeBruto)
                    throw new ArgumentException(
                        "El descuento no puede superar el importe del detalle."
                    );

                // ---------------------------------------------
                // 6.2 OBTENER PRODUCTO
                // ---------------------------------------------

                if (!productosProcesados.TryGetValue(
                    item.IdProducto,
                    out var producto))
                {
                    producto =
                        await _unitOfWork.Productos.GetByIdAsync(
                            item.IdProducto
                        );

                    if (producto == null)
                    {
                        throw new ArgumentException(
                            $"El producto con ID {item.IdProducto} no existe."
                        );
                    }

                    productosProcesados.Add(
                        item.IdProducto,
                        producto
                    );
                }

                // ---------------------------------------------
                // 6.3 VALIDAR CONFIGURACIÓN DEL PRODUCTO
                // ---------------------------------------------

                if (producto.TieneVencimiento &&
                    !producto.ManejaLotes)
                {
                    throw new ArgumentException(
                        $"El producto {producto.Nombre} tiene vencimiento, pero no maneja lotes."
                    );
                }

                if (producto.ManejaLotes &&
                    !producto.ControlaInventario)
                {
                    throw new ArgumentException(
                        $"El producto {producto.Nombre} debe controlar inventario para manejar lotes."
                    );
                }

                // ---------------------------------------------
                // 6.4 VALIDAR INFORMACIÓN DEL LOTE
                // ---------------------------------------------

                if (producto.ManejaLotes)
                {
                    if (string.IsNullOrWhiteSpace(item.NumeroLote))
                    {
                        throw new ArgumentException(
                            $"El producto {producto.Nombre} requiere un número de lote."
                        );
                    }

                    if (producto.TieneVencimiento &&
                        !item.FechaVencimiento.HasValue)
                    {
                        throw new ArgumentException(
                            $"El producto {producto.Nombre} requiere fecha de vencimiento."
                        );
                    }

                    if (item.FechaVencimiento.HasValue &&
                        item.FechaVencimiento.Value.Date <
                        dto.FechaCompra.Date)
                    {
                        throw new ArgumentException(
                            $"El producto {producto.Nombre} tiene una fecha de vencimiento anterior a la compra."
                        );
                    }

                    if (item.FechaFabricacion.HasValue &&
                        item.FechaFabricacion.Value.Date >
                        dto.FechaCompra.Date)
                    {
                        throw new ArgumentException(
                            $"La fecha de fabricación del producto {producto.Nombre} no puede ser posterior a la compra."
                        );
                    }

                    if (item.FechaFabricacion.HasValue &&
                        item.FechaVencimiento.HasValue &&
                        item.FechaFabricacion.Value.Date >
                        item.FechaVencimiento.Value.Date)
                    {
                        throw new ArgumentException(
                            $"Las fechas del producto {producto.Nombre} no son válidas."
                        );
                    }
                }
                else
                {
                    if (item.FechaVencimiento.HasValue ||
                        item.FechaFabricacion.HasValue ||
                        !string.IsNullOrWhiteSpace(item.NumeroLote))
                    {
                        throw new ArgumentException(
                            $"El producto {producto.Nombre} no está configurado para manejar información de lotes."
                        );
                    }
                }

                // ---------------------------------------------
                // 6.5 CREAR LOTE NUEVO SI CORRESPONDE
                // ---------------------------------------------

                Lote? lote = null;

                if (producto.ManejaLotes)
                {
                    lote = new Lote
                    {
                        IdSucursal = sucursalCompra,
                        IdProducto = producto.Id,

                        NumeroLote = item.NumeroLote!.Trim(),

                        FechaFabricacion =
                            item.FechaFabricacion.HasValue
                                ? DateOnly.FromDateTime(
                                    item.FechaFabricacion.Value
                                  )
                                : null,

                        FechaVencimiento =
                            item.FechaVencimiento.HasValue
                                ? DateOnly.FromDateTime(
                                    item.FechaVencimiento.Value
                                  )
                                : null,

                        CantidadInicial = item.Cantidad,
                        CantidadDisponible = item.Cantidad,

                        CostoCompra = item.PrecioCompra,

                        Estado = false,
                        FechaRegistro = DateTime.Now
                    };

                    await _unitOfWork.Lote.AddAsync(lote);
                }

                // ---------------------------------------------
                // 6.6 CREAR DETALLE DE COMPRA
                // ---------------------------------------------

                decimal subtotalDetalle =
                    importeBruto -
                    item.Descuento +
                    item.Impuesto;

                var detalle = new DetalleCompra
                {
                    IdProducto = producto.Id,
                    IdProductoNavigation = producto,

                    IdCompraNavigation = compra,

                    IdLoteNavigation = lote,

                    Cantidad = item.Cantidad,
                    PrecioCompra = item.PrecioCompra,

                    Descuento = item.Descuento,
                    Impuesto = item.Impuesto,

                    SubTotal = subtotalDetalle,

                    Estado = false,
                    FechaRegistro = DateTime.Now
                };

                compra.DetalleCompras.Add(detalle);

                // ---------------------------------------------
                // 6.7 ACUMULAR TOTALES
                // ---------------------------------------------

                subTotalGeneral += importeBruto;
                descuentoGeneral += item.Descuento;
                impuestoGeneral += item.Impuesto;

                // ---------------------------------------------
                // 6.8 ACTUALIZAR INVENTARIO DE SUCURSAL
                // ---------------------------------------------

                if (producto.ControlaInventario)
                {
                    if (!inventariosProcesados.TryGetValue(
                        producto.Id,
                        out var inventario))
                    {
                        inventario =
                            await _unitOfWork.InventarioSucursal
                                .GetBySucursalProductoAsync(
                                    sucursalCompra,
                                    producto.Id
                                );

                        if (inventario == null)
                        {
                            inventario = new InventarioSucursal
                            {
                                IdSucursal = sucursalCompra,
                                IdProducto = producto.Id,

                                StockActual = 0,
                                StockMinimo = 0,

                                Estado = false,
                                FechaRegistro = DateTime.Now
                            };

                            await _unitOfWork.InventarioSucursal
                                .AddAsync(inventario);
                        }

                        inventariosProcesados.Add(
                            producto.Id,
                            inventario
                        );
                    }

                    inventario.StockActual += item.Cantidad;
                    inventario.FechaModificacion = DateTime.Now;

                    _unitOfWork.InventarioSucursal.Update(
                        inventario
                    );
                }

                // ---------------------------------------------
                // 6.9 ACTUALIZAR ÚLTIMO PRECIO DE COMPRA
                // ---------------------------------------------

                producto.PrecioCompra = item.PrecioCompra;
                producto.FechaModificacion = DateTime.Now;

                _unitOfWork.Productos.Update(producto);
            }

            // -------------------------------------------------
            // 7. CALCULAR TOTALES DE LA COMPRA
            // -------------------------------------------------

            compra.SubTotal = subTotalGeneral;
            compra.Descuento = descuentoGeneral;
            compra.Impuesto = impuestoGeneral;

            compra.Total =
                subTotalGeneral -
                descuentoGeneral +
                impuestoGeneral;

            if (compra.Total < 0)
                throw new ArgumentException(
                    "El total de la compra no puede ser negativo."
                );

            // -------------------------------------------------
            // 8. GUARDAR COMPRA Y TODOS SUS DETALLES
            // -------------------------------------------------

            await _unitOfWork.Compra.AddAsync(compra);

            await _unitOfWork.SaveChangesAsync();

            // -------------------------------------------------
            // 9. DEVOLVER RESULTADO
            // -------------------------------------------------

            return _mapper.Map<CompraDto>(compra);
        }
    }
}
