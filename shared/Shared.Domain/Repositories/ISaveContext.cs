using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core.Repositories;

/// <summary>
/// Represents a context for saving changes.
/// </summary>
public interface ISaveContext
{
    /// <summary>
    /// Asynchronously saves all changes made in this context to the underlying database.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
