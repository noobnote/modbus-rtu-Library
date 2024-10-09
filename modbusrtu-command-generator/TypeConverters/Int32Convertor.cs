using ModbusLibrary.Core;
using ModbusLibrary.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.TypeConverters
{
    public class Int32Convertor : ITypeConvertor<Int32>
    {
        public int ByteSize { get { return 4; } }
        public int Convert(byte[] bytes)
        {
            if (bytes.Length % ByteSize != 0)
            {
                throw new TypeConversionException(typeof(int));
            }

            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
