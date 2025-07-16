using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Domain.Entities;
using UserManagementModule.Domain.Repositories.Users;
using UserManagementModule.Infrastructure.Presistence.Context;

namespace UserManagementModule.Infrastructure.Repositories.Users;

public class UserRepository : Repository<ApplicationUser, Guid>, IUserRepository
{
    public UserRepository(UserManagmentModuleDBContext context) : base(context)
    {
    }

}
