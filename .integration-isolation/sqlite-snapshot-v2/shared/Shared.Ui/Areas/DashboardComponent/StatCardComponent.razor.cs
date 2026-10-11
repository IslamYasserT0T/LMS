using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Shared.Ui.Areas.DashboardComponent;

public partial class StatCardComponent
{
    [Parameter] public string Title { get; set; } = "Total Users";
    [Parameter] public string ImageUrl { get; set; } = "_content/YourApp/images/icon.png";
    [Parameter] public string Value { get; set; } = "1200";
    [Parameter] public string ChipLabel { get; set; } = "+8%";
    [Parameter] public string ChipColor { get; set; } = "#dc3545";
    [Parameter] public string TextColor { get; set; } = "#dc3545";
    [Parameter] public string ChipIcon { get; set; } = Icons.Material.Filled.ArrowUpward;
}
