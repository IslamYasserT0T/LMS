using System.Drawing;
using Microsoft.AspNetCore.Components;

namespace Shared.Ui.Areas.DefinedComponent
{
    public partial class MainContentHomeComponent
    {
        [Parameter] public string ImageUrl { get; set; } = string.Empty;
        [Parameter] public string Headline { get; set; } = "Default Headline";
        [Parameter] public string Paragraph { get; set; } = "Default paragraph text.";
        [Parameter] public string ButtonText { get; set; } = "Click Me";
        [Parameter] public EventCallback OnButtonClick { get; set; }
        [Parameter]
        public string TextColor { get; set; } = Color.Black.ToString();
        
    }
}
