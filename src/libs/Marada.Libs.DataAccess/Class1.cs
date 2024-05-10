using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace Marada.Libs.DataAccess
{
    public class DataAccessException: Exception
    {
        public DataAccessException() { }
        public DataAccessException(string message) : base(message) { }
        public DataAccessException(string message, Exception inner) : base(message, inner) { }
        public override string Message
            => InnerException != null ? $"{Message} ({InnerException.Message})" : Message;
    }

    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        void Insert(T obj);
        void Update(T obj);
        void Delete(int id);
    }

    public class Repository<T>: IRepository<T> where T : class
    {
        private readonly DbContext context;
        private readonly DbSet<T> dbSet;

        /// <summary>
        /// Creates a new instance of a Repository, with the provided 
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Repository(DbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            ThrowOnCannotConnect();
            this.dbSet = context.Set<T>();
        }

        protected bool CanConnect()
            => context.Database is not null && context.Database.CanConnect();

        protected void ThrowOnCannotConnect()
        {
            if(!CanConnect()) 
                throw new DataAccessException("Cannot use DbContext to connect to database");
        }

        public IEnumerable<T> GetAll()
        {
            ThrowOnCannotConnect();

            return dbSet.ToList();
        }


        public void Insert(T obj)
        {
            dbSet.Add(obj);
        }

        public void Update(T obj)
        {
            dbSet.Attach(obj);
            context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            T existing = dbSet.Find(id);
            dbSet.Remove(existing);
        }

        //public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        //{
        //    return Context.Set<TEntity>().Where(predicate);
        //}

        public T GetById(int id)
        {
            if(id <= 0)
            {
                throw new ArgumentException("Invalid ID", nameof(id));
            }

            var entity = dbSet.Find(id);
            if(entity == null)
            {
                throw new DataAccessException($"Entity with ID {id} not found");
            }

            return entity;
        }
    }

    public class UnitOfWork: IUnitOfWork
    {
        private readonly DbContext context;
        private bool disposed = false;

        public UnitOfWork(DbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IRepository<T> Repository<T>() where T : class
        {
            return new Repository<T>(context);
        }

        public void Save()
        {
            try
            {
                context.SaveChanges();
            }
            catch(Exception ex)
            {
                throw new DataAccessException("Error saving changes", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IUnitOfWork: IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        void Save();
    }
}