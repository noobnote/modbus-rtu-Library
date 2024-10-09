using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    public abstract class TaskObject
    {
        public TaskObject(PortInfo portInfo, int host, int startAddress)
        {
            PortInfo = portInfo;
            Host = host;
            StartAddress = startAddress;
        }
        public PortInfo PortInfo { get; set; }
        public int Host { get; set; }
        public int StartAddress { get; set; }
    }

}
