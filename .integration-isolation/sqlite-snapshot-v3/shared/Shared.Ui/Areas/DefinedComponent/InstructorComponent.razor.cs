using Microsoft.AspNetCore.Components;

namespace Shared.Ui.Areas.DefinedComponent
{
    public partial class InstructorComponent
    {
        [Parameter] public string ImageUrl { get; set; } = "assets/images/teacher1.png";
        [Parameter] public string FullName { get; set; } = "الاسم بالكامل ثلاثي";
        [Parameter] public string Email { get; set; } = "teacher_name@mail.com";
    }
}
