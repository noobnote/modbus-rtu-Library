using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Exceptions
{
    public class PortConfigConflictException : Exception
    {
        public PortConfigConflictException() : base("串口已被打开且参数不一致")
        {

        }
    }
}
