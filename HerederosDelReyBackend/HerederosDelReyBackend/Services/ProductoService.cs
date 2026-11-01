using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;

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
            var Objeto = _mapper.Map<Producto>(dto);

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

            objeto.IdCategoria = dto.IdCategoria;
            objeto.IdMarca = dto.IdMarca;

            objeto.Codigo = dto.Codigo;
            objeto.CodigoBarras = dto.CodigoBarras;

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
