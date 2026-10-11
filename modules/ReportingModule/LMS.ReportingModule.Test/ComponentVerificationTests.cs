using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Xunit;
using Xunit.Abstractions;

namespace LMS.ReportingModule.Test;

public class ComponentVerificationTests
{
    private readonly ITestOutputHelper _output;

    public ComponentVerificationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void VerifyViewerAndDesignerSignatures()
    {
        var viewerType = typeof(IY.Reporting.Blazor.Components.Viewer.ReportViewer);
        Assert.NotNull(viewerType);
        
        var viewerSchemaProp = viewerType.GetProperty("Schema");
        Assert.NotNull(viewerSchemaProp);
        Assert.NotNull(viewerSchemaProp.GetCustomAttribute<ParameterAttribute>());
        Assert.Equal(typeof(IY.Reporting.Schema.ReportSchema), viewerSchemaProp.PropertyType);

        var viewerCapProp = viewerType.GetProperty("Capabilities");
        Assert.NotNull(viewerCapProp);
        Assert.NotNull(viewerCapProp.GetCustomAttribute<ParameterAttribute>());
        Assert.Equal(typeof(IY.Reporting.Blazor.Models.ReportingCapabilities), viewerCapProp.PropertyType);

        var designerType = typeof(IY.Reporting.Blazor.Components.Designer.ReportDesigner);
        Assert.NotNull(designerType);

        var designerSchemaProp = designerType.GetProperty("Schema");
        Assert.NotNull(designerSchemaProp);
        Assert.NotNull(designerSchemaProp.GetCustomAttribute<ParameterAttribute>());
        Assert.Equal(typeof(IY.Reporting.Schema.ReportSchema), designerSchemaProp.PropertyType);

        var designerCapProp = designerType.GetProperty("Capabilities");
        Assert.NotNull(designerCapProp);
        Assert.NotNull(designerCapProp.GetCustomAttribute<ParameterAttribute>());
        Assert.Equal(typeof(IY.Reporting.Blazor.Models.ReportingCapabilities), designerCapProp.PropertyType);
    }

    [Fact]
    public void VerifyWrapperComponents_RenamedAndRouteBound()
    {
        var reportingAssembly = typeof(LMS.ReportingModule.Module).Assembly;

        // Old conflicting names must NOT exist in LMS.ReportingModule
        var oldViewerType = reportingAssembly.GetType("LMS.ReportingModule.Ui.Pages.ReportViewer");
        Assert.Null(oldViewerType);

        var oldDesignerType = reportingAssembly.GetType("LMS.ReportingModule.Ui.Pages.ReportDesigner");
        Assert.Null(oldDesignerType);

        // New renamed wrapper components MUST exist
        var viewerPageType = reportingAssembly.GetType("LMS.ReportingModule.Ui.Pages.ReportsViewerPage");
        Assert.NotNull(viewerPageType);
        var viewerRoute = viewerPageType.GetCustomAttributes<RouteAttribute>().FirstOrDefault();
        Assert.NotNull(viewerRoute);
        Assert.Equal("/reports/viewer", viewerRoute.Template);

        var designerPageType = reportingAssembly.GetType("LMS.ReportingModule.Ui.Pages.ReportsDesignerPage");
        Assert.NotNull(designerPageType);
        var designerRoute = designerPageType.GetCustomAttributes<RouteAttribute>().FirstOrDefault();
        Assert.NotNull(designerRoute);
        Assert.Equal("/reports/designer", designerRoute.Template);

        var indexPageType = reportingAssembly.GetType("LMS.ReportingModule.Ui.Pages.ReportsIndex");
        Assert.NotNull(indexPageType);
        var indexRoute = indexPageType.GetCustomAttributes<RouteAttribute>().FirstOrDefault();
        Assert.NotNull(indexRoute);
        Assert.Equal("/reports", indexRoute.Template);
    }
}
