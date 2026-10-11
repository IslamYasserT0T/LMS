using Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleModule.Ui.Nav;
public class ProductNavigationProvider : INavigationProvider
{
    public IEnumerable<NavMenuItem> GetMenuItems()
    {
        return new[]
        {
            new NavMenuItem
            {
                Title = "Sample Module",
                Url = "/Component/razor",
                Icon = "oi oi-box"
            },
            new NavMenuItem
            {
                Title = "Sample Module2",
                Url = "/Component/razor",
                Icon = "oi oi-box"
            }
        };
    }
}
