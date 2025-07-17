using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Application.DTOs;
using UserManagementModule.Domain.Entities;

namespace UserManagementModule.Application.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ApplicationUser,CreateUpdateUserDto>().ReverseMap();
        CreateMap<UserDto,ApplicationUser>().ReverseMap();
        CreateMap<CreateUpdateUserDto,UserDto>().ReverseMap();
    }
}
