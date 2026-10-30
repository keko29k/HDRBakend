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
    public class InventarioSucursalService : IInventarioSucursalService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;


        public InventarioSucursalService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InventarioSucursalDto>> GetAllAsync()
        {
            var lista = await _unitOfWork.InventarioSucursal.GetAllAsync();
            return _mapper.Map<IEnumerable<InventarioSucursalDto>>(lista);
        }


        public async Task<InventarioSucursalDto> AddAsync(InventarioSucursalCreateDto dto)
        {
            var Objeto = _mapper.Map<InventarioSucursal>(dto);

            await _unitOfWork.InventarioSucursal.AddAsync(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<InventarioSucursalDto>(Objeto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var objeto = await _unitOfWork.InventarioSucursal.GetByIdAsync(id);
            if (objeto == null)
                return false;

            await _unitOfWork.InventarioSucursal.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }



        public async Task<InventarioSucursalDto?> GetByIdAsync(int id)
        {
            var objeto = await _unitOfWork.InventarioSucursal.GetByIdAsync(id);
            if (objeto == null)
                return null;

            return _mapper.Map<InventarioSucursalDto>(objeto);
        }

        public async Task<bool> UpdateAsync(int id, InventarioSucursalUpdateDto dto)
        {
            if (id == null)
                return false;

            var Objeto = _unitOfWork.InventarioSucursal.GetByIdAsync(id).Result;
            if (Objeto == null)
                return false;

            Objeto.IdProducto = dto.IdProducto;
            Objeto.IdSucursal = dto.IdSucursal;
            Objeto.StockActual = dto.StockActual;
            Objeto.StockMinimo = dto.StockMinimo;
            Objeto.StockMaximo = dto.StockMaximo;
            Objeto.PuntoReorden = dto.PuntoReorden;
            Objeto.Ubicacion = dto.Ubicacion;


            _unitOfWork.InventarioSucursal.Update(Objeto);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ApiResponse<IEnumerable<InventarioSucursalDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var inventarioSucursal = await _unitOfWork.InventarioSucursal.GetAllAsync(filter);
            var inventarioSucursalDto = _mapper.Map<IEnumerable<InventarioSucursalDto>>(inventarioSucursal);

            return new ApiResponse<IEnumerable<InventarioSucursalDto>>(inventarioSucursalDto, inventarioSucursal.MetaData);
        }

    }
}
