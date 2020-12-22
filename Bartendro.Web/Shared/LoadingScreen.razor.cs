using Microsoft.AspNetCore.Components;

namespace Bartendro.Web.Shared
{
    public partial class LoadingScreen
    {
        [Parameter]
        public bool Loading { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }
    }
}