using System.Collections.Generic;
using Bartendro.Application.Services;

namespace Bartendro.Application.Orchestrators
{
    public interface IDispensersOrchestrator
    {
        IEnumerable<string> GetPorts();
    }

    internal class DispensersOrchestrator : IDispensersOrchestrator
    {
        private readonly ISerialPortService _serialPortService;

        public DispensersOrchestrator(ISerialPortService serialPortService)
        {
            _serialPortService = serialPortService;
        }

        public IEnumerable<string> GetPorts()
        {
            return _serialPortService.GetPorts();
        }
    }
}