# Arabic and RTL Browser Results

Not rerun. Earlier evidence showed RTL flag change without real `ar-SA` culture/resource resolution. Current source still requires isolated Host startup and browser/circuit verification before Arabic or RTL can pass.

Required proof: `CultureInfo.CurrentCulture` and `CurrentUICulture` equal `ar-SA`, IY.Reporting.Blazor Arabic resource labels render in Viewer and Designer, circuit reload preserves culture, and RTL layout remains usable.
