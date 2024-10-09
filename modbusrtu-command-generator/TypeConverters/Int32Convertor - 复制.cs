using ModbusLibrary.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.TypeConverters
{
    public class Int32Convertor : TypeConvertor
    {
        public override Type TargetType { get { return typeof(int); } }
        public override int ByteSize { get { return 4; } }
        public override object Convert(byte[] bytes)
        {
            if (bytes.Length % ByteSize != 0)
            {
                throw new Exception($"输入的数组长度为{bytes.Length}，无法转换为{TargetType.Name}");
            }

            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
