using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Exceptions;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

public class ClienteService : IClienteService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ClienteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    public async Task<ClienteDto> AddAsync(ClienteCreateDto dto)
    {
        await ValidarNitUnicoAsync(dto.NitCi, null);

        var Objeto = _mapper.Map<Cliente>(dto);
        Objeto.NitCi = string.IsNullOrWhiteSpace(Objeto.NitCi) ? null : Objeto.NitCi.Trim();

        await _unitOfWork.Clientes.AddAsync(Objeto);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ClienteDto>(Objeto);
    }

    public async Task<bool> DeleteAsync(int id)
    {

        var objeto = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (objeto == null)
            return false;

        await _unitOfWork.Clientes.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ClienteDto>> GetAllAsync()
    {
        var lista = await _unitOfWork.Clientes.GetAllAsync();
        return _mapper.Map<IEnumerable<ClienteDto>>(lista);
    }

    public async Task<ClienteDto?> GetByIdAsync(int id)
    {
        var objeto = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (objeto == null)
            return null;

        return _mapper.Map<ClienteDto>(objeto);
    }

    public async Task<bool> UpdateAsync(int id, ClienteUpdateDto dto)
    {
        if (id == null)
            return false;

        var Cliente = _unitOfWork.Clientes.GetByIdAsync(id).Result;
        if (Cliente == null)
            return false;

        await ValidarNitUnicoAsync(dto.NitCi, id);

        Cliente.Nombres = dto.Nombres;
        Cliente.Apellidos = dto.Apellidos;
        Cliente.NitCi = string.IsNullOrWhiteSpace(dto.NitCi) ? null : dto.NitCi.Trim();
        Cliente.Telefono = dto.Telefono;
        Cliente.Correo = dto.Correo;
        Cliente.Direccion = dto.Direccion;
        Cliente.FechaNacimiento = dto.FechaNacimiento;
        Cliente.Observacion = dto.Observacion;  

        _unitOfWork.Clientes.Update(Cliente);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // No se permiten dos clientes activos con el mismo NIT/CI.
    // "0" se acepta repetido porque en Bolivia se usa para "sin NIT".
    private async Task ValidarNitUnicoAsync(string? nitCi, int? idActual)
    {
        var valor = nitCi?.Trim();
        if (string.IsNullOrEmpty(valor) || valor == "0")
            return;

        var existe = await _unitOfWork.Clientes
            .GetAllAsQueryable()
            .AnyAsync(c => c.NitCi == valor && (idActual == null || c.Id != idActual));

        if (existe)
            throw new DatoDuplicadoException($"Ya existe un cliente con el NIT/CI {valor}.");
    }

    public async Task<ApiResponse<IEnumerable<ClienteDto>>> GetAllAsync(PostQueryFilter filter)
    {
        var objeto = await _unitOfWork.Clientes.GetAllAsync(filter);
        var objetoDto = _mapper.Map<IEnumerable<ClienteDto>>(objeto);

        return new ApiResponse<IEnumerable<ClienteDto>>(objetoDto, objeto.MetaData);
    }
}