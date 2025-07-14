using SampleModule.Domain.Entities.Samples;
using Shared.Core.DependencyInjections;
using Shared.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Domain.Repositories.Samples;
/// <summary>
/// Sample Respositoy
/// </summary>
public interface ISampleRepository : IRepository<Sample, Guid>,  IApplicationService, IScopedService
{
}
