using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace HerederosDelReyBackend.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUsuarioRepository Usuarios { get; }
        IClienteRepository Clientes { get; }

        ICategoriaRepository Categorias { get; }
        IProveedorRepository Proveedores { get; }
        ICompraRepository Compra { get; }
        IDetalleCompraRepository DetalleCompras { get; }
        ICajaRepository Caja { get; }
        IVentaRepository Ventas { get; }
        IImagenesProductoRepository ImagenesProducto { get; }
        IInventarioSucursalRepository InventarioSucursal { get; }

        IMarcaRepository Marca { get; }

        IGastosRepository Gastos { get; }

        IProductoRepository Productos { get; }

        IDetalleVentaRepository DetalleVentas { get; }
        ISucursaleRepository Sucursale { get; }

        ILoteRepository Lote { get; }

        Task<int> SaveChangesAsync();

        Task<IDbContextTransaction> BeginTransactionAsync(
      IsolationLevel isolationLevel = IsolationLevel.Serializable);


    }
}
