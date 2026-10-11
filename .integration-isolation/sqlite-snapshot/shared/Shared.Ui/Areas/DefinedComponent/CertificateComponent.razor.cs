using Microsoft.AspNetCore.Components;

namespace Shared.Ui.Areas.DefinedComponent
{
    public partial class CertificateComponent
    {
        [Parameter] public string TopText { get; set; } = "أحدث الدورات";
        [Parameter] public string LeftImage { get; set; } = "assets/images/left.png";
        [Parameter] public string RightImage { get; set; } = "assets/images/right.png";
        [Parameter] public List<string> listImages { get; set; } = new List<string>();
        [Parameter] public string Image2 { get; set; } = "";
    }
}
