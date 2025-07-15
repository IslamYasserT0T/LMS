using Shared.Core.Repositories;
using Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Repositories;

///<inheritdoc/>
public class SaveContext : ISaveContext
{
    private readonly BaseDbContext _dbContext;

    public SaveContext(BaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    ///<inheritdoc/>
    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
}