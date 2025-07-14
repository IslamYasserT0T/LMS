using SampleModule.Domain.Entities;
using SampleModule.Domain.Repositories.Samples;
using SampleModule.Infrastructure.Presistence.Context;
using Shared.Core.DependencyInjections;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Infrastructure.Repositories.Samples;

public class SampleRepository : Repository<Sample, Guid>, 
    ISampleRepository , IApplicationService, IScopedService
{
    public SampleRepository(SampleModuleDBContext context) : base(context)
    {
    }
}
