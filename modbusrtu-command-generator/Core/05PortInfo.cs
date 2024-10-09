using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class PortInfo  
    {
        /// <summary>COM端口
        /// 
        /// </summary>
        public string Port { get; private set; }
        /// <summary>波特率
        /// 
        /// </summary>
        public int BaudRate { get; private set; }
        /// <summary>停止位
        /// 
        /// </summary>
        public StopBits StopBits { get; private set; }
        /// <summary>数据位
        /// 
        /// </summary>
        public int DataBits { get; private set; }
        /// <summary>校验位
        /// 
        /// </summary>
        public Parity Parity { get; private set; }
        public PortInfo(string port, int baudrate, StopBits stopBits, int dataBits, Parity parity)
        {

            this.Port = port;
            this.BaudRate = baudrate;
            this.StopBits = stopBits;
            this.Parity = parity;
            this.DataBits = dataBits;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            return this.GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            return (Port, BaudRate, StopBits, Parity, DataBits).GetHashCode();
        }
    }
}
