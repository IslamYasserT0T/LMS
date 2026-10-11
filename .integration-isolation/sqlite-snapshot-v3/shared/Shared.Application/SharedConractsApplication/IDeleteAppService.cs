using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.SharedConractsApplication;

/// <summary>
/// Interface for deleting an entity asynchronously.
/// </summary>
/// <typeparam name="TOutput">The output type, which must be a class.</typeparam>
/// <typeparam name="TId">The identifier type, which must be a class.</typeparam>
public interface IDeleteAppService<TOutput, TId>
    where TOutput : class // constraint for output type
{
    /// <summary>
    /// Deletes an entity asynchronously by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns>A task that represents the asynchronous operation, returning the deleted entity.</returns>
    Task<TOutput> DeleteAsync(TId id);
}