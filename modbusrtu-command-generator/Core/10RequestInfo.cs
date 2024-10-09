using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    public class RequestInfo
    {
        public PortInfo PortInfo { get; set; }
        public int Host { get; set; }
        public DataMemory.MemoryArea MemoryArea { get; set; }
        public int StartAddress { get; set; }
    }
}
