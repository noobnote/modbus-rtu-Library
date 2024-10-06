using modbusrtu_command_generator.CommandGenerator.WorkBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.ModbusLibrary.ModbusCore
{
    public static class ModbusManager
    {
        //添加任务（周期、即时）

        //取值

        //帮助类（生成命令、从返回报文中拿取值、转换值）

        public static void AddPeriodicTask(DeviceInfo deviceInfo, ReadBase readTask)
        { 
        
        }

        public static void RemovePeriodicTask(DeviceInfo deviceInfo, ReadBase readTask)
        { 
        
        }

        public static void AddImmediateTask(DeviceInfo deviceInfo, ReadBase readTask)
        {


        }


        public static T GetValues<T>()
        {
            //字典查找转换器，然后执行转换
            //或者再重载一个GetValues(),由调用者在参数列表中指定一个转换器（但是同时，也必须指定需要从缓冲区中取多少个地址）
            return default(T);
        }
    }
}
