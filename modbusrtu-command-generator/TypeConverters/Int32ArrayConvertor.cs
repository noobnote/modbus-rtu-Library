using ModbusLibrary.Core;
using ModbusLibrary.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.TypeConverters
{
    public class Int32ArrayConvertor : ITypeConvertor<int[]>
    {
        public int ByteSize { get { return 4; } }

        public int[] Convert(byte[] bytes)
        {
            if (bytes.Length % ByteSize != 0)
            {
                throw new TypeConversionException(typeof(int[]));
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
