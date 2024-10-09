using ModbusLibrary.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.RawDataProcessors
{
    public class ProcessorFor03 : IRawDataProcessor<IEnumerable<byte>>
    {
        public byte TargetFunc { get { return 0x03; } }

        public IEnumerable<byte> CaptureData(IEnumerable<byte> rawData)
        {
            var raw = rawData.ToArray();
            if (raw[2] != this.TargetFunc)
            {
                throw new ArgumentException("原始报文功能码与本处理器的目标功能码不符，无法处理");
            }
            int dataCount = raw[3];
            return raw.Skip(3).Take(dataCount);
        }

        public bool Validate(IEnumerable<byte> sentMessage, IEnumerable<byte> receivedMessage)
        {
            //功能码校验
            var sent = sentMessage.ToArray();
            if (sent[2] != this.TargetFunc)
            {
                throw new ArgumentException("原始报文功能码与本处理器的目标功能码不符，无法处理");
            }

            //报文长度校验
            int ExpectedByteCount = 5 + BitConverter.ToUInt16(sent, 4);
            var recived = receivedMessage.ToArray();
            if (recived.Length != ExpectedByteCount)
            {
                return false;
            }

            //校验码比较
            ushort expected = ModbusCrc16Calculator.CalculateCRC16(recived.Take(recived.Length - 2).ToArray());
            if (expected != BitConverter.ToInt16(recived.Skip(recived.Length - 2).ToArray(), 0))
            {
                return false;
            }

            return true;
        }

        public bool Validate(IEnumerable<byte> sentMessage, IEnumerable<byte> receivedMessage, Action<Exception> handleError)
        {
            //功能码校验
            var sent = sentMessage.ToArray();
            if (sent[2] != this.TargetFunc)
            {
                handleError?.Invoke(new ArgumentException("原始报文功能码与本处理器的目标功能码不符，无法处理"));
            }

            //报文长度校验
            int ExpectedByteCount = 5 + BitConverter.ToUInt16(sent, 4);
            var recived = receivedMessage.ToArray();
            if (recived.Length != ExpectedByteCount)
            {
                handleError?.Invoke(new Exception("应当返回的报文长度异常"));
                return false;
            }

            //校验码比较
            ushort expected = ModbusCrc16Calculator.CalculateCRC16(recived.Take(recived.Length - 2).ToArray());
            if (expected != BitConverter.ToInt16(recived.Skip(recived.Length - 2).ToArray(), 0))
            {
                handleError?.Invoke(new Exception("应当返回的报文校验码异常"));
                return false;
            }

            return true;
        }
    }
}
