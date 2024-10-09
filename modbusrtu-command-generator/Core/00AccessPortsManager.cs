using ModbusLibrary.Exceptions;
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
    public sealed class AccessPortsManager: IThreadSafeConvertorRegistry
    {
        private static object Lock { get; } = new object();
        private static volatile AccessPortsManager _instance;
        public static AccessPortsManager Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AccessPortsManager();
                    }
                }
                return _instance;
            }
        }

        private AccessPortsManager() { }
        private object AccessLock { get; set; } = new object();
        private Dictionary<PortInfo, AccessPort> AccessPortCollection { get; set; } = new Dictionary<PortInfo, AccessPort>();


        private object TypeConvertorCollectionLock { get; set; } = new object();
        private volatile Dictionary<Type, object> _typeConvertorCollection;
        private Dictionary<Type, object> TypeConvertorCollection
        {
            get
            {
                if (_typeConvertorCollection == null)
                {
                    _typeConvertorCollection = InitializeDic();
                }
                return _typeConvertorCollection;
            }
            set { _typeConvertorCollection = value; }
        }

        private Dictionary<Type, object> InitializeDic()
        {
            var pairs = new Dictionary<Type, object>();
            RegistTypeConverter(pairs, new Int32ArrayConvertor());
            RegistTypeConverter(pairs, new Int32Convertor());
            return pairs;
        }

        public ITypeConvertor<T> FindTypeConvertor<T>()
        {
            ITypeConvertor<T> convertor = null;
            lock (TypeConvertorCollectionLock)
            {
                if (TypeConvertorCollection.ContainsKey(typeof(T)))
                {
                    convertor = (ITypeConvertor<T>)TypeConvertorCollection[typeof(T)];
                }
            }
            return convertor;
        }


        /// <summary>添加周期任务
        /// 
        /// </summary>
        /// <param name="PortInfo"></param>
        /// <param name="task"></param>
        public async Task AddPeriodicTaskAsync<T>(T task) where T : TaskBase
        {
            try
            {
                AccessPort accessPort = RetrieveOrCreate(task.PortInfo);
                await accessPort.AddPeriodicTaskAsync(task);
            }
            catch (PortConfigConflictException)
            {
                throw;
            }
        }

        public void RemovePeriodicTask<T>(T task) where T : TaskBase
        {
            AccessPort accessPort = Retrieve(task.PortInfo);
            accessPort?.RemovePeriodicTaskAsync(task);
        }


        public async Task<WaitHandle> RunOnceAsync<T>(T task) where T : TaskBase
        {
            try
            {
                AccessPort accessPort = RetrieveOrCreate(task.PortInfo);
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
                    accessPort.OpenPort();
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
                var result = from cell in AccessPortCollection where cell.Key.Equals(PortInfo) select cell;
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
        private void RegistTypeConverter<T>(Dictionary<Type, object> dic, ITypeConvertor<T> typeConvertor)
        {
            if (!dic.ContainsKey(typeof(T)))
            {
                dic.Add(typeof(T), typeConvertor);
            }
        }
        /// <summary>注册一个类型转换器
        /// 
        /// </summary>
        void IThreadSafeConvertorRegistry.RegistTypeConverter<T>(ITypeConvertor<T> typeConvertor)
        {
            lock (TypeConvertorCollectionLock)
            {
                if (!TypeConvertorCollection.ContainsKey(typeof(T)))
                {
                    TypeConvertorCollection.Add(typeof(T), typeConvertor);
                }
            }
        }



        /// <summary>取值
        /// 
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="info">请求信息</param>
        /// <returns>T类型的对象</returns>
        public T GetValue<T>(RequestInfo info)
        {
            ITypeConvertor<T> convertor = FindTypeConvertor<T>();
            try
            {
                return GetValue<T>(info, convertor);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>取值
        /// 
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="info">请求信息</param>
        /// <param name="convertor">类型转换器</param>
        /// <returns>T类型的对象</returns>
        /// <exception cref="Exception"></exception>
        public T GetValue<T>(RequestInfo info, ITypeConvertor<T> convertor)
        {
            if (typeof(T).IsArray) throw new Exception("本方法不支持转换数组类型");
            if (convertor == null) throw new Exception("类型转换器为null");
            AccessPort accessPort = Retrieve(info.PortInfo);
            if (accessPort == null) throw new Exception("找不到设备对应端口");

            byte[] bytes = accessPort.GetValues(info, convertor.ByteSize);
            if (bytes == null || bytes.Length < convertor.ByteSize) throw new Exception("字节数不足，转换失败");
            return convertor.Convert(bytes);
        }


        /// <summary>取数组
        /// 
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="info">请求信息</param>
        /// <param name="arrayLength">数组长度</param>
        /// <returns>T类型的对象</returns>
        public T GetValues<T>(RequestInfo info, int arrayLength)
        {
            ITypeConvertor<T> convertor = FindTypeConvertor<T>();
            try
            { 
                return GetValues<T>(info, arrayLength, convertor); 
            }
            catch
            { 
                throw; 
            }
        }
        /// <summary>取数组
        /// 
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="info">请求信息</param>
        /// <param name="arrayLength">数组长度</param>
        /// <param name="convertor">类型转换器</param>
        /// <returns>T类型的对象</returns>
        /// <exception cref="Exception"></exception>
        public T GetValues<T>(RequestInfo info, int arrayLength, ITypeConvertor<T> convertor)
        {
            if (!typeof(T).IsArray) throw new Exception("本方法不支持转换非数组类型");
            if (convertor == null) throw new Exception("类型转换器为null");
            AccessPort accessPort = Retrieve(info.PortInfo);
            if (accessPort == null) throw new Exception("找不到设备对应端口");

            byte[] bytes = accessPort.GetValues(info, convertor.ByteSize * arrayLength);
            if (bytes == null || bytes.Length < convertor.ByteSize * arrayLength) throw new Exception("字节数不足，转换失败");
            return convertor.Convert(bytes);
        }
    }
}
