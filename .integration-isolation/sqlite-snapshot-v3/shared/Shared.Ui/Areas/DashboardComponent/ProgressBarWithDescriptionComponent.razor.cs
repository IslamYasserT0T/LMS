using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Shared.Ui.Areas.DashboardComponent;

public partial class ProgressBarWithDescriptionComponent
{
    [Parameter] public string Title { get; set; }
    [Parameter] public string Subtitle { get; set; }
    [Parameter] public int Value { get; set; }
    [Parameter] public Color Color { get; set; } = Color.Info;
    [Parameter] public Size Size { get; set; } = Size.Medium;
    [Parameter] public EventCallback OnButtonClick { get; set; }
}
