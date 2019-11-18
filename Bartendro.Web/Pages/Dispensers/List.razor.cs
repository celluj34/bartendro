using System.Collections.Generic;
using Bartendro.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Bartendro.Web.Pages.Dispensers
{
    public partial class List
    {
        protected bool Loading = true;
        protected IEnumerable<string> Ports;

        [Inject]
        private ISerialPortService DispensersOrchestrator {get;set;}

        protected override void OnInitialized()
        {
            Ports = DispensersOrchestrator.GetPorts();

            Loading = false;
        }

        protected void Refresh()
        {
            Loading = true;

            Ports = DispensersOrchestrator.GetPorts();

            Loading = false;
        }
    }
}