using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    public class TaskForWrite : TaskBase
    {
        public TaskForWrite(byte[] values, int functionCode, PortInfo portInfo, int host, int startAddress)
            : base(functionCode, portInfo, host, startAddress)
        {
            Values = values;
        }
        public byte[] Values { get; set; }
        public override byte[] CreateCommand()
        {
            //分别为不同功能码创建命令
            return null;
        }
        public override int GetExpectedByteCount()
        {
            //视具体功能码而定不同功能码
            return default(int);
        }
    }
}
