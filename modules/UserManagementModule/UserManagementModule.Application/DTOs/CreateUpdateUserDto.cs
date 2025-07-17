using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Core.Enums;

namespace UserManagementModule.Application.DTOs;

public class CreateUpdateUserDto : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public int? CityId { get; set; }
    public int? StateId { get; set; }
    public int? CountryId { get; set; }
    public string? PostalCode { get; set; }
    public Status Status { get; set; }
    public string? Image { get; set; }
}
