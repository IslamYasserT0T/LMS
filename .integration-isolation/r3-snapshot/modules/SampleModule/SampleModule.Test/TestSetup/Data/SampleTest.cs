using Microsoft.VisualBasic;
using SampleModule.Domain.Entities;
using SampleModule.Infrastructure.Presistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Test.TestSetup.Data;

public static class SampleTest
{
    public static void AddSample(this SampleModuleDBContext context)
    {
        context.Samples.AddRange(new List<Sample>()
        {
            new Sample()
            {
                Id = new Guid("12345678-1234-1234-1234-1234567890ab"),
                Name = "Sample test 1",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CreatedBy = "12345678-1234-1234-1234-1234567890ab",
                ModifiedBy = "12345678-1234-1234-1234-1234567890ab",
            },
            new Sample()
            {
                Id = new Guid("12345678-1234-1234-1234-1234567890cd"),
                Name = "Sample test 2",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CreatedBy = "12345678-1234-1234-1234-1234567890cd",
                ModifiedBy = "12345678-1234-1234-1234-1234567890cd",
            }
        });
    }
}
