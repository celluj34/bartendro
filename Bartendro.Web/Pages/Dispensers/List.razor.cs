using System.Collections.Generic;
using Bartendro.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Bartendro.Web.Pages.Dispensers
{
    public partial class List
    {
        private bool _loading = true;
        private IEnumerable<string> _ports;

        [Inject]
        private ISerialPortService DispensersOrchestrator {get;set;}

        protected override void OnInitialized()
        {
            _ports = DispensersOrchestrator.GetPorts();

            _loading = false;
        }

        protected void Refresh()
        {
            _loading = true;

            _ports = DispensersOrchestrator.GetPorts();

            _loading = false;
        }
    }
}