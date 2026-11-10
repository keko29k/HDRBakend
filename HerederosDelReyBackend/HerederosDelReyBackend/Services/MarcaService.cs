using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Exceptions;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class MarcaService : IMarcaService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MarcaService(IUnitOfWork unitOfWork , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MarcaDto>> GetAllAsync()
        {
            var marcas = await _unitOfWork.Marca.GetAllAsync();
            return marcas.Select(m => new MarcaDto
            {
                Id = m.Id,
                Nombre = m.Nombre
            });
        }

        public async Task<MarcaDto?> GetByIdAsync(int id)
        {
            var marca = await _unitOfWork.Marca.GetByIdAsync(id);
            if (marca == null) return null;
            return new MarcaDto
            {
                Id = marca.Id,
                Nombre = marca.Nombre
            };
        }

        public async Task<MarcaDto> AddAsync(MarcaCreateDto dto)
        {
            await ValidarNombreUnicoAsync(dto.Nombre, null);

            var marca = new Marca
            {
                Nombre = dto.Nombre.Trim()
            };
            await _unitOfWork.Marca.AddAsync(marca);
            await _unitOfWork.SaveChangesAsync();
            return new MarcaDto
            {
                Id = marca.Id,
                Nombre = marca.Nombre
            };
        }

        public async Task<bool> UpdateAsync(int id, MarcaUpdateDto dto)
        {
            var marca = await _unitOfWork.Marca.GetByIdAsync(id);
            if (marca == null) return false;
            await ValidarNombreUnicoAsync(dto.Nombre, id);
            marca.Nombre = dto.Nombre.Trim();
            _unitOfWork.Marca.Update(marca);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // No se permiten dos marcas activas con el mismo nombre.
        private async Task ValidarNombreUnicoAsync(string? nombre, int? idActual)
        {
            var valor = nombre?.Trim();
            if (string.IsNullOrEmpty(valor))
                return;

            var existe = await _unitOfWork.Marca
                .GetAllAsQueryable()
                .AnyAsync(m => m.Nombre == valor && (idActual == null || m.Id != idActual));

            if (existe)
                throw new DatoDuplicadoException($"Ya existe una marca con el nombre \"{valor}\".");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var marca = await _unitOfWork.Marca.GetByIdAsync(id);
            if (marca == null) return false;
            await _unitOfWork.Marca.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<ApiResponse<IEnumerable<MarcaDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var marcas = await _unitOfWork.Marca.GetAllAsync(filter);
            var marcasDto = _mapper.Map<IEnumerable<MarcaDto>>(marcas);

            return new ApiResponse<IEnumerable<MarcaDto>>(marcasDto, marcas.MetaData);
        }


    }
}