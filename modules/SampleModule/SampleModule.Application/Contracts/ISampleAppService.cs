using SampleModule.Application.DTOs.Sample;
using Shared.Application.SharedConractsApplication;
using Shared.Core.DependencyInjections;
using Shared.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Application.Contracts;
/// <summary>
/// Sample Contract for sample entity.
/// </summary>
public interface ISampleAppService : ICrudAppService<CreateUpdateSampleDto, Result<SampleDto>, Guid, Result<List<SampleDto>>>, IApplicationService, IScopedService
{
}
