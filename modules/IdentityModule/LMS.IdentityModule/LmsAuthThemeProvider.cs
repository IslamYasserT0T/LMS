using System.Threading.Tasks;
using IY.Auth.Contracts.UI;

namespace LMS.IdentityModule;

/// <summary>
/// Provides the LMS design system theme for IY.Auth.UI components.
/// Configured with MudBlazor / LMS brand styling and Noto Kufi Arabic font family.
/// </summary>
public class LmsAuthThemeProvider : IAuthThemeProvider
{
    public Task<AuthTheme> GetThemeAsync()
    {
        return Task.FromResult(new AuthTheme
        {
            PrimaryColor = "var(--mud-palette-primary, #594AE2)",
            BackgroundColor = "var(--mud-palette-background, #FFFFFF)",
            TextColor = "var(--mud-palette-text-primary, #424242)",
            BorderRadius = "var(--mud-default-borderradius, 8px)",
            Spacing = "16px",
            FontFamily = "Noto Kufi Arabic, system-ui, -apple-system, sans-serif"
        });
    }
}
