using Microsoft.AspNetCore.Components;

namespace Shared.Ui.Areas.DefinedComponent
{
    public partial class ImageWithAnalysisComponent
    {
        [Parameter] public string ImageUrl { get; set; } = string.Empty;
        [Parameter] public string AltText { get; set; } = "صورة التصنيف";
        [Parameter] public string Name { get; set; } = "علوم الحاسب";
        [Parameter] public int Count { get; set; } = 0;
    }
}
