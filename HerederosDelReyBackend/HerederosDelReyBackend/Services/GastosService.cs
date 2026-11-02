using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.Services
{
    public class GastosService : IGastosService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

       
        public GastosService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GastosDto>> GetAllAsync()
        {
            var lista = await _unitOfWork.Gastos.GetAllAsync();
            return _mapper.Map<IEnumerable<GastosDto>>(lista);
        }

        
        public async Task<GastosDto> AddAsync(GastosCreateDto dto)
        {
            var Objeto = _mapper.Map<Gasto>(dto);

            await _unitOfWork.Gastos.AddAsync(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<GastosDto>(Objeto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var objeto = await _unitOfWork.Gastos.GetByIdAsync(id);
            if (objeto == null)
                return false;

            await _unitOfWork.Gastos.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }



        public async Task<GastosDto?> GetByIdAsync(int id)
        {
            var objeto = await _unitOfWork.Gastos.GetByIdAsync(id);
            if (objeto == null)
                return null;

            return _mapper.Map<GastosDto>(objeto);
        }

        public async Task<bool> UpdateAsync(int id, GastosUpdateDto dto)
        {
            if (id == null)
                return false;

            var Objeto = _unitOfWork.Gastos.GetByIdAsync(id).Result;
            if (Objeto == null)
                return false;

            Objeto.Concepto = dto.Concepto;
            Objeto.NumeroComprobante = dto.NumeroComprobante;
            Objeto.Monto = dto.Monto;
            Objeto.Observacion = dto.Observacion;



            _unitOfWork.Gastos.Update(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ApiResponse<IEnumerable<GastosDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var gastos = await _unitOfWork.Gastos.GetAllAsync(filter);
            var gastosDto = _mapper.Map<IEnumerable<GastosDto>>(gastos);

            return new ApiResponse<IEnumerable<GastosDto>>(gastosDto, gastos.MetaData);
        }

      
    }
}
