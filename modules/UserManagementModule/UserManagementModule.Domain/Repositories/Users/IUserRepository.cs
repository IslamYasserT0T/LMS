using Shared.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Domain.Entities;

namespace UserManagementModule.Domain.Repositories.Users;

/// <summary>
/// Defines the contract for the User Repository.
/// </summary>
public interface IUserRepository :IRepository<ApplicationUser, Guid>
{
}
