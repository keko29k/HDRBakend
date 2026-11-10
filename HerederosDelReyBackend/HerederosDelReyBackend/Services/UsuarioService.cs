using AutoMapper;
using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Exceptions;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;

        public UsuarioService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
        }

        public async Task<UsuarioDto?> GetByIdAsync(int id)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
                return null;

            return _mapper.Map<UsuarioDto>(usuario);
        }

        public async Task<UsuarioDto> AddAsync(UsuarioCreateDto dto)
        {
            await ValidarDuplicadosAsync(dto.Usuario1, dto.Correo, null);

            var usuario = _mapper.Map<Usuario>(dto);
            usuario.Usuario1 = usuario.Usuario1.Trim();
            usuario.Correo = string.IsNullOrWhiteSpace(usuario.Correo) ? null : usuario.Correo.Trim();

            usuario.PasswordHash = _passwordHasher.HashPassword(dto.Clave);

            await _unitOfWork.Usuarios.AddAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UsuarioDto>(usuario);
        }

        public async Task<bool> UpdateAsync(int id, UsuarioUpdateDto dto)
        {
            if (id != dto.IdSucursal)
                return false;

            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
                return false;

            await ValidarDuplicadosAsync(dto.Usuario1, dto.Correo, id);

            usuario.Usuario1 = dto.Usuario1.Trim();
            usuario.Correo = string.IsNullOrWhiteSpace(dto.Correo) ? null : dto.Correo.Trim();
            usuario.Rol = dto.Rol;

            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
            {
                usuario.PasswordHash = _passwordHasher.HashPassword(dto.PasswordHash);
            }

            _unitOfWork.Usuarios.Update(usuario);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null)
                return false;

            await _unitOfWork.Usuarios.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // El nombre de usuario es único en la base de datos (incluye usuarios eliminados)
        // y el correo no puede repetirse entre usuarios activos.
        private async Task ValidarDuplicadosAsync(string? nombreUsuario, string? correo, int? idActual)
        {
            var usuarioLimpio = nombreUsuario?.Trim();
            if (!string.IsNullOrEmpty(usuarioLimpio))
            {
                var conUsuario = await _unitOfWork.Usuarios
                    .GetAllIncluyendoEliminados()
                    .Where(u => u.Usuario1 == usuarioLimpio && (idActual == null || u.Id != idActual))
                    .Select(u => new { u.Estado })
                    .FirstOrDefaultAsync();

                if (conUsuario != null)
                {
                    throw new DatoDuplicadoException(conUsuario.Estado
                        ? $"El usuario \"{usuarioLimpio}\" pertenece a una cuenta eliminada. Elige otro nombre de usuario."
                        : $"Ya existe el usuario \"{usuarioLimpio}\".");
                }
            }

            var correoLimpio = correo?.Trim();
            if (!string.IsNullOrEmpty(correoLimpio))
            {
                var existe = await _unitOfWork.Usuarios
                    .GetAllAsQueryable()
                    .AnyAsync(u => u.Correo == correoLimpio && (idActual == null || u.Id != idActual));

                if (existe)
                    throw new DatoDuplicadoException($"Ya existe un usuario con el correo {correoLimpio}.");
            }
        }

        public async Task<ApiResponse<IEnumerable<UsuarioDto>>> GetAllAsync(PostQueryFilter filter)
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync(filter);
            var usuariosDto = _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);

            return new ApiResponse<IEnumerable<UsuarioDto>>(usuariosDto, usuarios.MetaData);
        }

    }

}
