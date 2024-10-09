using ModbusLibrary.RawDataProcessors;
using ModbusLibrary.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    public sealed class SerialPortsManager
    {
        private static object Lock { get; } = new object();
        private static volatile SerialPortsManager _instance;
        public static SerialPortsManager Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SerialPortsManager();
                    }
                }
                return _instance;
            }
        }

        private SerialPortsManager() { }
        private object AccessLock { get; set; } = new object();
        private Dictionary<PortInfo, AccessPort> AccessPortCollection { get; set; } = new Dictionary<PortInfo, AccessPort>();


        private object TypeConvertorCollectionLock { get; set; } = new object();
        private Dictionary<Type, ITypeConvertor> _typeConvertorCollection;
        private Dictionary<Type, ITypeConvertor> TypeConvertorCollection
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

        /// <summary>添加周期任务
        /// 
        /// </summary>
        /// <param name="PortInfo"></param>
        /// <param name="task"></param>
        public async Task AddPeriodicTaskAsync(PortInfo PortInfo, TaskModule task)
        {
            try
            {
                AccessPort accessPort = RetrieveOrCreate(PortInfo);
                await accessPort.AddPeriodicTaskAsync(task);
            }
            catch (PortConfigConflictException)
            {
                throw;
            }
        }

        public void RemovePeriodicTask(PortInfo PortInfo, TaskModule task)
        {
            AccessPort accessPort = Retrieve(PortInfo);
            accessPort?.RemovePeriodicTaskAsync(task);
        }


        public async Task<WaitHandle> RunOnceAsync(PortInfo PortInfo, TaskModule task)
        {
            try
            {
                AccessPort accessPort = RetrieveOrCreate(PortInfo);
                return await accessPort.AddImmediateTaskAsync(task);
            }
            catch (PortConfigConflictException)
            {
                throw;
            }
        }

        /// <summary>关闭端口
        /// 
        /// </summary>
        /// <param name="port">端口</param>
        public void CloseAccessPort(string port)
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
        /// <param name="PortInfo"></param>
        /// <param name="rawDataProcessor">数据处理器</param>
        public void RegistRawDataProcessor(PortInfo PortInfo, IRawDataProcessor<IEnumerable<byte>> rawDataProcessor)
        {
            //内部类慎用。与使用了AccessLock的方法组合使用时，有死锁的可能
            lock (AccessLock)
            {
                var pair = AccessPortCollection.FirstOrDefault(cell =>
                {
                    return (cell.Key.Port == PortInfo.Port);
                });
                pair.Value?.RegistRawDataProcessor(rawDataProcessor);
            }
        }



        //检索或创建端口
        private AccessPort RetrieveOrCreate(PortInfo PortInfo)
        {
            //首先串口是否已启用？
            //未启用-创建
            //已启用-检查参数是否一致:
            //      参数一致：返回实例
            //      参数不一致：报错


            AccessPort accessPort = null;
            lock (AccessLock)
            {
                var result = from cell in AccessPortCollection where cell.Key.Port == PortInfo.Port select cell;

                if (result.Count() <= 0)
                {
                    //直接创建
                    accessPort = new AccessPort(PortInfo);
                    accessPort.ConfigureRawDataProcessors();
                    AccessPortCollection.Add(PortInfo, accessPort);
                }
                else
                {
                    var pair = result.First();
                    if (pair.Key.Equals(PortInfo))
                    {
                        accessPort = pair.Value;
                    }
                    else
                    {
                        //端口一致而参数不一致
                        throw new PortConfigConflictException();
                    }
                }
            }
            return accessPort;
        }

        private AccessPort Retrieve(PortInfo PortInfo)
        {
            AccessPort accessPort = null;
            lock (AccessLock)
            {
                var result = from cell in AccessPortCollection where cell.Key.Port == PortInfo.Port select cell;
                if (result.Count() > 0)
                {
                    accessPort = result.First().Value;
                }
            }
            return accessPort;
        }


        /// <summary>注册一个类型转换器
        /// 
        /// </summary>
        public void RegistTypeConverter(ITypeConvertor typeConvertor)
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
        /// <param name="PortInfo">设备信息</param>
        /// <param name="host">设备站号</param>
        /// <param name="func">功能码</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="byteSize">需读取字节数</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T GetValues<T>(PortInfo PortInfo, int host, int func, int startAddress, int byteSize)
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
                        return (cell.Key.Port == PortInfo.Port);
                        //return
                        //   (cell.Key.DataBits == PortInfo.DataBits) &&
                        //   (cell.Key.StopBits == PortInfo.StopBits) &&
                        //   (cell.Key.Parity == PortInfo.Parity) &&
                        //   (cell.Key.Port == PortInfo.Port) &&
                        //   (cell.Key.BaudRate == PortInfo.BaudRate) &&
                        //   (cell.Key.Name == PortInfo.Name);
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
        /// <param name="PortInfo">设备信息</param>
        /// <param name="taskModule">任务</param>
        /// <param name="byteSize">需读取字节数</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T GetValues<T>(PortInfo PortInfo, TaskModule taskModule, int byteSize)
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
                        return (cell.Key.Port == PortInfo.Port);
                        //return
                        //   (cell.Key.DataBits == PortInfo.DataBits) &&
                        //   (cell.Key.StopBits == PortInfo.StopBits) &&
                        //   (cell.Key.Parity == PortInfo.Parity) &&
                        //   (cell.Key.Port == PortInfo.Port) &&
                        //   (cell.Key.BaudRate == PortInfo.BaudRate) &&
                        //   (cell.Key.Name == PortInfo.Name);
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
        /// <param name="PortInfo">设备信息</param>
        /// <param name="taskModule">任务</param>
        /// <param name="byteSize">需读取字节数</param>
        /// <param name="convertor">类型转换器</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T GetValues<T>(PortInfo PortInfo, TaskModule taskModule, int byteSize, ITypeConvertor convertor)
        {
            if (convertor != null)
            {
                AccessPort accessPort = null;
                lock (AccessLock)
                {
                    accessPort = AccessPortCollection.FirstOrDefault(cell =>
                    {
                        return (cell.Key.Port == PortInfo.Port);
                        //return
                        //   (cell.Key.DataBits == PortInfo.DataBits) &&
                        //   (cell.Key.StopBits == PortInfo.StopBits) &&
                        //   (cell.Key.Parity == PortInfo.Parity) &&
                        //   (cell.Key.Port == PortInfo.Port) &&
                        //   (cell.Key.BaudRate == PortInfo.BaudRate) &&
                        //   (cell.Key.Name == PortInfo.Name);
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



    public class PortConfigConflictException: Exception 
    {
        public PortConfigConflictException() : base("串口已被打开且参数不一致")
        {

        }
    }

}
