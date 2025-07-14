using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.SharedConractsApplication;

/// <summary>
/// Interface for updating an existing entity asynchronously.
/// </summary>
/// <typeparam name="TInput">The input type, which must be a class.</typeparam>
/// <typeparam name="TOutput">The output type, which must be a class.</typeparam>
/// <typeparam name="TId">The identifier type, which must be a class.</typeparam>
public interface IUpdateAppService<TInput, TOutput, TId>
    where TInput : class // constraint for input type
    where TOutput : class // constraint for output type
{
    /// <summary>
    /// Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="input">The input data for the update operation.</param>
    /// <returns>A task that represents the asynchronous update operation, returning the updated entity.</returns>
    Task<TOutput> UpdateAsync(TInput input, TId id);
}
