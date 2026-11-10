using HerederosDelReyBackend.Models;


namespace HerederosDelReyBackend.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        IQueryable<T> GetAllAsQueryable();

        /// <summary>Incluye también los registros eliminados (Estado = true).</summary>
        IQueryable<T> GetAllIncluyendoEliminados();
        Task DeleteAsync(int id);
    }
}
