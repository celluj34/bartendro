using System.Collections.Generic;
using System.IO.Ports;

namespace Bartendro.Application.Services
{
    internal interface ISerialPortService
    {
        IEnumerable<string> GetPorts();
    }

    internal class SerialPortService : ISerialPortService
    {
        public IEnumerable<string> GetPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}