using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Nexus.Blazor.Client.Pages;

[Authorize]
public partial class Index
{
    [Inject]
    protected NavigationManager Navigation { get; set; } = null!;

    protected override void Dispose(bool disposing)
    {
        PageLayout.ShowToolbar = true;
    }
}
