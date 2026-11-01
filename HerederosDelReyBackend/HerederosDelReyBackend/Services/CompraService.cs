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

        public async Task<CompraDto> AddAsync(CompraCreateDto dto)
        {
            var compra = _mapper.Map<Compra>(dto);
            await _unitOfWork.Compra.AddAsync(compra);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CompraDto>(compra);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (await _unitOfWork.Compra.GetByIdAsync(id) == null)
                return false;

            await _unitOfWork.Compra.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CompraDto>> GetAllAsync()
        {
            var compras = await _unitOfWork.Compra.GetAllAsync();
            return _mapper.Map<IEnumerable<CompraDto>>(compras);
        }

        public async Task<CompraDto?> GetByIdAsync(int id)
        {
            var compra = await _unitOfWork.Compra.GetByIdAsync(id);
            return compra == null ? null : _mapper.Map<CompraDto>(compra);
        }

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

        public async Task<ApiResponse<IEnumerable<CompraDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var compras = await _unitOfWork.Compra.GetAllAsync(filter);
            var comprasDto = _mapper.Map<IEnumerable<CompraDto>>(compras);
            return new ApiResponse<IEnumerable<CompraDto>>(comprasDto, compras.MetaData);
        }


        public async Task<CompraDto> RegistrarAsync(
    RegistrarCompraDto dto,
    int idUsuario,
    int idSucursal,
    string? rol)
        {
            // 1. Validaciones generales
            if (dto == null)
                throw new ArgumentException("Los datos de la compra son obligatorios.");

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ArgumentException("La compra debe contener al menos un detalle.");

            if (idUsuario <= 0 || idSucursal <= 0)
                throw new ArgumentException("El usuario o la sucursal no son válidos.");

            // 2. Determinar la sucursal
            int sucursalCompra = idSucursal;

            if (dto.IdSucursal.HasValue && dto.IdSucursal.Value != idSucursal)
            {
                bool esAdministrador =
                    string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(rol, "ADMINISTRADOR", StringComparison.OrdinalIgnoreCase);

                if (!esAdministrador)
                    throw new UnauthorizedAccessException(
                        "No tienes permiso para registrar compras en otra sucursal."
                    );

                sucursalCompra = dto.IdSucursal.Value;
            }

            // 3. Validar entidades principales
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(dto.IdProveedor);

            if (proveedor == null)
                throw new ArgumentException("El proveedor seleccionado no existe.");

            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(idUsuario);

            if (usuario == null)
                throw new ArgumentException("El usuario no existe.");

            var sucursal = await _unitOfWork.Sucursale.GetByIdAsync(sucursalCompra);

            if (sucursal == null)
                throw new ArgumentException("La sucursal seleccionada no existe.");

            // 4. Preparar la cabecera
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

            decimal subTotalGeneral = 0;
            decimal descuentoGeneral = 0;
            decimal impuestoGeneral = 0;

            // 5. Procesar cada detalle
            foreach (var item in dto.Detalles)
            {
                if (item.IdProducto <= 0)
                    throw new ArgumentException("Existe un producto con identificador inválido.");

                if (item.Cantidad <= 0)
                    throw new ArgumentException("La cantidad debe ser mayor a cero.");

                if (item.PrecioCompra <= 0)
                    throw new ArgumentException("El precio de compra debe ser mayor a cero.");

                if (item.Descuento < 0 || item.Impuesto < 0)
                    throw new ArgumentException("El descuento y el impuesto no pueden ser negativos.");

                decimal importeBruto = item.Cantidad * item.PrecioCompra;

                if (item.Descuento > importeBruto)
                    throw new ArgumentException(
                        "El descuento no puede superar el importe del detalle."
                    );

                var producto = await _unitOfWork.Productos.GetByIdAsync(item.IdProducto);

                if (producto == null)
                    throw new ArgumentException(
                        $"El producto con ID {item.IdProducto} no existe."
                    );

                // 6. Validar información de lotes
                if (producto.ManejaLotes &&
                    string.IsNullOrWhiteSpace(item.NumeroLote))
                {
                    throw new ArgumentException(
                        $"El producto {producto.Nombre} requiere un número de lote."
                    );
                }

                if (producto.TieneVencimiento && !item.FechaVencimiento.HasValue)
                {
                    throw new ArgumentException(
                        $"El producto {producto.Nombre} requiere fecha de vencimiento."
                    );
                }

                if (item.FechaFabricacion.HasValue &&
                    item.FechaVencimiento.HasValue &&
                    item.FechaFabricacion.Value.Date > item.FechaVencimiento.Value.Date)
                {
                    throw new ArgumentException(
                        $"Las fechas del producto {producto.Nombre} no son válidas."
                    );
                }

                // 7. Crear lote cuando corresponda
                Lote? lote = null;

                if (producto.ManejaLotes || producto.TieneVencimiento)
                {
                    string numeroLote = producto.ManejaLotes
                        ? item.NumeroLote!.Trim()
                        : $"INTERNO-{Guid.NewGuid():N}";

                    lote = new Lote
                    {
                        IdSucursal = sucursalCompra,
                        IdProducto = producto.Id,
                        NumeroLote = numeroLote,

                        FechaFabricacion = item.FechaFabricacion.HasValue
                            ? DateOnly.FromDateTime(item.FechaFabricacion.Value)
                            : null,

                        FechaVencimiento = item.FechaVencimiento.HasValue
                            ? DateOnly.FromDateTime(item.FechaVencimiento.Value)
                            : null,

                        CantidadInicial = item.Cantidad,
                        CantidadDisponible = item.Cantidad,
                        CostoCompra = item.PrecioCompra,
                        Estado = false,
                        FechaRegistro = DateTime.Now
                    };

                    await _unitOfWork.Lote.AddAsync(lote);
                }

                // 8. Calcular importe del detalle
                decimal subtotalDetalle =
                    importeBruto - item.Descuento + item.Impuesto;

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

                // 9. Acumular totales
                subTotalGeneral += importeBruto;
                descuentoGeneral += item.Descuento;
                impuestoGeneral += item.Impuesto;

                // 10. Actualizar inventario si el producto lo controla
                if (producto.ControlaInventario)
                {
                    var inventario =
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
                            StockActual = item.Cantidad,
                            StockMinimo = 0,
                            Estado = false,
                            FechaRegistro = DateTime.Now
                        };

                        await _unitOfWork.InventarioSucursal.AddAsync(inventario);
                    }
                    else
                    {
                        inventario.StockActual += item.Cantidad;
                        inventario.FechaModificacion = DateTime.Now;

                        _unitOfWork.InventarioSucursal.Update(inventario);
                    }
                }

                // 11. Actualizar último precio de compra
                producto.PrecioCompra = item.PrecioCompra;
                producto.FechaModificacion = DateTime.Now;

                _unitOfWork.Productos.Update(producto);
            }

            // 12. Calcular totales de la compra
            compra.SubTotal = subTotalGeneral;
            compra.Descuento = descuentoGeneral;
            compra.Impuesto = impuestoGeneral;

            compra.Total =
                subTotalGeneral - descuentoGeneral + impuestoGeneral;

            // 13. Guardar todo
            await _unitOfWork.Compra.AddAsync(compra);

            await _unitOfWork.SaveChangesAsync();

            // 14. Devolver compra registrada
            return _mapper.Map<CompraDto>(compra);
        }


    }
}
