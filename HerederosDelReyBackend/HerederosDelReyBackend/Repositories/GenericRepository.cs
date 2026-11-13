using HerederosDelReyBackend.Data;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace HerederosDelReyBackend.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly HerederosDelReyContext _context;
        protected readonly DbSet<T> _entities;

        public GenericRepository(HerederosDelReyContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _entities
                .AsNoTracking()
                .Where(x => x.Estado == false)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _entities
                .FirstOrDefaultAsync(x => x.Id == id && x.Estado == false);
        }

        public async Task AddAsync(T entity)
        {
            entity.FechaRegistro = DateTime.Now;
            entity.Estado = false;

            await _entities.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _entities.Update(entity);
        }
        


        public async Task DeleteAsync(int id)
        {
            var entity = await _entities.FirstOrDefaultAsync(x => x.Id == id && x.Estado ==
false);

            if (entity != null)
            {
                entity.Estado = true;
                _entities.Update(entity);
            }

        }
        public IQueryable<T> GetAllAsQueryable()
        {
            return _entities
                .AsNoTracking()
                .Where(x => x.Estado == false)
                .OrderByDescending(x => x.Id)
                .AsQueryable();
        }

        public IQueryable<T> GetAllIncluyendoEliminados()
        {
            return _entities.AsNoTracking();
        }

    }
}
