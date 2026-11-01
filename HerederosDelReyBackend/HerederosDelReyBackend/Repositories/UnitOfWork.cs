using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace HerederosDelReyBackend.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HerederosDelReyContext _context;
        private IUsuarioRepository? _usuarioRepository;
        private IClienteRepository? _clienteRepository;
        private ICategoriaRepository? _categoriaRepository;
        private IProveedorRepository? _proveedorRepository;
        private ICompraRepository? _compraRepository;
        private IDetalleCompraRepository? _detallecompraRepository;
        private ICajaRepository? _cajaRepository;
        private IVentaRepository? _ventaRepository;
        private IMarcaRepository? _marcaRepository;
        private IGastosRepository? _gastoRepository;
        private IProductoRepository? _productoRepository;
        private IDetalleVentaRepository? _detalleVentaRepository;
        private IImagenesProductoRepository? _imagenesProductoRepository;
        private IInventarioSucursalRepository? _inventarioSucursalRepository;
        private ISucursaleRepository? _sucursaleRepository;

        private ILoteRepository? _loteRepository;

        private IReporteService? _reporteService;
      

        public UnitOfWork(HerederosDelReyContext context)
        {
            _context = context;
        }
        public IUsuarioRepository Usuarios
           => _usuarioRepository ??= new UsuarioRepository(_context);
        public IClienteRepository Clientes
           => _clienteRepository ??= new ClienteRepository(_context);

        public ICategoriaRepository Categorias
            => _categoriaRepository ??= new CategoriaRepository(_context);

        public IProveedorRepository Proveedores
            => _proveedorRepository ??= new ProveedorRepository(_context);

        public ICompraRepository Compra
            => _compraRepository ??= new CompraRepository(_context);

        public IDetalleCompraRepository DetalleCompras
            => _detallecompraRepository ??= new DetalleCompraRepository(_context);
        


        public IImagenesProductoRepository ImagenesProducto 
            => _imagenesProductoRepository ??= new ImagenesProductoRepository(_context);



        public IInventarioSucursalRepository InventarioSucursal
            => _inventarioSucursalRepository ??= new InventarioSucursalRepository(_context);




        public ICajaRepository Caja
            => _cajaRepository ??= new CajaRepository(_context);

        public IVentaRepository Ventas
            => _ventaRepository ??= new VentaRepository(_context);

        public IMarcaRepository Marca
            => _marcaRepository ??= new MarcaRepository(_context);

        public IGastosRepository Gastos
            => _gastoRepository ??= new GastosRepository(_context);

        public IProductoRepository Productos
            => _productoRepository ??= new ProductoRepository(_context);
            
        public IDetalleVentaRepository DetalleVentas
            => _detalleVentaRepository ??= new DetalleVentaRepository(_context);


        public ISucursaleRepository Sucursale
            => _sucursaleRepository ??= new SucursaleRepository(_context);

        public ILoteRepository Lote
            => _loteRepository ??= new LoteRepository(_context);

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
