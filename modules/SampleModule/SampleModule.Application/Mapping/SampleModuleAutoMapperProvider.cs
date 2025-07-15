using AutoMapper;
using Shared.Application.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Application.Mapping;

public class SampleModuleAutoMapperProvider : IHasAutoMapperProfiles
{
    public IEnumerable<Profile> GetProfiles()
    {
        return new Profile[]
        {
            new SampleProfile()
        };
    }
}