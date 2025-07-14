using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core.Repositories;

/// <summary>
/// Interface for base generic repository
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TId"></typeparam>
public interface IRepository<TEntity, TId> : ISaveContext where TEntity : class
{
    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns> returns true if success otherwise false</returns>
    Task<bool> AddAsync(TEntity entity);
    /// <summary>
    /// Edit entity
    /// </summary>
    /// <param name="entity"> </param>
    /// <returns> returns true if success otherwise false</returns>
    Task<bool> EditAsync(TEntity entity);
    /// <summary>
    /// Get entity
    /// </summary>
    /// <param name="id"></param>
    /// <returns> returns specific entity </returns>
    Task<TEntity?> GetByIdAsync(TId id);
    /// <summary>
    /// Get all
    /// </summary>
    /// <returns> returns list of entities</returns>
    Task<IQueryable<TEntity>> GetAllQuerableAsync();
    Task<IList<TEntity>> GetAllAsync();
    /// <summary>
    /// Delete entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns> returns true if success otherwise false </returns>
    Task<bool> DeleteAsync(TEntity entity);
    /// <summary>
    /// Check if entity exists
    /// </summary>
    /// <param name="id"></param>
    /// <returns> returns true if exists otherwise false </returns>
    Task<bool> ExistsAsync(TId id);
}