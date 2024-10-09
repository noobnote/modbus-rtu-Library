using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.ModbusLibrary.ModbusCore
{
    /// <summary>类型转换器
    /// 
    /// </summary>
    public interface ITypeConvertor
    {
        /// <summary>转换的目标类型
        /// 
        /// </summary>
        Type TargetType { get; }
        /// <summary>目标类型占用字节数
        /// 
        /// </summary>
        int ByteSize { get; }
        /// <summary>转换
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        object Convert(byte[] bytes);
    }

    public class TypeConvertor : ITypeConvertor
    {
        public virtual Type TargetType { get { return null; } }

        public virtual int ByteSize { get { return -1; } }

        public virtual object Convert(byte[] bytes)
        {
            return null;
        }
    }
}
