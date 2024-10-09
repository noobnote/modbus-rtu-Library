using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{

    public abstract class TaskBase : TaskObject
    {
        public TaskBase(int functionCode, PortInfo portInfo, int host, int startAddress)
            : base(portInfo, host, startAddress)
        {
            FunctionCode = functionCode;
        }
        public int FunctionCode { get; set; }
        public EventWaitHandle WaitHandle { get; set; }
        /// <summary>创建命令
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract byte[] CreateCommand();
        /// <summary>获取远程主机应当反馈的完整报文的字节数
        /// 
        /// </summary>
        public abstract int GetExpectedByteCount();
    }
}
