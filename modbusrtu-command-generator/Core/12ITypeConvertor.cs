using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    /// <summary>类型转换器
    /// 
    /// </summary>
    public interface ITypeConvertor<T>
    {
        /// <summary>基本单元占用字节数
        /// 
        /// </summary>
        int ByteSize { get; }
        /// <summary>转换
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        T Convert(byte[] bytes);
    }
}
