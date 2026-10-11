using Microsoft.AspNetCore.Components;

namespace Shared.Ui.Areas.DefinedComponent
{
    public partial class QuestionComponent
    {
        private bool _expanded = false;

        [Parameter] public string Question { get; set; } = "سؤال افتراضي؟";
        [Parameter] public string Answer { get; set; } = "هذا هو النص التوضيحي للإجابة.";

        private void Toggle() => _expanded = !_expanded;
    }
}
