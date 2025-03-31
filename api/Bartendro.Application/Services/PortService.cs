using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Bartendro.Application.Services
{
    public class RouterDriver
    {
        private const int DISPENSER_DEFAULT_VERSION = 2;
        private const int DISPENSER_DEFAULT_VERSION_SOFTWARE_ONLY = 3;

        private const int BAUD_RATE = 9600;
        private const int DEFAULT_TIMEOUT = 2000; // in milliseconds

        private const int MAX_DISPENSERS = 15;
        private const int SHOT_TICKS = 20;

        private const int RAW_PACKET_SIZE = 10;
        private const int PACKET_SIZE = 8;

        private const int PACKET_ACK_OK = 0;
        private const int PACKET_CRC_FAIL = 1;
        private const int PACKET_ACK_TIMEOUT = 2;
        private const int PACKET_ACK_INVALID = 3;
        private const int PACKET_ACK_INVALID_HEADER = 4;
        private const int PACKET_ACK_HEADER_IN_PACKET = 5;
        private const int PACKET_ACK_CRC_FAIL = 6;

        private const int PACKET_PING = 3;
        private const int PACKET_SET_MOTOR_SPEED = 4;
        private const int PACKET_TICK_DISPENSE = 5;
        private const int PACKET_TIME_DISPENSE = 6;
        private const int PACKET_LED_OFF = 7;
        private const int PACKET_LED_IDLE = 8;
        private const int PACKET_LED_DISPENSE = 9;
        private const int PACKET_LED_DRINK_DONE = 10;
        private const int PACKET_IS_DISPENSING = 11;
        private const int PACKET_LIQUID_LEVEL = 12;
        private const int PACKET_UPDATE_LIQUID_LEVEL = 13;
        private const int PACKET_ID_CONFLICT = 14;
        private const int PACKET_LED_CLEAN = 15;
        private const int PACKET_SET_CS_THRESHOLD = 16;
        private const int PACKET_SAVED_TICK_COUNT = 17;
        private const int PACKET_RESET_SAVED_TICK_COUNT = 18;
        private const int PACKET_GET_LIQUID_THRESHOLDS = 19;
        private const int PACKET_SET_LIQUID_THRESHOLDS = 20;
        private const int PACKET_FLUSH_SAVED_TICK_COUNT = 21;
        private const int PACKET_TICK_SPEED_DISPENSE = 22;
        private const int PACKET_PATTERN_DEFINE = 23;
        private const int PACKET_PATTERN_ADD_SEGMENT = 24;
        private const int PACKET_PATTERN_FINISH = 25;
        private const int PACKET_SET_MOTOR_DIRECTION = 26;
        private const int PACKET_GET_VERSION = 27;
        private const int PACKET_COMM_TEST = 0xFE;

        private const int DEST_BROADCAST = 0xFF;

        private const int MOTOR_DIRECTION_FORWARD = 1;
        private const int MOTOR_DIRECTION_BACKWARD = 0;

        private const int LED_PATTERN_IDLE = 0;
        private const int LED_PATTERN_DISPENSE = 1;
        private const int LED_PATTERN_DRINK_DONE = 2;
        private const int LED_PATTERN_CLEAN = 3;
        private const int LED_PATTERN_CURRENT_SENSE = 4;
        private readonly string _device;
        private readonly int[] _dispenserIds;
        private readonly int[] _dispenserPorts;
        private readonly ILogger<RouterDriver> _logger;
        private readonly bool _softwareOnly;
        private int _dispenserVersion;
        private int _numDispensers;
        private SerialPort _serialPort;
        private string _startupLog;

        public RouterDriver(ILogger<RouterDriver> logger, string device, bool softwareOnly)
        {
            _logger = logger;
            _device = device;
            _softwareOnly = softwareOnly;
            _dispenserVersion = DISPENSER_DEFAULT_VERSION;
            _startupLog = string.Empty;
            _dispenserIds = Enumerable.Repeat(-1, MAX_DISPENSERS).ToArray();
            _dispenserPorts = Enumerable.Repeat(255, MAX_DISPENSERS).ToArray();
            _numDispensers = softwareOnly ? MAX_DISPENSERS : 0;
        }

        public string GetStartupLog()
        {
            return _startupLog;
        }

        public int GetDispenserVersion()
        {
            return _dispenserVersion;
        }

        public void Reset()
        {
            if (_softwareOnly)
            {
                return;
            }

            Close();
            Open();
        }

        public int Count()
        {
            return _numDispensers;
        }

        public void SetTimeout(int timeout)
        {
            if (_serialPort != null)
            {
                _serialPort.ReadTimeout = timeout;
                _serialPort.WriteTimeout = timeout;
            }
        }

        public void Open()
        {
            if (_softwareOnly)
            {
                return;
            }

            ClearStartupLog();

            try
            {
                _logger.LogInformation($"Opening {_device}");
                _serialPort = new SerialPort(_device, BAUD_RATE, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 10,
                    WriteTimeout = 10
                };

                _serialPort.Open();
            }
            catch (Exception ex)
            {
                throw new SerialIOException(ex.Message);
            }

            _logger.LogInformation("Done.\n");

            // Initialize status LED and dispenser select
            // Assuming StatusLED and DispenserSelect are implemented elsewhere
            var status = new StatusLED(_softwareOnly);
            status.SetColor(0, 0, 1);

            var dispenserSelect = new DispenserSelect(MAX_DISPENSERS, _softwareOnly);
            dispenserSelect.Open();
            dispenserSelect.Reset();

            // Prime the communication line
            _serialPort.Write([
                    170,
                    170,
                    170
                ],
                0,
                3);

            Thread.Sleep(1);

            _logger.LogInformation("Discovering dispensers");
            _numDispensers = 0;
            for (var port = 0; port < MAX_DISPENSERS; port++)
            {
                LogStartup($"port {port}:");
                dispenserSelect.Select(port);
                Thread.Sleep(10);
                while (true)
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.Write("???");
                    var data = new byte[3];
                    var bytesRead = _serialPort.Read(data, 0, 3);
                    if (bytesRead == 3)
                    {
                        if (data[0] != data[1] || data[0] != data[2])
                        {
                            LogStartup($"  {BitConverter.ToString(data)} -- inconsistent");
                            continue;
                        }

                        if (data[0] == 0)
                        {
                            LogStartup("  ignoring dispenser id 0.");
                            break;
                        }

                        int id = data[0];
                        _dispenserIds[_numDispensers] = id;
                        _dispenserPorts[_numDispensers] = port;
                        _numDispensers++;
                        LogStartup($"  {BitConverter.ToString(data)} -- Found dispenser with pump id {id:X2}, index {_numDispensers}");
                        break;
                    }

                    if (bytesRead > 1)
                    {
                        LogStartup($"  {BitConverter.ToString(data)} -- Did not receive 3 characters back. Trying again.");
                        Thread.Sleep(500);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Select(0);
            SetTimeout(DEFAULT_TIMEOUT);
            _serialPort.Write([
                    255
                ],
                0,
                1);

            var duplicateIds = _dispenserIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateIds.Any())
            {
                foreach (var dup in duplicateIds)
                {
                    if (dup == -1)
                    {
                        continue;
                    }

                    LogStartup("ERROR: Dispenser id conflict!\n");
                    var sent = false;
                    for (var i = 0; i < _dispenserIds.Length; i++)
                    {
                        if (_dispenserIds[i] == dup)
                        {
                            if (!sent)
                            {
                                SendPacket8(i, PACKET_ID_CONFLICT, 0);
                                sent = true;
                            }

                            LogStartup($"  dispenser {i} has id {dup}\n");
                            _dispenserIds[i] = -1;
                            _numDispensers--;
                        }
                    }
                }
            }

            _dispenserVersion = GetDispenserVersion(0);
            if (_dispenserVersion < 0)
            {
                _dispenserVersion = DISPENSER_DEFAULT_VERSION;
            }
            else
            {
                status.SwapBlueGreen();
            }

            _logger.LogInformation($"Detected dispensers version {_dispenserVersion}. (Only checked first dispenser)");

            LedIdle();
        }

        public void Close()
        {
            if (_softwareOnly)
            {
                return;
            }

            _serialPort.Close();
            _serialPort = null;
        }

        public void Log(string msg)
        {
            if (_softwareOnly)
            {
                return;
            }

            try
            {
                var t = DateTime.Now;

                // Assuming cl is a StreamWriter or similar
                // cl.Write($"{t:yyyy-MM-dd HH:mm} {msg}");
                // cl.Flush();
            }
            catch (IOException)
            {
                // Handle exception
            }
        }

        public bool MakeShot()
        {
            if (_softwareOnly)
            {
                return true;
            }

            SendPacket32(0, PACKET_TICK_DISPENSE, 90);
            return true;
        }

        public bool Ping(int dispenser)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket32(dispenser, PACKET_PING, 0);
        }

        public bool Start(int dispenser)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket8(dispenser, PACKET_SET_MOTOR_SPEED, 255, 1);
        }

        public bool SetMotorDirection(int dispenser, int direction)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket8(dispenser, PACKET_SET_MOTOR_DIRECTION, direction);
        }

        public bool Stop(int dispenser)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket8(dispenser, PACKET_SET_MOTOR_SPEED, 0);
        }

        public bool DispenseTime(int dispenser, int duration)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket32(dispenser, PACKET_TIME_DISPENSE, duration);
        }

        public bool DispenseTicks(int dispenser, int ticks, int speed = 255)
        {
            if (_softwareOnly)
            {
                return true;
            }

            var ret = SendPacket16(dispenser, PACKET_TICK_SPEED_DISPENSE, ticks, speed);

            // if it fails, re-try once.
            if (!ret)
            {
                _logger.LogError("*** dispense command failed. re-trying once.");
                ret = SendPacket16(dispenser, PACKET_TICK_SPEED_DISPENSE, ticks, speed);
            }

            return ret;
        }

        public bool LedOff()
        {
            if (_softwareOnly)
            {
                return true;
            }

            Sync(0);
            SendPacket8(DEST_BROADCAST, PACKET_LED_OFF, 0);
            return true;
        }

        public bool LedIdle()
        {
            if (_softwareOnly)
            {
                return true;
            }

            Sync(0);
            SendPacket8(DEST_BROADCAST, PACKET_LED_IDLE, 0);
            Thread.Sleep(10);
            Sync(1);
            return true;
        }

        public bool LedDispense()
        {
            if (_softwareOnly)
            {
                return true;
            }

            Sync(0);
            SendPacket8(DEST_BROADCAST, PACKET_LED_DISPENSE, 0);
            Thread.Sleep(10);
            Sync(1);
            return true;
        }

        public bool LedComplete()
        {
            if (_softwareOnly)
            {
                return true;
            }

            Sync(0);
            SendPacket8(DEST_BROADCAST, PACKET_LED_DRINK_DONE, 0);
            Thread.Sleep(10);
            Sync(1);
            return true;
        }

        public bool LedClean()
        {
            if (_softwareOnly)
            {
                return true;
            }

            Sync(0);
            SendPacket8(DEST_BROADCAST, PACKET_LED_CLEAN, 0);
            Thread.Sleep(10);
            Sync(1);
            return true;
        }

        public bool LedError()
        {
            if (_softwareOnly)
            {
                return true;
            }

            Sync(0);
            SendPacket8(DEST_BROADCAST, PACKET_LED_CLEAN, 0);
            Thread.Sleep(10);
            Sync(1);
            return true;
        }

        public bool CommTest()
        {
            Sync(0);
            return SendPacket8(0, PACKET_COMM_TEST, 0);
        }

        public (bool? dispensing, bool? isOverCurrent) IsDispensing(int dispenser)
        {
            if (_softwareOnly)
            {
                return (false, false);
            }

            SetTimeout(100);
            var ret = SendPacket8(dispenser, PACKET_IS_DISPENSING, 0);
            SetTimeout(DEFAULT_TIMEOUT);
            if (ret)
            {
                var (ack, value0, value1) = ReceivePacket8_2();
                if (ack == PACKET_ACK_OK)
                {
                    return (value0 != 0, value1 != 0);
                }

                if (ack == PACKET_ACK_TIMEOUT)
                {
                    return (null, null);
                }
            }

            return (true, false);
        }

        public bool UpdateLiquidLevels()
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket8(DEST_BROADCAST, PACKET_UPDATE_LIQUID_LEVEL, 0);
        }

        public int GetLiquidLevel(int dispenser)
        {
            if (_softwareOnly)
            {
                return 100;
            }

            if (SendPacket8(dispenser, PACKET_LIQUID_LEVEL, 0))
            {
                var (ack, value, _) = ReceivePacket16();
                if (ack == PACKET_ACK_OK)
                {
                    return value;
                }
            }

            return -1;
        }

        public (int low, int @out) GetLiquidLevelThresholds(int dispenser)
        {
            if (_softwareOnly)
            {
                return (0, 0);
            }

            if (SendPacket8(dispenser, PACKET_GET_LIQUID_THRESHOLDS, 0))
            {
                var (ack, low, @out) = ReceivePacket16();
                if (ack == PACKET_ACK_OK)
                {
                    return (low, @out);
                }
            }

            return (-1, -1);
        }

        public bool SetLiquidLevelThresholds(int dispenser, int low, int @out)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket16(dispenser, PACKET_SET_LIQUID_THRESHOLDS, low, @out);
        }

        public int GetDispenserVersion(int dispenser)
        {
            if (_softwareOnly)
            {
                return DISPENSER_DEFAULT_VERSION_SOFTWARE_ONLY;
            }

            if (SendPacket8(dispenser, PACKET_GET_VERSION, 0))
            {
                SetTimeout(100);
                var (ack, ver, _) = ReceivePacket16(true);
                SetTimeout(DEFAULT_TIMEOUT);
                if (ack == PACKET_ACK_OK)
                {
                    return ver;
                }
            }

            return -1;
        }

        public void SetStatusColor(int red, int green, int blue)
        {
            if (_softwareOnly) {}

            // Assuming status is an instance of StatusLED
            // status.SetColor(red, green, blue);
        }

        public int GetSavedTickCount(int dispenser)
        {
            if (_softwareOnly)
            {
                return 0;
            }

            if (SendPacket8(dispenser, PACKET_SAVED_TICK_COUNT, 0))
            {
                var (ack, ticks, _) = ReceivePacket16();
                if (ack == PACKET_ACK_OK)
                {
                    return ticks;
                }
            }

            return -1;
        }

        public bool FlushSavedTickCount()
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket8(DEST_BROADCAST, PACKET_FLUSH_SAVED_TICK_COUNT, 0);
        }

        public bool PatternDefine(int dispenser, int pattern)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket8(dispenser, PACKET_PATTERN_DEFINE, pattern);
        }

        public bool PatternAddSegment(int dispenser, int red, int green, int blue, int steps)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket8(dispenser, PACKET_PATTERN_ADD_SEGMENT, red, green, blue, steps);
        }

        public bool PatternFinish(int dispenser)
        {
            if (_softwareOnly)
            {
                return true;
            }

            return SendPacket8(dispenser, PACKET_PATTERN_FINISH, 0);
        }

        private void Sync(int state)
        {
            if (_softwareOnly) {}

            // Assuming dispenserSelect is an instance of DispenserSelect
            // dispenserSelect.Sync(state);
        }

        private void Select(int dispenser)
        {
            if (_softwareOnly)
            {
                return;
            }

            if (dispenser == 255)
            {
                return;
            }

            var port = _dispenserPorts[dispenser];

            // Assuming dispenserSelect is an instance of DispenserSelect
            // dispenserSelect.Select(port);
        }

        private bool SendPacket(int dest, byte[] packet)
        {
            if (_softwareOnly)
            {
                return true;
            }

            Select(dest);
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();

            ushort crc = 0;
            foreach (var b in packet)
            {
                crc = Crc16Update(crc, b);
            }

            var encoded = Pack7Bit(packet.Concat(BitConverter.GetBytes(crc)).ToArray());
            if (encoded.Length != RAW_PACKET_SIZE)
            {
                _logger.LogError($"send_packet: Encoded packet size is wrong: {encoded.Length} vs {RAW_PACKET_SIZE}");
                return false;
            }

            try
            {
                var t0 = DateTime.Now;
                _serialPort.Write(new byte[]
                        {
                            0xFF,
                            0xFF
                        }.Concat(encoded)
                         .ToArray(),
                    0,
                    RAW_PACKET_SIZE + 2);

                var t1 = DateTime.Now;
                _logger.LogDebug($"packet time: {(t1 - t0).TotalMilliseconds} ms");

                if (dest == DEST_BROADCAST)
                {
                    return true;
                }

                var ch = _serialPort.ReadByte();
                if (ch == -1)
                {
                    _logger.LogError("*** send packet: read timeout");
                    return false;
                }

                var ack = (byte)ch;
                if (ack == PACKET_ACK_OK)
                {
                    return true;
                }

                _logger.LogError($"send_packet: Invalid ACK code {ack}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SerialException: {ex.Message}");
                return false;
            }
        }

        private bool SendPacket8(int dest, int type, int val0, int val1 = 0, int val2 = 0, int val3 = 0)
        {
            var dispenserId = GetDispenserId(dest);
            if (dispenserId < 0)
            {
                return false;
            }

            var packet = new[]
            {
                (byte)dispenserId,
                (byte)type,
                (byte)val0,
                (byte)val1,
                (byte)val2,
                (byte)val3
            };

            return SendPacket(dest, packet);
        }

        private bool SendPacket16(int dest, int type, int val0, int val1)
        {
            var dispenserId = GetDispenserId(dest);
            if (dispenserId < 0)
            {
                return false;
            }

            var packet = new[]
                {
                    (byte)dispenserId,
                    (byte)type
                }.Concat(BitConverter.GetBytes((ushort)val0))
                 .Concat(BitConverter.GetBytes((ushort)val1))
                 .ToArray();

            return SendPacket(dest, packet);
        }

        private bool SendPacket32(int dest, int type, int val)
        {
            var dispenserId = GetDispenserId(dest);
            if (dispenserId < 0)
            {
                return false;
            }

            var packet = new[]
                {
                    (byte)dispenserId,
                    (byte)type
                }.Concat(BitConverter.GetBytes(val))
                 .ToArray();

            return SendPacket(dest, packet);
        }

        private (int ack, byte[] packet) ReceivePacket(bool quiet = false)
        {
            if (_softwareOnly)
            {
                return (PACKET_ACK_TIMEOUT, []);
            }

            var header = 0;
            while (true)
            {
                var ch = _serialPort.ReadByte();
                if (ch == -1)
                {
                    if (!quiet)
                    {
                        _logger.LogError("receive packet: response timeout");
                    }

                    return (PACKET_ACK_TIMEOUT, []);
                }

                if (ch == 0xFF)
                {
                    header++;
                }
                else
                {
                    header = 0;
                }

                if (header == 2)
                {
                    break;
                }
            }

            var ack = PACKET_ACK_OK;
            var rawPacket = new byte[RAW_PACKET_SIZE];
            var bytesRead = _serialPort.Read(rawPacket, 0, RAW_PACKET_SIZE);
            if (bytesRead != RAW_PACKET_SIZE)
            {
                if (!quiet)
                {
                    _logger.LogError("receive packet: timeout");
                }

                ack = PACKET_ACK_TIMEOUT;
            }

            var packet = Array.Empty<byte>();

            if (ack == PACKET_ACK_OK)
            {
                packet = Pack7Bit(rawPacket);
                if (packet.Length != PACKET_SIZE)
                {
                    ack = PACKET_ACK_INVALID;
                    if (!quiet)
                    {
                        _logger.LogError("receive_packet: Unpacked length incorrect");
                    }
                }

                if (ack == PACKET_ACK_OK)
                {
                    var receivedCrc = BitConverter.ToUInt16(packet, 6);
                    packet = packet.Take(6).ToArray();

                    ushort crc = 0;
                    foreach (var b in packet)
                    {
                        crc = Crc16Update(crc, b);
                    }

                    if (receivedCrc != crc)
                    {
                        if (!quiet)
                        {
                            _logger.LogError("receive_packet: CRC fail");
                        }

                        ack = PACKET_ACK_CRC_FAIL;
                    }
                }
            }

            try
            {
                _serialPort.Write([
                        (byte)ack
                    ],
                    0,
                    1);
            }
            catch (Exception)
            {
                if (!quiet)
                {
                    _logger.LogError("receive_packet: Send ack timeout!");
                }

                ack = PACKET_ACK_TIMEOUT;
            }

            return (ack, ack == PACKET_ACK_OK ? packet : []);
        }

        private (int ack, int value) ReceivePacket8(bool quiet = false)
        {
            var (ack, packet) = ReceivePacket(quiet);
            if (ack == PACKET_ACK_OK)
            {
                return (ack, packet[2]);
            }

            return (ack, 0);
        }

        private (int ack, int value0, int value1) ReceivePacket8_2(bool quiet = false)
        {
            var (ack, packet) = ReceivePacket(quiet);
            if (ack == PACKET_ACK_OK)
            {
                return (ack, packet[2], packet[3]);
            }

            return (ack, 0, 0);
        }

        private (int ack, int value0, int value1) ReceivePacket16(bool quiet = false)
        {
            var (ack, packet) = ReceivePacket(quiet);
            if (ack == PACKET_ACK_OK)
            {
                int value0 = BitConverter.ToUInt16(packet, 2);
                int value1 = BitConverter.ToUInt16(packet, 4);
                return (ack, value0, value1);
            }

            return (ack, 0, 0);
        }

        private void ClearStartupLog()
        {
            _startupLog = string.Empty;
        }

        private void LogStartup(string txt)
        {
            _logger.LogInformation(txt);
            _startupLog += $"{txt}\n";
        }

        private int GetDispenserId(int dest)
        {
            if (dest != DEST_BROADCAST)
            {
                try
                {
                    return _dispenserIds[dest];
                }
                catch (IndexOutOfRangeException)
                {
                    _logger.LogError($"*** send_packet to dispenser {dest + 1} (of {_dispenserIds.Length} dispensers)");
                    return 255;
                }
            }

            return dest;
        }

        private ushort Crc16Update(ushort crc, byte a)
        {
            crc ^= a;
            for (var i = 0; i < 8; i++)
            {
                if ((crc & 1) != 0)
                {
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                }
                else
                {
                    crc >>= 1;
                }
            }

            return crc;
        }

        private byte[] Pack7Bit(byte[] data)
        {
            // Implement the 7-bit packing logic here
            // This is a placeholder implementation
            return data;
        }
    }

    public class SerialIOException : Exception
    {
        public SerialIOException(string message) : base(message) {}
    }

    // Placeholder classes for StatusLED and DispenserSelect
    public class StatusLED
    {
        public StatusLED(bool softwareOnly) {}
        public void SetColor(int red, int green, int blue) {}
        public void SwapBlueGreen() {}
    }

    public class DispenserSelect
    {
        public DispenserSelect(int maxDispensers, bool softwareOnly) {}
        public void Open() {}
        public void Reset() {}
        public void Select(int port) {}
        public void Sync(int state) {}
    }
}