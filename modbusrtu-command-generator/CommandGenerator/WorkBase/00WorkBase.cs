using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.CommandGenerator.WorkBase
{
    public abstract class WorkBase
    {
        /// <summary>站号
        /// 
        /// </summary>
        public abstract byte Host { get; set; }
        /// <summary>功能码
        /// 
        /// </summary>
        public abstract byte FunctionCode { get; set; }
        /// <summary>起始地址
        /// 
        /// </summary>
        public abstract ushort StartAddress { get; set; }
        /// <summary>命令
        /// 
        /// </summary>
        public abstract byte[] Command { get; }
        /// <summary>创建命令
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract byte[] CreateCommand();
    }
}
