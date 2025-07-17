using AutoMapper;
using Shared.Application.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementModule.Application.Mapping;

public class UserModuleAutoMapperProvider : IHasAutoMapperProfiles
{
    public IEnumerable<Profile> GetProfiles()
    {
        return new Profile[]
        {
            new UserProfile()
        };
    }
}
