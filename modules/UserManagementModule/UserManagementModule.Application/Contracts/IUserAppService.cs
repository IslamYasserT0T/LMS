using Shared.Application.SharedConractsApplication;
using Shared.Core.DependencyInjections;
using Shared.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Application.DTOs;

namespace UserManagementModule.Application.Contracts;

public interface IUserAppService : ICrudAppService<CreateUpdateUserDto, Result<UserDto>, Guid, Result<List<UserDto>>>
{
}
