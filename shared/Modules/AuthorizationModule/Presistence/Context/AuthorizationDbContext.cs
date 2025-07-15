using Casbin.Persist.Adapter.EFCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorizationModule.Presistence.Context;

public class AuthorizationDbContext : CasbinDbContext<int>
{
    public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> dbContext) : base(dbContext)
    {
    }
}
