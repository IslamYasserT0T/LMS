using Microsoft.AspNetCore.Components;

namespace Shared.Ui.Entities.ImageWithAnalysisTemplate
{
    public class ImageWithAnalysisModel
    {
        [Parameter] public string ImageUrl { get; set; } = string.Empty;
        [Parameter] public string AltText { get; set; } = "صورة التصنيف";
        [Parameter] public string Name { get; set; } = "علوم الحاسب";
        [Parameter] public int Count { get; set; } = 0;
    }
}
