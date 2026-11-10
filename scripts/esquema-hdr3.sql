/* =====================================================================
   HEREDEROS DEL REY · ESQUEMA DE LA BASE DE DATOS
   Base de origen: HDR3 (SQL Server 2016 / LocalDB)
   Generado el 2026-11-07 a partir del catálogo de la base (sys.tables,
   sys.columns, sys.indexes, sys.foreign_keys, sys.default_constraints).

   Contenido: 17 tablas, sus claves primarias, valores por
   defecto, índices únicos y 27 llaves foráneas. Sin datos.
   Intercalación de la base: SQL_Latin1_General_CP1_CI_AS

   Uso:
     1. (Opcional) Crea la base descomentando el bloque CREATE DATABASE.
     2. Ejecuta el script completo sobre la base vacía.
   ===================================================================== */

/*
CREATE DATABASE [HDR3] COLLATE SQL_Latin1_General_CP1_CI_AS;
GO
USE [HDR3];
GO
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =====================================================================
   1. TABLAS (con clave primaria y valores por defecto)
   ===================================================================== */

-- Caja
CREATE TABLE [dbo].[Caja] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdSucursal] int NOT NULL,
    [IdUsuarioApertura] int NOT NULL,
    [IdUsuarioCierre] int NULL,
    [FechaApertura] datetime2 NOT NULL CONSTRAINT [DF__Caja__FechaApert__5CD6CB2B] DEFAULT (getdate()),
    [FechaCierre] datetime2 NULL,
    [MontoInicial] decimal(18, 2) NOT NULL,
    [MontoFinal] decimal(18, 2) NULL,
    [EstadoCaja] nvarchar(20) NOT NULL CONSTRAINT [DF__Caja__EstadoCaja__5DCAEF64] DEFAULT ('ABIERTA'),
    [Observacion] nvarchar(500) NULL,
    [Estado] bit NOT NULL CONSTRAINT [DF__Caja__Estado__5EBF139D] DEFAULT ((1)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__Caja__FechaRegis__5FB337D6] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Caja__3214EC0712E7A00E] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Categorias
CREATE TABLE [dbo].[Categorias] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [Descripcion] nvarchar(300) NULL,
    [Estado] bit NULL CONSTRAINT [DF__Categoria__Estad__276EDEB3] DEFAULT ((1)),
    [FechaRegistro] datetime2 NULL CONSTRAINT [DF__Categoria__Fecha__286302EC] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Categori__3214EC0727599415] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Clientes
CREATE TABLE [dbo].[Clientes] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [Nombres] nvarchar(120) NOT NULL,
    [Apellidos] nvarchar(120) NULL,
    [NitCi] nvarchar(30) NULL,
    [Telefono] nvarchar(30) NULL,
    [Correo] nvarchar(120) NULL,
    [Direccion] nvarchar(250) NULL,
    [FechaNacimiento] date NULL,
    [Observacion] nvarchar(500) NULL,
    [Estado] bit NULL CONSTRAINT [DF__Clientes__Estado__32E0915F] DEFAULT ((1)),
    [FechaRegistro] datetime2 NULL CONSTRAINT [DF__Clientes__FechaR__33D4B598] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Clientes__3214EC079D3B402A] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Compras
CREATE TABLE [dbo].[Compras] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdSucursal] int NOT NULL,
    [IdProveedor] int NOT NULL,
    [IdUsuario] int NOT NULL,
    [TipoDocumento] nvarchar(30) NOT NULL CONSTRAINT [DF__Compras__TipoDoc__656C112C] DEFAULT ('RECIBO'),
    [NumeroDocumento] nvarchar(50) NULL,
    [FechaCompra] datetime2 NOT NULL CONSTRAINT [DF__Compras__FechaCo__66603565] DEFAULT (getdate()),
    [SubTotal] decimal(18, 2) NOT NULL CONSTRAINT [DF__Compras__SubTota__6754599E] DEFAULT ((0)),
    [Descuento] decimal(18, 2) NOT NULL CONSTRAINT [DF__Compras__Descuen__68487DD7] DEFAULT ((0)),
    [Impuesto] decimal(18, 2) NOT NULL CONSTRAINT [DF__Compras__Impuest__693CA210] DEFAULT ((0)),
    [Total] decimal(18, 2) NOT NULL CONSTRAINT [DF__Compras__Total__6A30C649] DEFAULT ((0)),
    [Observacion] nvarchar(500) NULL,
    [EstadoCompra] nvarchar(20) NOT NULL CONSTRAINT [DF__Compras__EstadoC__6B24EA82] DEFAULT ('COMPLETADA'),
    [Estado] bit NOT NULL CONSTRAINT [DF__Compras__Estado__6C190EBB] DEFAULT ((1)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__Compras__FechaRe__6D0D32F4] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Compras__3214EC07EA7A56A6] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- DetalleCompras
CREATE TABLE [dbo].[DetalleCompras] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdCompra] int NOT NULL,
    [IdProducto] int NOT NULL,
    [IdLote] int NULL,
    [Cantidad] decimal(18, 3) NOT NULL,
    [PrecioCompra] decimal(18, 2) NOT NULL,
    [Descuento] decimal(18, 2) NOT NULL CONSTRAINT [DF__DetalleCo__Descu__72C60C4A] DEFAULT ((0)),
    [Impuesto] decimal(18, 2) NOT NULL CONSTRAINT [DF__DetalleCo__Impue__73BA3083] DEFAULT ((0)),
    [SubTotal] decimal(18, 2) NOT NULL,
    [Estado] bit NOT NULL CONSTRAINT [DF__DetalleCo__Estad__74AE54BC] DEFAULT ((1)),
    [Eliminado] bit NOT NULL CONSTRAINT [DF__DetalleCo__Elimi__75A278F5] DEFAULT ((0)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__DetalleCo__Fecha__76969D2E] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__DetalleC__3214EC07C5FF59B1] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- DetalleVentas
CREATE TABLE [dbo].[DetalleVentas] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdVenta] int NOT NULL,
    [IdProducto] int NOT NULL,
    [IdLote] int NULL,
    [Cantidad] decimal(18, 3) NOT NULL,
    [PrecioVenta] decimal(18, 2) NOT NULL,
    [Descuento] decimal(18, 2) NOT NULL CONSTRAINT [DF__DetalleVe__Descu__0C85DE4D] DEFAULT ((0)),
    [Impuesto] decimal(18, 2) NOT NULL CONSTRAINT [DF__DetalleVe__Impue__0D7A0286] DEFAULT ((0)),
    [SubTotal] decimal(18, 2) NOT NULL,
    [Estado] bit NOT NULL CONSTRAINT [DF__DetalleVe__Estad__0E6E26BF] DEFAULT ((1)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__DetalleVe__Fecha__0F624AF8] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__DetalleV__3214EC07C81B0BA6] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Gastos
CREATE TABLE [dbo].[Gastos] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdCaja] int NOT NULL,
    [IdUsuario] int NOT NULL,
    [Concepto] nvarchar(250) NOT NULL,
    [NumeroComprobante] nvarchar(50) NULL,
    [Monto] decimal(18, 2) NOT NULL,
    [Observacion] nvarchar(500) NULL,
    [Estado] bit NOT NULL CONSTRAINT [DF__Gastos__Estado__1AD3FDA4] DEFAULT ((1)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__Gastos__FechaReg__1BC821DD] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Gastos__3214EC076FBCACCA] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- ImagenesProductos
CREATE TABLE [dbo].[ImagenesProductos] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdProducto] int NOT NULL,
    [NombreArchivo] nvarchar(200) NULL,
    [RutaImagen] nvarchar(500) NOT NULL,
    [EsPrincipal] bit NULL CONSTRAINT [DF__ImagenesP__EsPri__571DF1D5] DEFAULT ((0)),
    [Estado] bit NULL CONSTRAINT [DF__ImagenesP__Estad__5812160E] DEFAULT ((1)),
    [FechaRegistro] datetime2 NULL CONSTRAINT [DF__ImagenesP__Fecha__59063A47] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Imagenes__3214EC0791D77EF0] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- InventarioSucursal
CREATE TABLE [dbo].[InventarioSucursal] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdSucursal] int NOT NULL,
    [IdProducto] int NOT NULL,
    [StockActual] decimal(18, 3) NOT NULL CONSTRAINT [DF__Inventari__Stock__49C3F6B7] DEFAULT ((0)),
    [StockMinimo] decimal(18, 3) NOT NULL CONSTRAINT [DF__Inventari__Stock__4AB81AF0] DEFAULT ((0)),
    [StockMaximo] decimal(18, 3) NULL,
    [PuntoReorden] decimal(18, 3) NULL,
    [Ubicacion] nvarchar(100) NULL,
    [Estado] bit NULL CONSTRAINT [DF__Inventari__Estad__4BAC3F29] DEFAULT ((1)),
    [FechaRegistro] datetime2 NULL CONSTRAINT [DF__Inventari__Fecha__4CA06362] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Inventar__3214EC0743D1EDF5] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_InventarioSucursal] UNIQUE NONCLUSTERED ([IdSucursal], [IdProducto])
);
GO

-- Lotes
CREATE TABLE [dbo].[Lotes] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdSucursal] int NOT NULL,
    [IdProducto] int NOT NULL,
    [NumeroLote] nvarchar(100) NOT NULL,
    [FechaFabricacion] date NULL,
    [FechaVencimiento] date NULL,
    [CantidadInicial] decimal(18, 3) NOT NULL,
    [CantidadDisponible] decimal(18, 3) NOT NULL,
    [CostoCompra] decimal(18, 2) NOT NULL,
    [Estado] bit NULL CONSTRAINT [DF__Lotes__Estado__5165187F] DEFAULT ((1)),
    [FechaRegistro] datetime2 NULL CONSTRAINT [DF__Lotes__FechaRegi__52593CB8] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Lotes__3214EC07509547ED] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Marcas
CREATE TABLE [dbo].[Marcas] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [Nombre] nvarchar(120) NOT NULL,
    [Descripcion] nvarchar(250) NULL,
    [Estado] bit NULL CONSTRAINT [DF__Marcas__Estado__2B3F6F97] DEFAULT ((1)),
    [FechaRegistro] datetime2 NULL CONSTRAINT [DF__Marcas__FechaReg__2C3393D0] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Marcas__3214EC070CE1EC9A] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Pagos
CREATE TABLE [dbo].[Pagos] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdVenta] int NOT NULL,
    [IdCaja] int NOT NULL,
    [MetodoPago] nvarchar(50) NOT NULL,
    [Monto] decimal(18, 2) NOT NULL,
    [Referencia] nvarchar(150) NULL,
    [Observacion] nvarchar(500) NULL,
    [Estado] bit NOT NULL CONSTRAINT [DF__Pagos__Estado__151B244E] DEFAULT ((1)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__Pagos__FechaRegi__160F4887] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Pagos__3214EC07899E8A64] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Productos
CREATE TABLE [dbo].[Productos] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdCategoria] int NOT NULL,
    [IdMarca] int NULL,
    [Codigo] nvarchar(50) NOT NULL,
    [CodigoBarras] nvarchar(100) NULL,
    [Nombre] nvarchar(200) NOT NULL,
    [NombreGenerico] nvarchar(200) NULL,
    [Descripcion] nvarchar(500) NULL,
    [PrecioCompra] decimal(18, 2) NOT NULL CONSTRAINT [DF__Productos__Preci__3D5E1FD2] DEFAULT ((0)),
    [PrecioVenta] decimal(18, 2) NOT NULL CONSTRAINT [DF__Productos__Preci__3E52440B] DEFAULT ((0)),
    [Peso] decimal(18, 3) NULL,
    [ControlaInventario] bit NOT NULL CONSTRAINT [DF__Productos__Contr__3F466844] DEFAULT ((1)),
    [ManejaLotes] bit NOT NULL CONSTRAINT [DF__Productos__Manej__403A8C7D] DEFAULT ((0)),
    [TieneVencimiento] bit NOT NULL CONSTRAINT [DF__Productos__Tiene__412EB0B6] DEFAULT ((0)),
    [PermiteVenta] bit NOT NULL CONSTRAINT [DF__Productos__Permi__4222D4EF] DEFAULT ((1)),
    [Estado] bit NOT NULL CONSTRAINT [DF__Productos__Estad__4316F928] DEFAULT ((1)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__Productos__Fecha__440B1D61] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Producto__3214EC07F339D8A9] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ__Producto__06370DACD49A49A1] UNIQUE NONCLUSTERED ([Codigo])
);
GO

-- Proveedores
CREATE TABLE [dbo].[Proveedores] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [Nombre] nvarchar(150) NOT NULL,
    [Contacto] nvarchar(120) NULL,
    [Nit] nvarchar(30) NULL,
    [Telefono] nvarchar(30) NULL,
    [Correo] nvarchar(120) NULL,
    [Direccion] nvarchar(250) NULL,
    [Observacion] nvarchar(500) NULL,
    [Estado] bit NULL CONSTRAINT [DF__Proveedor__Estad__2F10007B] DEFAULT ((1)),
    [FechaRegistro] datetime2 NULL CONSTRAINT [DF__Proveedor__Fecha__300424B4] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Proveedo__3214EC07E6654B1F] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Sucursales
CREATE TABLE [dbo].[Sucursales] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [Nombre] nvarchar(150) NOT NULL,
    [Direccion] nvarchar(250) NULL,
    [Telefono] nvarchar(30) NULL,
    [Correo] nvarchar(120) NULL,
    [Estado] bit NOT NULL CONSTRAINT [DF__Sucursale__Estad__239E4DCF] DEFAULT ((1)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__Sucursale__Fecha__24927208] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Sucursal__3214EC071F03B274] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Usuarios
CREATE TABLE [dbo].[Usuarios] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdSucursal] int NOT NULL,
    [Nombres] nvarchar(120) NOT NULL,
    [Apellidos] nvarchar(120) NULL,
    [Rol] varchar(50) NULL,
    [Usuario] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [Telefono] nvarchar(30) NULL,
    [Correo] nvarchar(150) NULL,
    [Foto] nvarchar(300) NULL,
    [Estado] bit NULL CONSTRAINT [DF__Usuarios__Estado__37A5467C] DEFAULT ((1)),
    [FechaRegistro] datetime2 NULL CONSTRAINT [DF__Usuarios__FechaR__38996AB5] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Usuarios__3214EC07A1DACFF1] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ__Usuarios__E3237CF719B1BED6] UNIQUE NONCLUSTERED ([Usuario])
);
GO

-- Ventas
CREATE TABLE [dbo].[Ventas] (
    [Id] int IDENTITY(1, 1) NOT NULL,
    [IdSucursal] int NOT NULL,
    [IdCliente] int NULL,
    [IdUsuario] int NOT NULL,
    [ACredito] bit NOT NULL CONSTRAINT [DF__Ventas__ACredito__7C4F7684] DEFAULT ((0)),
    [MontoPagado] decimal(18, 2) NOT NULL CONSTRAINT [DF__Ventas__MontoPag__7D439ABD] DEFAULT ((0)),
    [SaldoPendiente] decimal(18, 2) NOT NULL CONSTRAINT [DF__Ventas__SaldoPen__7E37BEF6] DEFAULT ((0)),
    [FechaLimitePago] date NULL,
    [FechaVenta] datetime2 NOT NULL CONSTRAINT [DF__Ventas__FechaVen__00200768] DEFAULT (getdate()),
    [Total] decimal(18, 2) NOT NULL CONSTRAINT [DF__Ventas__Total__03F0984C] DEFAULT ((0)),
    [Observacion] nvarchar(500) NULL,
    [EstadoVenta] nvarchar(20) NOT NULL CONSTRAINT [DF__Ventas__EstadoVe__04E4BC85] DEFAULT ('COMPLETADA'),
    [Estado] bit NOT NULL CONSTRAINT [DF__Ventas__Estado__05D8E0BE] DEFAULT ((1)),
    [FechaRegistro] datetime2 NOT NULL CONSTRAINT [DF__Ventas__FechaReg__06CD04F7] DEFAULT (getdate()),
    [FechaModificacion] datetime2 NULL,
    [FechaEliminacion] datetime2 NULL,
    CONSTRAINT [PK__Ventas__3214EC07AF67BCFA] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

/* =====================================================================
   3. LLAVES FORÁNEAS (27)
   ===================================================================== */

ALTER TABLE [dbo].[Caja] WITH CHECK
    ADD CONSTRAINT [FK_Caja_Sucursal] FOREIGN KEY ([IdSucursal])
    REFERENCES [dbo].[Sucursales] ([Id]);
GO

ALTER TABLE [dbo].[Caja] WITH CHECK
    ADD CONSTRAINT [FK_Caja_UsuarioApertura] FOREIGN KEY ([IdUsuarioApertura])
    REFERENCES [dbo].[Usuarios] ([Id]);
GO

ALTER TABLE [dbo].[Caja] WITH CHECK
    ADD CONSTRAINT [FK_Caja_UsuarioCierre] FOREIGN KEY ([IdUsuarioCierre])
    REFERENCES [dbo].[Usuarios] ([Id]);
GO

ALTER TABLE [dbo].[Compras] WITH CHECK
    ADD CONSTRAINT [FK_Compras_Proveedor] FOREIGN KEY ([IdProveedor])
    REFERENCES [dbo].[Proveedores] ([Id]);
GO

ALTER TABLE [dbo].[Compras] WITH CHECK
    ADD CONSTRAINT [FK_Compras_Sucursal] FOREIGN KEY ([IdSucursal])
    REFERENCES [dbo].[Sucursales] ([Id]);
GO

ALTER TABLE [dbo].[Compras] WITH CHECK
    ADD CONSTRAINT [FK_Compras_Usuario] FOREIGN KEY ([IdUsuario])
    REFERENCES [dbo].[Usuarios] ([Id]);
GO

ALTER TABLE [dbo].[DetalleCompras] WITH CHECK
    ADD CONSTRAINT [FK_DetalleCompras_Compra] FOREIGN KEY ([IdCompra])
    REFERENCES [dbo].[Compras] ([Id]);
GO

ALTER TABLE [dbo].[DetalleCompras] WITH CHECK
    ADD CONSTRAINT [FK_DetalleCompras_Lote] FOREIGN KEY ([IdLote])
    REFERENCES [dbo].[Lotes] ([Id]);
GO

ALTER TABLE [dbo].[DetalleCompras] WITH CHECK
    ADD CONSTRAINT [FK_DetalleCompras_Producto] FOREIGN KEY ([IdProducto])
    REFERENCES [dbo].[Productos] ([Id]);
GO

ALTER TABLE [dbo].[DetalleVentas] WITH CHECK
    ADD CONSTRAINT [FK_DetalleVentas_Lote] FOREIGN KEY ([IdLote])
    REFERENCES [dbo].[Lotes] ([Id]);
GO

ALTER TABLE [dbo].[DetalleVentas] WITH CHECK
    ADD CONSTRAINT [FK_DetalleVentas_Producto] FOREIGN KEY ([IdProducto])
    REFERENCES [dbo].[Productos] ([Id]);
GO

ALTER TABLE [dbo].[DetalleVentas] WITH CHECK
    ADD CONSTRAINT [FK_DetalleVentas_Venta] FOREIGN KEY ([IdVenta])
    REFERENCES [dbo].[Ventas] ([Id]);
GO

ALTER TABLE [dbo].[Gastos] WITH CHECK
    ADD CONSTRAINT [FK_Gastos_Caja] FOREIGN KEY ([IdCaja])
    REFERENCES [dbo].[Caja] ([Id]);
GO

ALTER TABLE [dbo].[Gastos] WITH CHECK
    ADD CONSTRAINT [FK_Gastos_Usuario] FOREIGN KEY ([IdUsuario])
    REFERENCES [dbo].[Usuarios] ([Id]);
GO

ALTER TABLE [dbo].[ImagenesProductos] WITH CHECK
    ADD CONSTRAINT [FK_ImagenesProductos_Producto] FOREIGN KEY ([IdProducto])
    REFERENCES [dbo].[Productos] ([Id]);
GO

ALTER TABLE [dbo].[InventarioSucursal] WITH CHECK
    ADD CONSTRAINT [FK_InventarioSucursal_Producto] FOREIGN KEY ([IdProducto])
    REFERENCES [dbo].[Productos] ([Id]);
GO

ALTER TABLE [dbo].[InventarioSucursal] WITH CHECK
    ADD CONSTRAINT [FK_InventarioSucursal_Sucursal] FOREIGN KEY ([IdSucursal])
    REFERENCES [dbo].[Sucursales] ([Id]);
GO

ALTER TABLE [dbo].[Lotes] WITH CHECK
    ADD CONSTRAINT [FK_Lotes_Producto] FOREIGN KEY ([IdProducto])
    REFERENCES [dbo].[Productos] ([Id]);
GO

ALTER TABLE [dbo].[Lotes] WITH CHECK
    ADD CONSTRAINT [FK_Lotes_Sucursal] FOREIGN KEY ([IdSucursal])
    REFERENCES [dbo].[Sucursales] ([Id]);
GO

ALTER TABLE [dbo].[Pagos] WITH CHECK
    ADD CONSTRAINT [FK_Pagos_Caja] FOREIGN KEY ([IdCaja])
    REFERENCES [dbo].[Caja] ([Id]);
GO

ALTER TABLE [dbo].[Pagos] WITH CHECK
    ADD CONSTRAINT [FK_Pagos_Venta] FOREIGN KEY ([IdVenta])
    REFERENCES [dbo].[Ventas] ([Id]);
GO

ALTER TABLE [dbo].[Productos] WITH CHECK
    ADD CONSTRAINT [FK_Productos_Categorias] FOREIGN KEY ([IdCategoria])
    REFERENCES [dbo].[Categorias] ([Id]);
GO

ALTER TABLE [dbo].[Productos] WITH CHECK
    ADD CONSTRAINT [FK_Productos_Marcas] FOREIGN KEY ([IdMarca])
    REFERENCES [dbo].[Marcas] ([Id]);
GO

ALTER TABLE [dbo].[Usuarios] WITH CHECK
    ADD CONSTRAINT [FK_Usuarios_Sucursales] FOREIGN KEY ([IdSucursal])
    REFERENCES [dbo].[Sucursales] ([Id]);
GO

ALTER TABLE [dbo].[Ventas] WITH CHECK
    ADD CONSTRAINT [FK_Ventas_Cliente] FOREIGN KEY ([IdCliente])
    REFERENCES [dbo].[Clientes] ([Id]);
GO

ALTER TABLE [dbo].[Ventas] WITH CHECK
    ADD CONSTRAINT [FK_Ventas_Sucursal] FOREIGN KEY ([IdSucursal])
    REFERENCES [dbo].[Sucursales] ([Id]);
GO

ALTER TABLE [dbo].[Ventas] WITH CHECK
    ADD CONSTRAINT [FK_Ventas_Usuario] FOREIGN KEY ([IdUsuario])
    REFERENCES [dbo].[Usuarios] ([Id]);
GO

PRINT 'Esquema creado: 17 tablas y 27 llaves foráneas.';
GO
