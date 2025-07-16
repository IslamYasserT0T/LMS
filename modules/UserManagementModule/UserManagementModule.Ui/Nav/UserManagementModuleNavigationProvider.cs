using Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementModule.Ui.Nav;

public class UserManagementModuleNavigationProvider : INavigationProvider
{
    public IEnumerable<NavMenuItem> GetMenuItems()
    {
        return new[]
        {
            new NavMenuItem
            {
                Title = "User Management Module",
                Url = "/Component/razor",
                Icon = "oi oi-box"
            }
        };
    }
}