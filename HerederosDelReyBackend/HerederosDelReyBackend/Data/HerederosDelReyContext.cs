using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;

namespace HerederosDelReyBackend.Data;

public partial class HerederosDelReyContext : DbContext
{
    public HerederosDelReyContext()
    {
    }

    public HerederosDelReyContext(DbContextOptions<HerederosDelReyContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Caja> Cajas { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

    public virtual DbSet<DetalleVenta> DetalleVentas { get; set; }

    public virtual DbSet<Gasto> Gastos { get; set; }

    public virtual DbSet<ImagenesProducto> ImagenesProductos { get; set; }

    public virtual DbSet<InventarioSucursal> InventarioSucursals { get; set; }

    public virtual DbSet<Lote> Lotes { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Sucursale> Sucursales { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=KevinFarmacia;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Caja__3214EC075CF07D0C");

            entity.ToTable("Caja");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.EstadoCaja)
                .HasMaxLength(20)
                .HasDefaultValue("ABIERTA");
            entity.Property(e => e.FechaApertura).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MontoFinal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MontoInicial).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Observacion).HasMaxLength(500);

            entity.HasOne(d => d.IdSucursalNavigation).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.IdSucursal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Caja_Sucursal");

            entity.HasOne(d => d.IdUsuarioAperturaNavigation).WithMany(p => p.CajaIdUsuarioAperturaNavigations)
                .HasForeignKey(d => d.IdUsuarioApertura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Caja_UsuarioApertura");

            entity.HasOne(d => d.IdUsuarioCierreNavigation).WithMany(p => p.CajaIdUsuarioCierreNavigations)
                .HasForeignKey(d => d.IdUsuarioCierre)
                .HasConstraintName("FK_Caja_UsuarioCierre");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07D8BD615F");

            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(120);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clientes__3214EC074CD4AE7A");

            entity.Property(e => e.Apellidos).HasMaxLength(120);
            entity.Property(e => e.Correo).HasMaxLength(120);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NitCi).HasMaxLength(30);
            entity.Property(e => e.Nombres).HasMaxLength(120);
            entity.Property(e => e.Observacion).HasMaxLength(500);
            entity.Property(e => e.Telefono).HasMaxLength(30);
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Compras__3214EC078FDF0EC6");

            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.EstadoCompra)
                .HasMaxLength(20)
                .HasDefaultValue("COMPLETADA");
            entity.Property(e => e.FechaCompra).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Impuesto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NumeroDocumento).HasMaxLength(50);
            entity.Property(e => e.Observacion).HasMaxLength(500);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(30)
                .HasDefaultValue("RECIBO");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Proveedor");

            entity.HasOne(d => d.IdSucursalNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdSucursal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Sucursal");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compras_Usuario");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DetalleC__3214EC074CEBC348");

            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Impuesto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrecioCompra).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DCompra_Compra");

            entity.HasOne(d => d.IdLoteNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdLote)
                .HasConstraintName("FK_DCompra_Lote");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DCompra_Producto");
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DetalleV__3214EC07C1830767");

            entity.Property(e => e.Cantidad).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Descuento).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Impuesto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdLoteNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdLote)
                .HasConstraintName("FK_DVenta_Lote");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DVenta_Producto");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DVenta_Venta");
        });

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Gastos__3214EC0743A1F534");

            entity.Property(e => e.Concepto).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NumeroComprobante).HasMaxLength(50);
            entity.Property(e => e.Observacion).HasMaxLength(500);

            entity.HasOne(d => d.IdCajaNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdCaja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gasto_Caja");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gasto_Usuario");
        });

        modelBuilder.Entity<ImagenesProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Imagenes__3214EC073EA0269E");

            entity.Property(e => e.EsPrincipal).HasDefaultValue(false);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NombreArchivo).HasMaxLength(200);
            entity.Property(e => e.RutaImagen).HasMaxLength(500);

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ImagenesProductos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Imagen_Producto");
        });

        modelBuilder.Entity<InventarioSucursal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventar__3214EC076E167ED1");

            entity.ToTable("InventarioSucursal");

            entity.HasIndex(e => new { e.IdSucursal, e.IdProducto }, "UQ_InventarioSucursal").IsUnique();

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PuntoReorden).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.StockActual).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.StockMaximo).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.StockMinimo).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Ubicacion).HasMaxLength(100);

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.InventarioSucursals)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventarioSucursal_Producto");

            entity.HasOne(d => d.IdSucursalNavigation).WithMany(p => p.InventarioSucursals)
                .HasForeignKey(d => d.IdSucursal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventarioSucursal_Sucursal");
        });

        modelBuilder.Entity<Lote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lotes__3214EC078D4DBACB");

            entity.Property(e => e.CantidadDisponible).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.CantidadInicial).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.CostoCompra).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.NumeroLote).HasMaxLength(100);

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Lotes)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lote_Producto");

            entity.HasOne(d => d.IdSucursalNavigation).WithMany(p => p.Lotes)
                .HasForeignKey(d => d.IdSucursal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lote_Sucursal");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Marcas__3214EC07422EA2AB");

            entity.Property(e => e.Descripcion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(120);
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pagos__3214EC073DBE0DA7");

            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MetodoPago).HasMaxLength(50);
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Observacion).HasMaxLength(500);
            entity.Property(e => e.Referencia).HasMaxLength(150);

            entity.HasOne(d => d.IdCajaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdCaja)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Caja");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Venta");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07EDCCA9CF");

            entity.HasIndex(e => e.Codigo, "UQ_Productos_Codigo").IsUnique();

            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.CodigoBarras).HasMaxLength(100);
            entity.Property(e => e.ControlaInventario).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.NombreGenerico).HasMaxLength(200);
            entity.Property(e => e.PermiteVenta).HasDefaultValue(true);
            entity.Property(e => e.Peso).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.PrecioCompra).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_Categoria");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdMarca)
                .HasConstraintName("FK_Producto_Marca");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Proveedo__3214EC072E29F257");

            entity.Property(e => e.Contacto).HasMaxLength(120);
            entity.Property(e => e.Correo).HasMaxLength(120);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nit).HasMaxLength(30);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Observacion).HasMaxLength(500);
            entity.Property(e => e.Telefono).HasMaxLength(30);
        });

        modelBuilder.Entity<Sucursale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sucursal__3214EC073556EAD8");

            entity.Property(e => e.Correo).HasMaxLength(120);
            entity.Property(e => e.Direccion).HasMaxLength(250);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(30);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07F17921F2");

            entity.HasIndex(e => e.Usuario1, "UQ_Usuarios").IsUnique();

            entity.Property(e => e.Apellidos).HasMaxLength(120);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Foto).HasMaxLength(300);
            entity.Property(e => e.Nombres).HasMaxLength(120);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Telefono).HasMaxLength(30);
            entity.Property(e => e.Usuario1)
                .HasMaxLength(50)
                .HasColumnName("Usuario");

            entity.HasOne(d => d.IdSucursalNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdSucursal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Sucursal");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ventas__3214EC076BECC2BE");

            entity.Property(e => e.Acredito).HasColumnName("ACredito");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.EstadoVenta)
                .HasMaxLength(20)
                .HasDefaultValue("COMPLETADA");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FechaVenta).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MontoPagado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Observacion).HasMaxLength(500);
            entity.Property(e => e.SaldoPendiente).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_Venta_Cliente");

            entity.HasOne(d => d.IdSucursalNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdSucursal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Sucursal");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}