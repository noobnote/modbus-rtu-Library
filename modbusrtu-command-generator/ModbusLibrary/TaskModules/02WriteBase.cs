using modbusrtu_command_generator.ModbusLibrary.ModbusCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace modbusrtu_command_generator
{
    public class WriteBase : TaskModule
    {
        private byte[] _command;
        public override byte Host { get; set; }
        public override byte FunctionCode { get; set; }
        public override ushort StartAddress { get; set; }
        /// <summary>写入的（寄存器/线圈）数量
        /// 
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>写入的字节数量
        /// 
        /// </summary>
        public int ByteQuantity { get; set; }
        /// <summary>写入的值
        /// 
        /// </summary>
        public IEnumerable<byte> Values { get; set; }
        /// <summary>写入完成时刻
        /// 
        /// </summary>
        public DateTime CompleteTime { get; set; }
        public override byte[] Command { get { return this._command; } }
        public override ManualResetEvent WaitHandle { get; set; }
        public WriteBase(byte host, byte functionCode, ushort startAddress, ushort quantity)
        {
            this.Host = host;
            this.FunctionCode = functionCode;
            this.StartAddress = startAddress;
            this.Quantity = quantity;
        }

        public override byte[] CreateCommand()
        {
            //注意，这里只实现了写单个地址的
            if (Command == null)
            {
                byte[] bytes = new byte[6];
                bytes[0] = Host;
                bytes[1] = FunctionCode;
                bytes[2] = (byte)(StartAddress / (1 << 8));
                bytes[3] = (byte)(StartAddress % (1 << 8));
                bytes[4] = (byte)(ByteQuantity / (1 << 8));
                bytes[5] = (byte)(ByteQuantity % (1 << 8));

                ushort crc = ModbusCrc16Calculator.CalculateCRC16(bytes);

                byte[] command = new byte[bytes.Length + 2];
                command[command.Length - 2] = (byte)(crc / 256);
                command[command.Length - 1] = (byte)(crc % 256);

                Array.Copy(bytes, 0, command, 0, bytes.Length);
                this._command = command;
            }

            return this.Command;
        }

        public override int GetExpectedByteCount()
        {
            throw new NotImplementedException();
        }
    }
}
