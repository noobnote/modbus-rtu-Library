using ModbusLibrary.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.TypeConverters
{
    public class Int32ArrayConvertor : TypeConvertor
    {
        public override Type TargetType { get { return typeof(int[]); } }
        public override object Convert(byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
            {
                throw new Exception($"输入的数组长度为{bytes.Length}，无法转换为{TargetType.Name}");
            }


            List<int> ints = new List<int>();
            for (int index = 0; index < bytes.Length; index += 4)
            {
                ints.Add(BitConverter.ToInt32(bytes.Skip(index).Take(4).ToArray(), 0));
            }
            return ints.ToArray();
        }
    }
}
