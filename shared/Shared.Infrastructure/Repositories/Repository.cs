using Microsoft.EntityFrameworkCore;
using Shared.Core.Repositories;
using Shared.Infrastructure.Entities;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Repositories;

/// <summary>
/// A generic repository for managing entities.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TId"></typeparam>
public class Repository<TEntity, TId> : SaveContext, IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
{
    private readonly IBaseDbContext _context;

    public Repository(IBaseDbContext context) : base(context)
    {
        _context = context;
    }
    ///<inheritdoc/>
    public virtual async Task<bool> AddAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
        return await _context.SaveChangesAsync() > 0;
    }
    ///<inheritdoc/>

    public virtual async Task<bool> DeleteAsync(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }
    ///<inheritdoc/>

    public virtual async Task<bool> EditAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }
    ///<inheritdoc/>

    public virtual Task<bool> ExistsAsync(TId id)
    {
        return _context.Set<TEntity>().AnyAsync(e => e.Id.Equals(id));
    }
    ///<inheritdoc/>

    public virtual async Task<IList<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }

    public virtual async Task<IQueryable<TEntity>> GetAllQuerableAsync()
    {
        await Task.Yield();
        return _context.Set<TEntity>().AsQueryable();
    }

    ///<inheritdoc/>

    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {
        return await _context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id.Equals(id));
    }
}

