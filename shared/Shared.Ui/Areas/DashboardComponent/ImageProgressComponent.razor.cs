using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Shared.Ui.Areas.DashboardComponent;

public partial class ImageProgressComponent
{
    [Parameter] public string ImageUrl { get; set; } = string.Empty;
    [Parameter] public string AltText { get; set; } = "Card image";
    [Parameter] public string Title { get; set; } = "Default Title";
    [Parameter] public string RightText { get; set; } = "Label";
    [Parameter] public string RightImageUrl { get; set; } = "https://via.placeholder.com/24";
    [Parameter] public string Subtitle { get; set; } = "Subheading";
    [Parameter] public string Description { get; set; } = "This is a sample description text for the card.";
    [Parameter] public int Value { get; set; }
    [Parameter] public int Remaining { get; set; }
    [Parameter] public Color Color { get; set; } = Color.Info;
    [Parameter] public Size Size { get; set; } = Size.Medium;

    [Parameter] public string ButtonText { get; set; } = "Details";
    [Parameter] public EventCallback OnButtonClick { get; set; }
}
