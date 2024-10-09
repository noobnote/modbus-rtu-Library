using ModbusLibrary.RawDataProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    public static class AccessPortsManagerHelper
    {
        /// <summary>预设原始数据处理器
        /// 
        /// </summary>
        /// <param name="accessPort"></param>
        public static void ConfigureRawDataProcessors(this AccessPort accessPort)
        {
            accessPort?.RegistRawDataProcessor(new ProcessorFor03());
            //其它数据处理器待扩展
        }

        public static void ConfigureDefaultTypeConverter()
        { 
        
        }
    }
}
