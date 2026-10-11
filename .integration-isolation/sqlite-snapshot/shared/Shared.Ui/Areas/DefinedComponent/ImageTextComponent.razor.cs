using Microsoft.AspNetCore.Components;

namespace Shared.Ui.Areas.DefinedComponent
{
    public partial class ImageTextComponent
    {
        [Parameter] public string ImageUrl { get; set; } = string.Empty;
        [Parameter] public string AltText { get; set; } = "Card image";
        [Parameter] public string Title { get; set; } = "Default Title";
    }
}
