using modbusrtu_command_generator.CommandGenerator.WorkBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.ModbusLibrary.ModbusCore
{
    public static class ModbusManager
    {
        //添加任务（周期、即时）

        //取值

        //帮助类（生成命令、从返回报文中拿取值、转换值）

        static object AccessLock { get; set; } = new object();
        static Dictionary<DeviceInfo, AccessPort> AccessPortCollection { get; set; } = new Dictionary<DeviceInfo, AccessPort>();

        
        public static void AddPeriodicTask(DeviceInfo deviceInfo, TaskModule task)
        {
            AccessPort accessPort = RetrieveOrCreate(deviceInfo);
            accessPort.AddPeriodicTask(task);
        }

        public static void RemovePeriodicTask(DeviceInfo deviceInfo, TaskModule task)
        {
            AccessPort accessPort = Retrieve(deviceInfo);
            if (accessPort != null)
            {
                accessPort.RemovePeriodicTask(task);
            }
        }

        public static void AddImmediateTask(DeviceInfo deviceInfo, TaskModule task)
        {
            AccessPort accessPort = RetrieveOrCreate(deviceInfo);
            accessPort.AddImmediateTask(task);
        }

        /// <summary>关闭端口
        /// 
        /// </summary>
        /// <param name="port">端口</param>
        public static void CloseAccessPort(int port)
        {

            lock (AccessLock)
            {
                var pair = AccessPortCollection.FirstOrDefault(cell =>
                {
                    return (cell.Key.Port == port);
                });

                AccessPort accessPort = pair.Value;
                if (accessPort != null)
                {
                    accessPort.ClosePort();
                    AccessPortCollection.Remove(pair.Key);
                }
            }
        }
        private static AccessPort RetrieveOrCreate(DeviceInfo deviceInfo)
        {
            AccessPort accessPort = null;
            lock (AccessLock)
            {
                accessPort = AccessPortCollection.FirstOrDefault(cell =>
                {
                    return (cell.Key.Port == deviceInfo.Port);
                    //return
                    //   (cell.Key.DataBits == deviceInfo.DataBits) &&
                    //   (cell.Key.StopBits == deviceInfo.StopBits) &&
                    //   (cell.Key.Parity == deviceInfo.Parity) &&
                    //   (cell.Key.Port == deviceInfo.Port) &&
                    //   (cell.Key.BaudRate == deviceInfo.BaudRate) &&
                    //   (cell.Key.Name == deviceInfo.Name);
                }).Value;

                if (accessPort == null)
                {
                    accessPort = new AccessPort(deviceInfo);
                    AccessPortCollection.Add(deviceInfo, accessPort);
                }
            }
            return accessPort;
        }

        private static AccessPort Retrieve(DeviceInfo deviceInfo)
        {
            AccessPort accessPort = null;
            lock (AccessLock)
            {
                accessPort = AccessPortCollection.FirstOrDefault(cell =>
                {
                    return (cell.Key.Port == deviceInfo.Port);
                    //return
                    //   (cell.Key.DataBits == deviceInfo.DataBits) &&
                    //   (cell.Key.StopBits == deviceInfo.StopBits) &&
                    //   (cell.Key.Parity == deviceInfo.Parity) &&
                    //   (cell.Key.Port == deviceInfo.Port) &&
                    //   (cell.Key.BaudRate == deviceInfo.BaudRate) &&
                    //   (cell.Key.Name == deviceInfo.Name);
                }).Value;
            }
            return accessPort;
        }

        //static AccessPort FindAccessPort(DeviceInfo deviceInfo)
        //{
        //    lock (AccessLock)
        //    {
        //        return keyValuePairs.FirstOrDefault(cell =>
        //        {
        //            return
        //               (cell.Key.DataBits == deviceInfo.DataBits) &&
        //               (cell.Key.StopBits == deviceInfo.StopBits) &&
        //               (cell.Key.Parity == deviceInfo.Parity) &&
        //               (cell.Key.Port == deviceInfo.Port) &&
        //               (cell.Key.BaudRate == deviceInfo.BaudRate) &&
        //               (cell.Key.Name == deviceInfo.Name);
        //        }).Value;
        //    }
        //}
        //static AccessPort FindAccessPort(int port)
        //{
        //    lock (AccessLock)
        //    {
        //        return keyValuePairs.FirstOrDefault(cell =>
        //        {
        //            return (cell.Key.Port == port);
        //        }).Value;
        //    }
        //}


        public static T GetValues<T>()
        {
            //字典查找转换器，然后执行转换
            //或者再重载一个GetValues(),由调用者在参数列表中指定一个转换器（但是同时，也必须指定需要从缓冲区中取多少个地址）
            return default(T);
        }
    }
}
