using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Exceptions
{
    /// <summary>类型转换异常
    /// 
    /// </summary>
    public class TypeConversionException : Exception
    {
        public Type TargetType { get; private set; }
        public TypeConversionException(Type targetType) : base($"类型转换异常，目标类型：{targetType.Name}")
        {
            TargetType = targetType;
        }
    }
}
