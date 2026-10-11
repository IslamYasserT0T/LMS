using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.SharedConractsApplication;

/// <summary>
/// Interface for retrieving an entity by its identifier asynchronously.
/// </summary>
/// <typeparam name="TOutput">The output type, which must be a class.</typeparam>
/// <typeparam name="TId">The identifier type, which must be a class.</typeparam>
public interface IGetByIdAppService<TOutput, TId> 
    where TOutput : class // constraint for output type
{
    /// <summary>
    /// Retrieves an entity by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation, containing the retrieved entity.</returns>
    Task<TOutput> GetByIdAsync(TId id);
}
