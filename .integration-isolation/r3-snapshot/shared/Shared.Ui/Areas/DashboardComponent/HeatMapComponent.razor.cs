using ApexCharts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Ui.Entities.RegionTemplate;

namespace Shared.Ui.Areas.DashboardComponent
{
    public partial class HeatMapComponent
    {
        [Parameter]
        public List<RegionIncident> Incidents { get; set; }

        private ApexChartOptions<RegionIncident> options;
        private List<RegionIncident> incidents = new();
        private bool isInitialized = false;

        protected override void OnInitialized()
        {
            options = new ApexChartOptions<RegionIncident>
            {
                Colors = new List<string> { "#56B1F7", "#3D7FB3", "#29587E", "#132B43" },
                Chart = new Chart
                {
                    Height = 370,
                    Width = 380
                },
            };
        }

        protected override void OnParametersSet()
        {
            if (Incidents != null && Incidents.Any())
            {
                incidents = Incidents;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if ((firstRender || !isInitialized) && incidents.Any())
            {
                await UpdateMapColors();
                isInitialized = true;
            }
        }

        private async Task UpdateMapColors()
        {
            try
            {
                var regionData = incidents
                    .GroupBy(e => e.Region)
                    .ToDictionary(
                        g => g.Key,
                        g => (int)g.Average(x => x.Severity)
                    );

                await JS.InvokeVoidAsync("colorSaudiMap", regionData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error coloring map: {ex.Message}");
            }
        }


    }
}
