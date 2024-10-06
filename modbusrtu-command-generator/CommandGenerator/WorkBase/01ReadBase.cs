using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.CommandGenerator.WorkBase
{
    public class ReadBase : WorkBase
    {
        private byte[] _command;
        public override byte Host { get; set; }
        public override byte FunctionCode { get; set; }
        public override ushort StartAddress { get; set; }
        /// <summary>读取地址数
        /// 
        /// </summary>
        public ushort Quantity { get; set; }
        /// <summary>数据
        /// 
        /// </summary>
        public IEnumerable<byte> Data { get; set; }
        /// <summary>读取时刻
        /// 
        /// </summary>
        public DateTime ReceiveTime { get; set; }
        public override byte[] Command { get { return this._command; } }
        public ReadBase(byte host, byte functionCode, ushort startAddress, ushort quantity)
        {
            this.Host = host;
            this.FunctionCode = functionCode;
            this.StartAddress = startAddress;
            this.Quantity = quantity;
        }

        public override byte[] CreateCommand()
        {
            if (Command == null)
            {
                byte[] bytes = new byte[6];
                bytes[0] = Host;
                bytes[1] = FunctionCode;
                bytes[2] = (byte)(StartAddress / (1 << 8));
                bytes[3] = (byte)(StartAddress % (1 << 8));
                bytes[4] = (byte)(Quantity / (1 << 8));
                bytes[5] = (byte)(Quantity % (1 << 8));

                ushort crc = ModbusCrc16.CalculateCRC16(bytes);

                byte[] command = new byte[bytes.Length + 2];
                command[command.Length - 2] = (byte)(crc / 256);
                command[command.Length - 1] = (byte)(crc % 256);

                Array.Copy(bytes, 0, command, 0, bytes.Length);
                this._command = command;
            }

            return this.Command;
        }
        /// <summary>获取远程主机应当反馈的完整报文的字节数
        /// 
        /// </summary>
        public virtual int GetExpectedByteCount()
        {
            //这里只实现了0x03的，其它功能码的需要继承此类后重写该方法

            int count = 0;
            if (FunctionCode == 0x03)
            {
                count = 5 + Quantity * 2;
            }
            return count;

        }
    }
}
