using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Shared.Ui.Areas.DashboardComponent;

public partial class ProgressBarWithoutDescriptionComponent
{
    [Parameter] public string Title { get; set; }
    [Parameter] public int Value { get; set; }
    [Parameter] public Color Color { get; set; } = Color.Info;
    [Parameter] public Size Size { get; set; } = Size.Medium;

    private string ProgressStyle => $"height:36px; transform: translateX({-Value}%); transition: transform 0.3s ease;";

    [Parameter] public EventCallback OnButtonClick { get; set; }
}
