using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.SharedConractsApplication;

/// <summary>
/// Interface for creating a new entity asynchronously.
/// </summary>
/// <typeparam name="TInput">The input type, which must be a class.</typeparam>
/// <typeparam name="TOutput">The output type, which must be a class.</typeparam>
public interface ICreateAppService<TInput, TOutput> where TInput : class where TOutput : class
{
    /// <summary>
    /// Creates a new entity asynchronously.
    /// </summary>
    /// <param name="input">The input data for creating the entity.</param>
    /// <returns>A task that represents the creation operation, returning the created entity.</returns>
    Task<TOutput> CreateAsync(TInput input);
}