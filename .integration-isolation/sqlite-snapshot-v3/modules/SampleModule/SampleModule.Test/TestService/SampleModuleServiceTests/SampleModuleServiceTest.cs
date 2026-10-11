using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Moq;
using SampleModule.Application.DTOs.Sample;
using SampleModule.Application.Mapping;
using SampleModule.Application.Services;
using SampleModule.Infrastructure.Repositories.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SampleModule.Test.TestService.SampleModuleServiceTests;

public class SampleModuleServiceTest
{
    private static SampleAppService _sampleAppService;

    public SampleModuleServiceTest()
    {
        var dbContext = SampleModuleContextGenerator.Generator();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SampleProfile>(), new LoggerFactory());
        var sampleRepository = new SampleRepository(dbContext);
        var mapper = config.CreateMapper();
        _sampleAppService = new SampleAppService(sampleRepository, mapper);
    }

    private SampleAppService CreateSampleService()
    {
        if (_sampleAppService is null)
        {
            var dbContext = SampleModuleContextGenerator.Generator();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SampleProfile>(), new LoggerFactory());
            var sampleRepository = new SampleRepository(dbContext);
            var mapper = config.CreateMapper();
            _sampleAppService = new SampleAppService(sampleRepository, mapper);
        }

        return _sampleAppService;
    }

    private void CheckService()
    {
        if (_sampleAppService is null)
            _sampleAppService = CreateSampleService();
    }

    [Fact]
    public async Task CreateInfoCard_ShouldReturnSuccess()
    {
        CheckService();
        var CreateUpdateSampleDto = new CreateUpdateSampleDto()
        {
           Name = "Test",

        };

        var result = await _sampleAppService.CreateAsync(CreateUpdateSampleDto);
        Assert.True(result.IsSuccess);
    }

   
}
