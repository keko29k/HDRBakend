using AutoMapper;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Venta, VentaDto>().ReverseMap();
            CreateMap<VentaCreateDto, Venta>().ReverseMap();
            CreateMap<VentaUpdateDto, Venta>().ReverseMap();


            CreateMap<DetalleVentaDto, DetalleVenta>().ReverseMap();

            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<UsuarioCreateDto, Usuario>().ReverseMap();
            CreateMap<UsuarioUpdateDto, Usuario>().ReverseMap();

            CreateMap<VentaDto, Venta>().ReverseMap();

            CreateMap<Cliente, ClienteDto>().ReverseMap();
            CreateMap<ClienteCreateDto, Cliente>().ReverseMap();
            CreateMap<ClienteUpdateDto, Cliente>().ReverseMap();

            CreateMap<Categoria, CategoriaDto>().ReverseMap();
            CreateMap<CategoriaCreateDto, Categoria>().ReverseMap();
            CreateMap<CategoriaUpdateDto, Categoria>().ReverseMap();

            CreateMap<Proveedore, ProveedoresDto>().ReverseMap();
            CreateMap<ProveedoresCreateDto, Proveedore>().ReverseMap();
            CreateMap<ProveedoresUptadeDto, Proveedore>().ReverseMap();

            CreateMap<Marca, MarcaDto>().ReverseMap();
            CreateMap<MarcaCreateDto, Marca>().ReverseMap();
            CreateMap<MarcaUpdateDto, Marca>().ReverseMap();

            CreateMap<Producto, ProductoDto>()
                     .ForMember(dest => dest.StockActual,
                         opt => opt.MapFrom(src =>
                             src.Lotes
                                 .Where(l => !l.Estado)
                                 .Sum(l => l.CantidadDisponible)
                         ));
            CreateMap<ProductoCreateDto, Producto>().ReverseMap();
            CreateMap<ProductoUpdateDto, Producto>().ReverseMap();



            CreateMap<Gasto, GastosDto>().ReverseMap();
            CreateMap<GastosCreateDto, Gasto>().ReverseMap();
            CreateMap<GastosUpdateDto, Gasto>().ReverseMap();


            CreateMap<Caja, CajaDto>()
               .ForMember(d => d.NombreSucursal,
                   o => o.MapFrom(s => s.IdSucursalNavigation.Nombre))
               .ForMember(d => d.NombreUsuario,
                   o => o.MapFrom(s => s.IdUsuarioAperturaNavigation.Nombres + " " +
                                       s.IdUsuarioAperturaNavigation.Apellidos)); CreateMap<CajaCreateDto, Caja>().ReverseMap();
            CreateMap<CajaUpdateDto, Caja>().ReverseMap();



            CreateMap<Compra, CompraDto>()
           .ForMember(d => d.Sucursal,
               o => o.MapFrom(s => s.IdSucursalNavigation.Nombre))
           .ForMember(d => d.Proveedor,
               o => o.MapFrom(s => s.IdProveedorNavigation.Nombre))
           .ForMember(d => d.Usuario,
               o => o.MapFrom(s => s.IdUsuarioNavigation.Nombres));

            CreateMap<CompraCreateDto, Compra>().ReverseMap();
            CreateMap<CompraUpdateDto, Compra>().ReverseMap();


            CreateMap<DetalleCompra, DetalleCompraDto>().ReverseMap();
            CreateMap<DetalleCompraCreateDto, DetalleCompra>().ReverseMap();
            CreateMap<DetalleCompraUpdateDto, DetalleCompra>().ReverseMap();



            CreateMap<ImagenesProducto, ImagenesProductoDto>().ReverseMap();
            CreateMap<ImagenesProductoCreateDto, ImagenesProducto>().ReverseMap();
            CreateMap<ImagenesProductoUpdateDto, ImagenesProducto>().ReverseMap();

            CreateMap<Sucursale, SucursaleDto>().ReverseMap();
            CreateMap<SucursaleCreateDto, Sucursale>().ReverseMap();
            CreateMap<SucursaleUpdateDto, Sucursale>().ReverseMap();

            CreateMap<InventarioSucursal, InventarioSucursalDto>().ReverseMap();
            CreateMap<InventarioSucursalCreateDto, InventarioSucursal>().ReverseMap();
            CreateMap<InventarioSucursalUpdateDto, InventarioSucursal>().ReverseMap();


            CreateMap<Lote, LoteDto>().ReverseMap();
            CreateMap<LoteCreateDto, Lote>().ReverseMap();
            CreateMap<LoteUpdateDto, Lote>().ReverseMap();

        }
    }
}