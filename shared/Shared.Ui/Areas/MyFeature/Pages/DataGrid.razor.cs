using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Ui.Areas.MyFeature.Pages;
public partial class DataGrid<TItem>
{
    private string _searchString = "";

    [Parameter] public Func<GridState<TItem>, Task<GridData<TItem>>> ServerData { get; set; }
    [Parameter] public RenderFragment Columns { get; set; }
    [Parameter] public RenderFragment<TItem> ActionsTemplate { get; set; }
    [Parameter] public string Title { get; set; } = "Items";
    [Parameter] public EventCallback CreateCallback { get; set; }
    [Parameter] public EventCallback<string> OnSearch { get; set; }
    public MudDataGrid<TItem> GridRef { get; set; }
    private async Task OnSearchChanged(string value)
    {
        _searchString = value;
        await OnSearch.InvokeAsync(value);
        await GridRef.ReloadServerData();
    }
}
