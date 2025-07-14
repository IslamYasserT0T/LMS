using Shared.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Domain.Entities;

public class Sample : Auditable<Guid>
{
    public string Name { get; set; }
}
