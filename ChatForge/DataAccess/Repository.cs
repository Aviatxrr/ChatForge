using System.Linq.Expressions;
using ChatForge.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatForge.DataAccess;


public interface IRepository<T>
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void AddRange(ICollection<T> entities);
    void Update(T entity);
    void Delete(T entity);
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="T">Class name of Entity that this repository will correspond with MySQL table of 'T'</typeparam>
public class Repository<T> : IRepository<T> where T : class, IModel
    {
        private readonly ChatForgeContext _context;
        private readonly DbSet<T> _dbSet;

        /// <summary>
        /// Create a Repository for CRUD. 
        /// </summary>
        /// <param name="context">The DB context to use. e.g. 'new ChatForgeContext()'</param>
        public Repository(ChatForgeContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Get a specific Entity by its numeric id
        /// </summary>
        /// <param name="id">The numeric ID of the primary key associated with the MySQL table</param>
        /// <returns>The entity associated with 'id' of type 'T'</returns>
        /// <exception cref="ApplicationException"></exception>
        public T GetById(int id)
        {
            try
            {
                return _dbSet.First(d => d.Id == id);
            }
            catch (InvalidOperationException)
            {
                throw new ApplicationException($"{typeof(T).Name} with ID: {id} not found");
            }
        }
        /// <summary>
        /// Get all Entities of type 'T'.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAll()
        {
            return _dbSet;
        }
        
        /// <summary>
        /// Add 'entity' to the database
        /// </summary>
        /// <param name="entity">the entity to add.</param>
        public void Add(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
        }
        /// <summary>
        /// Add a range of entities to the database
        /// </summary>
        /// <param name="entities">the list of entities to add</param>
        public void AddRange(ICollection<T> entites)
        {
            foreach (T entity in entites)
            {
                Add(entity);
            }
        }
        /// <summary>
        /// update a particular entity
        /// </summary>
        /// <param name="entity">the entity to update.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _dbSet.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }
        /// <summary>
        /// Delete a particular entity
        /// </summary>
        /// <param name="entity">the entity to delete</param>
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
        }
        
    }