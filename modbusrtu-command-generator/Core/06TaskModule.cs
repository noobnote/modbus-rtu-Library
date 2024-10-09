using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    /// <summary>任务模块
    /// 
    /// </summary>
    public abstract class TaskModule
    {
        /// <summary>站号
        /// 
        /// </summary>
        public abstract byte Host { get; set; }
        /// <summary>功能码
        /// 
        /// </summary>
        public abstract byte FunctionCode { get; set; }
        /// <summary>起始地址
        /// 
        /// </summary>
        public abstract ushort StartAddress { get; set; }
        /// <summary>可等待句柄
        /// 
        /// </summary>
        public abstract ManualResetEvent WaitHandle { get; set; }
        /// <summary>命令
        /// 
        /// </summary>
        public abstract byte[] Command { get; }
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
