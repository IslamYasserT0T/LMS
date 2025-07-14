using AutoMapper;
using SampleModule.Application.Contracts;
using SampleModule.Application.DTOs.Sample;
using SampleModule.Domain.Entities;
using SampleModule.Domain.Repositories.Samples;
using Shared.Core.DependencyInjections;
using Shared.Core.Results;

namespace SampleModule.Application.Services;

public class SampleAppService : ISampleAppService, IApplicationService, IScopedService
{
    private readonly ISampleRepository _sampleRepository;
    private readonly IMapper _mapper;
    public SampleAppService(ISampleRepository sampleRepository)
    {
        _sampleRepository = sampleRepository;
    }
    #region Public Methods
    ///<inheritdoc/>
    public async Task<Result<SampleDto>> CreateAsync(CreateUpdateSampleDto input)
    {
        var _sample = _mapper.Map<Sample>(input);
        var _isAdded = await _sampleRepository.AddAsync(_sample);
        if (!_isAdded)
            return Result.Error("Sample could not be added");
        return Result.Success(_mapper.Map<SampleDto>(_sample));

    }
    ///<inheritdoc/>

    public async Task<Result<SampleDto>> DeleteAsync(Guid id)
    {
        var _sample = await _sampleRepository.GetByIdAsync(id);
        if (_sample is null)
            return Result.Error("Sample not found");
        var _isDeleted = await _sampleRepository.DeleteAsync(_sample);
        if (!_isDeleted)
            return Result.Error("Sample could not be deleted");
        return Result.Success(_mapper.Map<SampleDto>(_sample));
    }

    ///<inheritdoc/>

    public async Task<Result<List<SampleDto>>> GetAllAsync()
    {
        var _samples = await _sampleRepository.GetAllAsync();
        return Result.Success(_mapper.Map<List<SampleDto>>(_samples));
    }
    ///<inheritdoc/>

    public async Task<Result<SampleDto>> GetByIdAsync(Guid id)
    {
        var _sample = await _sampleRepository.GetByIdAsync(id);
        if (_sample is null)
            return Result.Error("Sample not found");

        return Result.Success(_mapper.Map<SampleDto>(_sample));
    }
    ///<inheritdoc/>
    public async Task<Result<SampleDto>> UpdateAsync(CreateUpdateSampleDto input, Guid id)
    {
        var _sample = await _sampleRepository.GetByIdAsync(id);
        if (_sample is null)
            return Result.Error("Sample not found");
        _mapper.Map(input, _sample);
        var _isUpdated = await _sampleRepository.EditAsync(_sample);
        if (!_isUpdated)
            return Result.Error("Sample could not be updated");
        return Result.Success(_mapper.Map<SampleDto>(_sample));
    }
    #endregion
}
