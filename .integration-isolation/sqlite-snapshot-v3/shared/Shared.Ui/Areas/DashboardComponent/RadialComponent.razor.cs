using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Shared.Ui.Areas.DashboardComponent;

public partial class RadialComponent
{
    [Parameter] public double ActualValue { get; set; } = 0;
    [Parameter] public double MaxValue { get; set; } = 100;
    [Parameter] public string Title { get; set; } = "Progress";
    [Parameter] public Color Color { get; set; } = Color.Primary;
    [Parameter] public Size Size { get; set; } = Size.Medium;
    [Parameter] public int StrokeWidth { get; set; } = 4;

    private double NormalizedValue => MaxValue > 0
        ? Math.Min((ActualValue / MaxValue) * 100, 100)
        : 0;
}
