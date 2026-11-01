
using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class CajaService : ICajaService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CajaService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // APERTURA DE CAJA
        public async Task<CajaDto> AbrirCajaAsync(
            CajaCreateDto dto,
            int idUsuario,
            int idSucursal)
        {
            if (dto == null)
                throw new ArgumentException("Datos de caja inválidos");

            if (idUsuario <= 0 || idSucursal <= 0)
                throw new ArgumentException("Usuario o sucursal inválidos");

            if (dto.MontoInicial < 0)
                throw new ArgumentException("El monto inicial no puede ser negativo");

            // Verificar si el usuario ya tiene una caja abierta
            var cajaExistente = await _unitOfWork.Caja
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(c =>
                    c.IdUsuarioApertura == idUsuario &&
                    c.IdSucursal == idSucursal &&
                    c.EstadoCaja == "ABIERTA"
                );

            if (cajaExistente != null)
                throw new ArgumentException(
                    "Ya tienes una caja abierta en esta sucursal. Debes cerrarla antes de abrir otra."
                );

            // Crear caja con datos del usuario autenticado
            var caja = new Caja
            {
                IdSucursal = idSucursal,
                IdUsuarioApertura = idUsuario,
                IdUsuarioCierre = null,
                FechaApertura = DateTime.Now,
                FechaCierre = null,
                MontoInicial = dto.MontoInicial,
                MontoFinal = null,
                EstadoCaja = "ABIERTA",
                Observacion = dto.Observacion
            };

            await _unitOfWork.Caja.AddAsync(caja);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CajaDto>(caja);
        }

        // OBTENER CAJA ABIERTA DEL USUARIO AUTENTICADO
        public async Task<CajaDto?> ObtenerMiCajaAbiertaAsync(
            int idUsuario,
            int idSucursal)
        {
            var caja = await _unitOfWork.Caja
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(c =>
                    c.IdUsuarioApertura == idUsuario &&
                    c.IdSucursal == idSucursal &&
                    c.EstadoCaja == "ABIERTA"
                );

            if (caja == null)
                return null;

            return _mapper.Map<CajaDto>(caja);
        }

        // MÉTODOS EXISTENTES
        public async Task<CajaDto> AddAsync(CajaCreateDto dto)
        {
            var objeto = _mapper.Map<Caja>(dto);

            objeto.FechaApertura = DateTime.Now;

            await _unitOfWork.Caja.AddAsync(objeto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CajaDto>(objeto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var objeto = await _unitOfWork.Caja.GetByIdAsync(id);

            if (objeto == null)
                return false;

            await _unitOfWork.Caja.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CajaDto>> GetAllAsync()
        {
            var lista = await _unitOfWork.Caja.GetAllAsync();

            return _mapper.Map<IEnumerable<CajaDto>>(lista);
        }

        public async Task<CajaDto?> GetByIdAsync(int id)
        {
            var objeto = await _unitOfWork.Caja.GetByIdAsync(id);

            if (objeto == null)
                return null;

            return _mapper.Map<CajaDto>(objeto);
        }

        public async Task<bool> UpdateAsync(
            int id,
            CajaUpdateDto dto)
        {
            var objeto = await _unitOfWork.Caja.GetByIdAsync(id);

            if (objeto == null)
                return false;

            objeto.MontoInicial = dto.MontoInicial;
            objeto.MontoFinal = dto.MontoFinal;
            objeto.FechaApertura = dto.FechaApertura;
            objeto.FechaCierre = dto.FechaCierre;
            objeto.EstadoCaja = dto.EstadoCaja;

            _unitOfWork.Caja.Update(objeto);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<ApiResponse<IEnumerable<CajaDto>>> GetAllAsync(
            PostQueryFilter filter)
        {
            var objetos = await _unitOfWork.Caja.GetAllAsync(filter);

            var objetosDto = _mapper.Map<IEnumerable<CajaDto>>(objetos);

            return new ApiResponse<IEnumerable<CajaDto>>(
                objetosDto,
                objetos.MetaData
            );
        }

        public async Task<bool> CerrarCajaAsync(int id)
        {
            var caja = await _unitOfWork.Caja.GetByIdAsync(id);

            if (caja == null)
                return false;

            if (caja.EstadoCaja == "CERRADA")
                return false;

            var totalVentas = _unitOfWork.Ventas
                .GetAllAsQueryable()
                .Where(v => v.FechaRegistro >= caja.FechaApertura)
                .Sum(v => (decimal?)v.Total) ?? 0;

            var totalGastos = _unitOfWork.Gastos
                .GetAllAsQueryable()
                .Where(g => g.IdCaja == id)
                .Sum(g => (decimal?)g.Monto) ?? 0;

            caja.MontoFinal = caja.MontoInicial + totalVentas - totalGastos;
            caja.FechaCierre = DateTime.Now;
            caja.EstadoCaja = "CERRADA";

            _unitOfWork.Caja.Update(caja);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
