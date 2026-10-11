using FluentValidation;
using SampleModule.Application.DTOs.Sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Application.Validators;

public class CreateSampleDtoValidator : AbstractValidator<CreateUpdateSampleDto>
{
    public CreateSampleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }
}
