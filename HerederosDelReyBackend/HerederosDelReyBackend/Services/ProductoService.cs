using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Exceptions;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;


        public ProductoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductoDto> AddAsync(ProductoCreateDto dto)
        {
            await ValidarCodigosAsync(dto.Codigo, dto.CodigoBarras, null);

            var Objeto = _mapper.Map<Producto>(dto);
            Objeto.Codigo = Objeto.Codigo.Trim();
            Objeto.CodigoBarras = string.IsNullOrWhiteSpace(Objeto.CodigoBarras) ? null : Objeto.CodigoBarras.Trim();

            await _unitOfWork.Productos.AddAsync(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProductoDto>(Objeto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var objeto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (objeto == null)
                return false;

            await _unitOfWork.Productos.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductoDto>> GetAllAsync()
        {
            var lista = await _unitOfWork.Productos.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductoDto>>(lista);
        }

        public async Task<ProductoDto?> GetByIdAsync(int id)
        {
            var objeto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (objeto == null)
                return null;

            return _mapper.Map<ProductoDto>(objeto);
        }


        public async Task<bool> UpdateAsync(int id, ProductoUpdateDto dto)
        {
            var objeto = await _unitOfWork.Productos.GetByIdAsync(id);

            if (objeto == null)
                return false;

            await ValidarCodigosAsync(dto.Codigo, dto.CodigoBarras, id);

            objeto.IdCategoria = dto.IdCategoria;
            objeto.IdMarca = dto.IdMarca;

            objeto.Codigo = dto.Codigo.Trim();
            objeto.CodigoBarras = string.IsNullOrWhiteSpace(dto.CodigoBarras) ? null : dto.CodigoBarras.Trim();

            objeto.Nombre = dto.Nombre;
            objeto.NombreGenerico = dto.NombreGenerico;
            objeto.Descripcion = dto.Descripcion;

            objeto.PrecioCompra = dto.PrecioCompra;
            objeto.PrecioVenta = dto.PrecioVenta;
            objeto.Peso = dto.Peso;

            objeto.ControlaInventario = dto.ControlaInventario;
            objeto.ManejaLotes = dto.ManejaLotes;
            objeto.TieneVencimiento = dto.TieneVencimiento;
            objeto.PermiteVenta = dto.PermiteVenta;

            _unitOfWork.Productos.Update(objeto);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // El código es único en la base de datos (incluye productos eliminados)
        // y el código de barras no puede repetirse entre productos activos.
        private async Task ValidarCodigosAsync(string? codigo, string? codigoBarras, int? idActual)
        {
            var codigoLimpio = codigo?.Trim();
            if (!string.IsNullOrEmpty(codigoLimpio))
            {
                var conCodigo = await _unitOfWork.Productos
                    .GetAllIncluyendoEliminados()
                    .Where(p => p.Codigo == codigoLimpio && (idActual == null || p.Id != idActual))
                    .Select(p => new { p.Estado })
                    .FirstOrDefaultAsync();

                if (conCodigo != null)
                {
                    throw new DatoDuplicadoException(conCodigo.Estado
                        ? $"El código \"{codigoLimpio}\" pertenece a un producto eliminado. Usa otro código."
                        : $"Ya existe un producto con el código \"{codigoLimpio}\".");
                }
            }

            var barrasLimpio = codigoBarras?.Trim();
            if (!string.IsNullOrEmpty(barrasLimpio))
            {
                var existe = await _unitOfWork.Productos
                    .GetAllAsQueryable()
                    .AnyAsync(p => p.CodigoBarras == barrasLimpio && (idActual == null || p.Id != idActual));

                if (existe)
                    throw new DatoDuplicadoException($"Ya existe un producto con el código de barras {barrasLimpio}.");
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductoDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var objeto = await _unitOfWork.Productos.GetAllAsync(filter);
            var objetoDto = _mapper.Map<IEnumerable<ProductoDto>>(objeto);

            return new ApiResponse<IEnumerable<ProductoDto>>(objetoDto, objeto.MetaData);
        }


        public async Task<ApiResponse<IEnumerable<ProductoDto>>> GetBySucursalAsync(
            PostQueryFilter filter,
            int idSucursal)
        {
            var productos = await _unitOfWork.Productos
                .GetBySucursalAsync(filter, idSucursal);

            var productosDto = _mapper.Map<IEnumerable<ProductoDto>>(productos);

            return new ApiResponse<IEnumerable<ProductoDto>>(
                productosDto,
                productos.MetaData
            );
        }

    }
}
