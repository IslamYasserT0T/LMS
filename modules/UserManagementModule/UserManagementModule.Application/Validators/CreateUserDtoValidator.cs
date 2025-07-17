using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementModule.Application.DTOs;

namespace UserManagementModule.Application.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUpdateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Email is not valid");
    }
}
