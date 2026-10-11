using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.SharedConractsApplication;

/// <summary>
/// Interface for getting a list of entities asynchronously.
/// </summary>
/// <typeparam name="TOutput">The output type, which must be a class.</typeparam>
public interface IGetListAppService<TOutputList> where TOutputList : class
{
    /// <summary>
    /// Retrieves a list of all entities asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing a list of entities.</returns>
    Task<TOutputList> GetAllAsync();
}
