using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application.Mapping;

/// <summary>
/// A class representing a collection of mapping profiles.
/// </summary>
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMappings();
    }

    /// <summary>
    /// Registers all entity and DTO mappings by calling the respective extension methods.
    /// </summary>
    private void CreateMappings()
    {
       
    }

}

