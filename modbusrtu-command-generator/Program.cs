using modbusrtu_command_generator.ModbusLibrary.ModbusCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator
{
    internal class Program
    {
        static Dictionary<DeviceInfo, List<TaskModule>> keyValuePairs { get; set; } = new Dictionary<DeviceInfo, List<TaskModule>>();
        static void Main(string[] args)
        {
            //示例
            DeviceInfo deviceInfo = DeviceInfo.Create(port: 1,
                host: 1,
                name: "温控仪",
                baudrate: 9600,
                stopBits: System.IO.Ports.StopBits.None,
                dataBits: 8,
                parity: System.IO.Ports.Parity.Odd);

            List<TaskModule> taskModules = new List<TaskModule>();
            taskModules.Add(new ReadBase(deviceInfo.Host, 3, 0x0000, 1));
            taskModules.Add(new ReadBase(deviceInfo.Host, 3, 0x0100, 4));
            keyValuePairs.Add(deviceInfo, taskModules);


            
            //添加周期任务
            ModbusManager.AddPeriodicTask(deviceInfo, keyValuePairs[deviceInfo][0]);
            ModbusManager.AddPeriodicTask(deviceInfo, keyValuePairs[deviceInfo][1]);


            //取值
            var value = ModbusManager.GetValues<int>(deviceInfo, keyValuePairs[deviceInfo][0], 8980990);

            Console.ReadKey();
        }
    }

}
