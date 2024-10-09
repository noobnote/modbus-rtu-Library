using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    /// <summary>类型安全的转换器注册机
    /// 
    /// </summary>
    public interface IThreadSafeConvertorRegistry
    {
        /// <summary>注册类型转换器
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeConvertor"></param>
        void RegistTypeConverter<T>(ITypeConvertor<T> typeConvertor);
    }
}
