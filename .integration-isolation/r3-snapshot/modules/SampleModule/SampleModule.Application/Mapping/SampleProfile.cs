using AutoMapper;
using SampleModule.Application.DTOs.Sample;
using SampleModule.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Application.Mapping;

public class SampleProfile : Profile
{
    public SampleProfile()
    {
        CreateMap<Sample, SampleDto>().ReverseMap();
        CreateMap<CreateUpdateSampleDto, Sample>().ReverseMap();
    }
}
