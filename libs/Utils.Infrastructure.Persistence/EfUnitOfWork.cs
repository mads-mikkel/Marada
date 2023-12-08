using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Marada.Utils.Infrastructure.Persistence
{
    public interface IUnitOfWork
    {
        void Commit();
        Task CommitAsync();
    }

    public class UnitOfWork: IUnitOfWork
    {
        #region Fields
        private readonly DbContext dbContext;
        #endregion

        #region Contstructors
        public UnitOfWork(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        #endregion

        #region Methods
        public virtual void Commit()
        {

            dbContext.SaveChanges();
        }

        public virtual async Task CommitAsync()
        {
            await dbContext.SaveChangesAsync();
        }
        #endregion       
    }

    public interface IRepository<T>
    {
        T GetBy(object id);
        Task<T> GetByAsync(object id);

        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();

        void Add(T entity);
        Task AddAsync(T entity);
    }

    public abstract class Repository<T>: IRepository<T> where T : class
    {
        protected readonly DbSet<T> set;

        public Repository(DbSet<T> set)
        {
            this.set = set;
        }

        public IQueryable<T> Get() => set;

        public IEnumerable<T> GetAll()
        {
            try
            {
                return set.ToList();
            }
            catch(Exception e)
            {
                throw new EfUnitOfWorkException($"Unable to get all {nameof(T)}s: {e.Message}", e);
            }
        }

        public virtual IEnumerable<T> Get(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<T> query = set;

            if(filter != null)
            {
                query = query.Where(filter);
            }

            foreach(var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if(orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public T GetBy(object id)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetByAsync(object id)
        {
            throw new NotImplementedException();
        }

        public void Save(T entity)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(T entity)
        {
            throw new NotImplementedException();
        }
    }
}