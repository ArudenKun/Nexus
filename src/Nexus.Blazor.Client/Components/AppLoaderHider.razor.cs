using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Nexus.Blazor.Client.Components;

public partial class AppLoaderHider
{
    [Inject]
    public IJSRuntime JavaScript { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JavaScript.InvokeVoidAsync(
                "eval",
                "document.getElementById('app-loader')?.remove()"
            );
        }
    }
}
