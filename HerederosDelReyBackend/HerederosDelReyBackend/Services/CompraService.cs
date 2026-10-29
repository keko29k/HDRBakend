using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.Services
{
    public class CompraService : ICompraService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CompraService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CompraDto> AddAsync(CompraCreateDto dto)
        {
            var compra = _mapper.Map<Compra>(dto);
            await _unitOfWork.Compra.AddAsync(compra);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CompraDto>(compra);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (await _unitOfWork.Compra.GetByIdAsync(id) == null)
                return false;

            await _unitOfWork.Compra.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CompraDto>> GetAllAsync()
        {
            var compras = await _unitOfWork.Compra.GetAllAsync();
            return _mapper.Map<IEnumerable<CompraDto>>(compras);
        }

        public async Task<CompraDto?> GetByIdAsync(int id)
        {
            var compra = await _unitOfWork.Compra.GetByIdAsync(id);
            return compra == null ? null : _mapper.Map<CompraDto>(compra);
        }

        public async Task<bool> UpdateAsync(int id, CompraUpdateDto dto)
        {
            var compra = await _unitOfWork.Compra.GetByIdAsync(id);
            if (compra == null)
                return false;

            compra.TipoDocumento = dto.TipoDocumento;
            compra.NumeroDocumento = dto.NumeroDocumento;
            compra.Descuento = dto.Descuento;
            compra.Observacion = dto.Observacion;
            compra.EstadoCompra = dto.EstadoCompra;

            _unitOfWork.Compra.Update(compra);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ApiResponse<IEnumerable<CompraDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var compras = await _unitOfWork.Compra.GetAllAsync(filter);
            var comprasDto = _mapper.Map<IEnumerable<CompraDto>>(compras);
            return new ApiResponse<IEnumerable<CompraDto>>(comprasDto, compras.MetaData);
        }
    }
}
