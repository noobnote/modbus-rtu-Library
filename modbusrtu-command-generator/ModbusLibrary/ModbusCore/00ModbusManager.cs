using modbusrtu_command_generator.ModbusLibrary.RawDataProcessors;
using modbusrtu_command_generator.ModbusLibrary.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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


        static object TypeConvertorCollectionLock { get; set; } = new object();
        static Dictionary<Type, ITypeConvertor> _typeConvertorCollection;
        static Dictionary<Type, ITypeConvertor> TypeConvertorCollection
        {
            get
            {
                if (_typeConvertorCollection == null)
                {
                    _typeConvertorCollection = new Dictionary<Type, ITypeConvertor>();

                    List<ITypeConvertor> list = new List<ITypeConvertor>()
                    {
                        new Int32ArrayConvertor(),
                        new Int32Convertor(),
                        //其它类型转换器 待扩展
                    };

                    foreach (var convert in list)
                    {
                        _typeConvertorCollection.Add(convert.TargetType, convert);
                    }
                }
                return _typeConvertorCollection;
            }
            set { _typeConvertorCollection = value; }
        }


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

        //public static void AddImmediateTask(DeviceInfo deviceInfo, TaskModule task)
        //{
        //    AccessPort accessPort = RetrieveOrCreate(deviceInfo);
        //    accessPort.AddImmediateTask(task);
        //}

        public static WaitHandle RunOnceAsync(DeviceInfo deviceInfo, TaskModule task)
        {
            AccessPort accessPort = RetrieveOrCreate(deviceInfo);
            return accessPort.AddImmediateTask(task);
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

        /// <summary>注册一个原始数据处理器
        /// 
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="rawDataProcessor">数据处理器</param>
        public static void RegistRawDataProcessor(DeviceInfo deviceInfo, IRawDataProcessor<IEnumerable<byte>> rawDataProcessor)
        {
            //内部类慎用。与使用了AccessLock的方法组合使用时，有死锁的可能
            lock (AccessLock)
            {
                var pair = AccessPortCollection.FirstOrDefault(cell =>
                {
                    return (cell.Key.Port == deviceInfo.Port);
                });

                AccessPort accessPort = pair.Value;
                if (accessPort != null)
                {
                    accessPort.RegistRawDataProcessor(rawDataProcessor);
                }
            }
        }

        /// <summary>预设原始数据处理器
        /// 
        /// </summary>
        /// <param name="accessPort"></param>
        private static void ConfigureRawDataProcessors(this AccessPort accessPort)
        {
            accessPort?.RegistRawDataProcessor(new ProcessorFor03());
            //其它数据处理器待扩展
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
                    accessPort.ConfigureRawDataProcessors();
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


        /// <summary>注册一个类型转换器
        /// 
        /// </summary>
        public static void RegistTypeConverter(ITypeConvertor typeConvertor)
        {
            lock (TypeConvertorCollectionLock)
            {
                if (!TypeConvertorCollection.ContainsKey(typeConvertor.TargetType))
                {
                    TypeConvertorCollection.Add(typeConvertor.TargetType, typeConvertor);
                }
            }
        }




        /// <summary>取值
        /// 
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="deviceInfo">设备信息</param>
        /// <param name="host">设备站号</param>
        /// <param name="func">功能码</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="byteSize">需读取字节数</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetValues<T>(DeviceInfo deviceInfo, int host, int func, int startAddress, int byteSize)
        {
            ITypeConvertor convertor = null;
            lock (TypeConvertorCollectionLock)
            {
                if (TypeConvertorCollection.ContainsKey(typeof(T)))
                {
                    convertor = TypeConvertorCollection[typeof(T)];
                    //return (T)TypeConvertorCollection[typeof(T)].Convert(new byte[2]);
                }
            }
            if (convertor != null)
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

                if (accessPort == null)
                {
                    throw new Exception("找不到设备对应端口");
                }
                byte[] bytes;
                if (convertor.ByteSize > 0)
                {
                    bytes = accessPort.GetValues(host, func, startAddress, convertor.ByteSize);
                }
                else
                { 
                    bytes = accessPort.GetValues(host, func, startAddress, byteSize);
                }

                return (T)convertor.Convert(bytes);
            }

            return default(T);
        }

        /// <summary>取值
        /// 
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="deviceInfo">设备信息</param>
        /// <param name="taskModule">任务</param>
        /// <param name="byteSize">需读取字节数</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetValues<T>(DeviceInfo deviceInfo, TaskModule taskModule, int byteSize)
        {
            ITypeConvertor convertor = null;
            lock (TypeConvertorCollectionLock)
            {
                if (TypeConvertorCollection.ContainsKey(typeof(T)))
                {
                    convertor = TypeConvertorCollection[typeof(T)];
                }
            }
            if (convertor != null)
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

                if (accessPort == null)
                {
                    throw new Exception("找不到设备对应端口");
                }

                byte[] bytes;
                if (convertor.ByteSize > 0)
                {
                    bytes = accessPort.GetValues(taskModule, convertor.ByteSize);
                }
                else
                {
                    bytes = accessPort.GetValues(taskModule, byteSize);
                }

                return (T)convertor.Convert(bytes);
            }

            return default(T);
        }

        /// <summary>取值
        /// 
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="deviceInfo">设备信息</param>
        /// <param name="taskModule">任务</param>
        /// <param name="byteSize">需读取字节数</param>
        /// <param name="convertor">类型转换器</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetValues<T>(DeviceInfo deviceInfo, TaskModule taskModule, int byteSize, ITypeConvertor convertor)
        {
            if (convertor != null)
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

                if (accessPort == null)
                {
                    throw new Exception("找不到设备对应端口");
                }

                byte[] bytes;
                if (convertor.ByteSize > 0)
                {
                    bytes = accessPort.GetValues(taskModule, convertor.ByteSize);
                }
                else
                {
                    bytes = accessPort.GetValues(taskModule, byteSize);
                }

                return (T)convertor.Convert(bytes);
            }

            return default(T);
        }
    }

}
