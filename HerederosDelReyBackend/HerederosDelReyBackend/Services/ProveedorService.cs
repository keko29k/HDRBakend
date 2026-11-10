using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Exceptions;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using HerederosDelReyBackend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProveedorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProveedoresDto> AddAsync(ProveedoresCreateDto dto)
        {
            await ValidarDuplicadosAsync(dto.Nombre, dto.Nit, null);

            var Objeto = _mapper.Map<Proveedore>(dto);
            Objeto.Nombre = Objeto.Nombre.Trim();
            Objeto.Nit = string.IsNullOrWhiteSpace(Objeto.Nit) ? null : Objeto.Nit.Trim();

            await _unitOfWork.Proveedores.AddAsync(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProveedoresDto>(Objeto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var objeto = await _unitOfWork.Proveedores.GetByIdAsync(id);
            if (objeto == null)
                return false;

            await _unitOfWork.Proveedores.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProveedoresDto>> GetAllAsync()
        {
            var lista = await _unitOfWork.Proveedores.GetAllAsync();
            return _mapper.Map<IEnumerable<ProveedoresDto>>(lista);
        }

        public async Task<ProveedoresDto?> GetByIdAsync(int id)
        {
            var objeto = await _unitOfWork.Proveedores.GetByIdAsync(id);
            if (objeto == null)
                return null;

            return _mapper.Map<ProveedoresDto>(objeto);
        }

        public async Task<bool> UpdateAsync(int id, ProveedoresUptadeDto dto)
        {
            if (id == null)
                return false;

            var proveedor = _unitOfWork.Proveedores.GetByIdAsync(id).Result;
            if (proveedor == null)
                return false;

            // Hoy la edición solo guarda el nombre, así que solo se valida el nombre.
            await ValidarDuplicadosAsync(dto.Nombre, null, id);

            proveedor.Nombre = dto.Nombre.Trim();



            _unitOfWork.Proveedores.Update(proveedor);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // No se permiten dos proveedores activos con el mismo nombre o el mismo NIT.
        private async Task ValidarDuplicadosAsync(string? nombre, string? nit, int? idActual)
        {
            var activos = _unitOfWork.Proveedores
                .GetAllAsQueryable()
                .Where(p => idActual == null || p.Id != idActual);

            var nombreLimpio = nombre?.Trim();
            if (!string.IsNullOrEmpty(nombreLimpio) &&
                await activos.AnyAsync(p => p.Nombre == nombreLimpio))
            {
                throw new DatoDuplicadoException($"Ya existe un proveedor con el nombre \"{nombreLimpio}\".");
            }

            var nitLimpio = nit?.Trim();
            if (!string.IsNullOrEmpty(nitLimpio) &&
                await activos.AnyAsync(p => p.Nit == nitLimpio))
            {
                throw new DatoDuplicadoException($"Ya existe un proveedor con el NIT {nitLimpio}.");
            }
        }
        public async Task<ApiResponse<IEnumerable<ProveedoresDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var objeto = await _unitOfWork.Proveedores.GetAllAsync(filter);
            var objetoDto = _mapper.Map<IEnumerable<ProveedoresDto>>(objeto);

            return new ApiResponse<IEnumerable<ProveedoresDto>>(objetoDto, objeto.MetaData);
        }
    }
}
