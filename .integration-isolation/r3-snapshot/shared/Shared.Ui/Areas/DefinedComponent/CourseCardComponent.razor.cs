using Microsoft.AspNetCore.Components;

namespace Shared.Ui.Areas.DefinedComponent
{
    public partial class CourseCardComponent
    {
        [Parameter] public string ImageUrl { get; set; } = string.Empty;
        [Parameter] public string Title { get; set; } = "تصميم أنظمة الطاقه الشمسية";
        [Parameter] public string Category { get; set; } = "الطاقه المستدامة";
        [Parameter] public double Rating { get; set; } = 4.3;
        [Parameter] public string InstructorName { get; set; } = "م/ عبد الله محمد";
        [Parameter] public string InstructorImage { get; set; } = "assets/images/instructor.jpg";
        [Parameter] public int Price { get; set; } = 120;
    }
}
