using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    /// <summary>访问端口
    /// 
    /// </summary>
    public class AccessPort
    {
        /// <summary>端口号
        /// 
        /// </summary>
        public string PortName { get; private set; }

        public delegate void AccessPortClosed(AccessPort accessPort);

        public event AccessPortClosed AccessPortClosedEvent;
        private SerialPort _serialPort;
        private SerialPort SerialPort { get { return this._serialPort; } }

        private WaitHandle[] WaitHandles { get; set; }

        public AccessPort(PortInfo deviceInfo)
        {
            this._serialPort = new SerialPort();
            _serialPort.BaudRate = deviceInfo.BaudRate;
            //_serialPort.PortName = PortName = "COM" + deviceInfo.Port.ToString();
            _serialPort.PortName = PortName = deviceInfo.Port.ToString();
            _serialPort.StopBits = deviceInfo.StopBits;//停止位
            _serialPort.DataBits = deviceInfo.DataBits;//数据位
            _serialPort.Parity = deviceInfo.Parity;//校验位
        }

        public void OpenPort()
        {
            this.SerialPort?.Open();
            BeginRunTask();
        }
        public void ClosePort()
        {
            CloseAllTasks();
            this.SerialPort?.Close();
        }


        /// <summary>仅一次
        /// 
        /// </summary>
        protected AutoResetEvent OnlyOnce { get; set; }
        /// <summary>周期
        /// 
        /// </summary>
        protected ManualResetEvent Periodic { get; set; }
        /// <summary>退出
        /// 
        /// </summary>
        protected AutoResetEvent ExitEvent { get; set; }
        /// <summary>任务线程已开始
        /// 
        /// </summary>
        public bool Begun { get; private set; }


        private void InitializeConfig()
        {
            OnlyOnce = new AutoResetEvent(false);
            Periodic = new ManualResetEvent(true);
            ExitEvent = new AutoResetEvent(false);

            WaitHandles = new WaitHandle[]
            {
                OnlyOnce,
                Periodic,
                ExitEvent,
            };
        }

        /// <summary>初始化并运行核心线程
        /// 
        /// </summary>
        private void InitializeAndLaunchCoreThread()
        {
            if (this.Begun)
            {
                return;
            }

            InitializeConfig();


            Task.Run(() =>
            {
                this.Begun = true;
                while (this.Begun)
                {
                    int signal = WaitHandle.WaitAny(WaitHandles);
                    switch (signal)
                    {
                        case 0:
                            {
                                //仅一次
                                ExecuteTasks(OnlyOnceTasks);
                            }
                            break;
                        case 1:
                            {
                                //周期
                                ExecuteTasks(PeriodicTasks);
                            }
                            break;
                        case 2:
                            {
                                this.AccessPortClosedEvent?.Invoke(this);
                                this.Begun = false;
                                break;
                            }
                    }
                }
            });
        }

        /// <summary>继续执行周期任务
        /// 
        /// </summary>
        private void Continue()
        {
            Periodic?.Set();
        }
        /// <summary>暂停执行周期任务
        /// 
        /// </summary>
        private void Suspend()
        {
            Periodic?.Reset();
        }
        /// <summary>开始运行任务
        /// 
        /// </summary>
        private void BeginRunTask()
        {
            InitializeAndLaunchCoreThread();
        }
        /// <summary>关闭所有任务
        /// 
        /// </summary>
        private void CloseAllTasks()
        {
            Suspend();
            ExitEvent?.Set();
        }
        /// <summary>运行任务且任务只执行一次
        /// 
        /// </summary>
        private void ExecuteOnlyOnceTask()
        {
            OnlyOnce?.Set();
        }




        /// <summary>单次任务队列
        /// 
        /// </summary>
        Queue<TaskBase> OnlyOnceTasks { get; set; } = new Queue<TaskBase>();

        /// <summary>周期任务队列
        /// 
        /// </summary>
        List<TaskBase> PeriodicTasks { get; set; } = new List<TaskBase>();
        /// <summary>单次任务队列锁
        /// 
        /// </summary>
        object OnlyOnceTasksLock { get; set; } = new object();
        /// <summary>周期任务队列锁
        /// 
        /// </summary>
        object PeriodicTasksLock { get; set; } = new object();

        /// <summary>添加周期任务
        /// 
        /// </summary>
        /// <param name="taskModule"></param>
        public async Task AddPeriodicTaskAsync<T>(T task) where T : TaskBase
        {
            await Task.Run(() =>
            {
                lock (PeriodicTasksLock)
                {
                    PeriodicTasks.Add(task);
                }
            });
        }


        /// <summary>移除周期任务
        /// 
        /// </summary>
        /// <param name="taskModule"></param>
        public async Task RemovePeriodicTaskAsync<T>(T task) where T : TaskBase
        {
            await Task.Run(() =>
            {
                lock (PeriodicTasksLock)
                {
                    PeriodicTasks.Remove(task);
                }
            });
        }

        /// <summary>添加单次任务
        /// 
        /// </summary>
        /// <param name="taskModule"></param>
        public async Task<WaitHandle> AddImmediateTaskAsync<T>(T task) where T : TaskBase
        {
            return await Task.Run<WaitHandle>(() =>
            {
                task.WaitHandle = new ManualResetEvent(false);
                lock (OnlyOnceTasksLock)
                {
                    OnlyOnceTasks.Enqueue(task);
                }
                ExecuteOnlyOnceTask();
                return task.WaitHandle;
            });
        }

        /// <summary>执行列表中的任务
        /// 
        /// </summary>
        /// <param name="onlyOnceTasks">仅用于区别</param>
        void ExecuteTasks(Queue<TaskBase> onlyOnceTasks)
        {
            lock (OnlyOnceTasksLock)
            {
                if (OnlyOnceTasks.Count <= 0)
                {
                    return;
                }
                while (OnlyOnceTasks.Count > 0)
                {
                    Execute(OnlyOnceTasks.Dequeue());
                }
            }
        }

        /// <summary>执行列表中的任务
        /// 
        /// </summary>
        /// <param name="periodicTasks">仅用于区别</param>
        void ExecuteTasks(List<TaskBase> periodicTasks)
        {
            lock (PeriodicTasksLock)
            {
                foreach (var task in PeriodicTasks)
                {
                    Execute(task);
                }
            }
        }

        ///// <summary>执行列表中的任务
        ///// 
        ///// </summary>
        ///// <param name="periodicTasks">仅用于区别</param>
        //void ExecuteTasks<T>(List<TaskBase> periodicTasks)
        //{
        //    lock (PeriodicTasksLock)
        //    {
        //        foreach (var task in PeriodicTasks)
        //        {
        //            Execute(task);
        //        }
        //    }
        //}


        public void Execute<T>(T taskModule) where T : TaskBase
        {
            this.SerialPort.DiscardInBuffer();
            this.SerialPort.DiscardOutBuffer();

            this.SerialPort.Write(taskModule.CreateCommand(), 0, taskModule.CreateCommand().Length);

            //应当接收的总字节数
            int expectedByteCount = taskModule.GetExpectedByteCount();
            //其它功能码待定
            byte[] buffer = new byte[expectedByteCount];


            Thread.Sleep(10);

            int offset = 0;
            while (expectedByteCount > 0)
            {
                int received = this.SerialPort.Read(buffer, offset, buffer.Length);
                offset += received;
                expectedByteCount -= received;

                //还需要考虑读取超时的情况（例如 远程主机反馈的是报错报文，那么读取字节数就可能达不到expectedByteCount，也就跳不出循环）
            }

            IRawDataProcessor<IEnumerable<byte>> processor = FindRawDataProcessor(taskModule.FunctionCode);

            //数据处理
            if (processor != null)
            {
                if (processor.Validate(taskModule.CreateCommand(), buffer))
                {
                    //处理read操作
                    if (IsRead(taskModule.FunctionCode))
                    {
                        byte[] processedData = processor.CaptureData(buffer).ToArray();

                        SaveData(taskModule, processedData);
                    }

                    taskModule.WaitHandle?.Set();
                }
                else
                {
                    //报文收发异常，需要处理该异常

                }
            }
            else
            {
                //找不到处理器，需要处理该异常
            }
        }

        /// <summary>检查功能码是否为“读取”
        /// 
        /// </summary>
        /// <param name="Func">被检查的功能码</param>
        /// <returns>true==read; false!=read</returns>
        private bool IsRead(int Func)
        {
            return Func == 1 || Func == 2 || Func == 3 || Func == 4;
        }



        /// <summary>原始数据处理器字典-锁
        /// 
        /// </summary>
        object RawDataProcessorDicLock { get; set; } = new object();
        /// <summary>原始数据处理器-字典
        /// 
        /// </summary>
        Dictionary<int, IRawDataProcessor<IEnumerable<byte>>> RawDataProcessorDic { get; set; } = new Dictionary<int, IRawDataProcessor<IEnumerable<byte>>>();
        /// <summary>注册一个原始数据处理器
        /// 
        /// </summary>
        /// <param name="rawDataProcessor">数据处理器</param>
        public void RegistRawDataProcessor(IRawDataProcessor<IEnumerable<byte>> rawDataProcessor)
        {
            lock (RawDataProcessorDicLock)
            {
                if (RawDataProcessorDic.ContainsKey(rawDataProcessor.TargetFunc))
                {
                    RawDataProcessorDic.Add(rawDataProcessor.TargetFunc, rawDataProcessor);
                }
            }
        }
        /// <summary>查找原始数据处理器
        /// 
        /// </summary>
        /// <param name="targetFunc">目标功能码</param>
        /// <returns>原始数据处理器</returns>
        public IRawDataProcessor<IEnumerable<byte>> FindRawDataProcessor(int targetFunc)
        {
            IRawDataProcessor<IEnumerable<byte>> processor = null;
            lock (RawDataProcessorDicLock)
            {
                if (RawDataProcessorDic.ContainsKey(targetFunc))
                {
                    processor = RawDataProcessorDic[targetFunc];
                }
            }
            return processor;
        }



        /// <summary>数据存储器字典-锁
        /// 
        /// </summary>
        object MemoryDicLock { get; set; } = new object();
        /// <summary>数据存储器-字典
        /// 
        /// </summary>
        Dictionary<int, DataMemory> MemoryDic { get; set; } = new Dictionary<int, DataMemory>();
        /// <summary>注册一个数据存储器
        /// 
        /// </summary>
        /// <param name="dataMemory">数据存储器</param>
        public void RegistDataMemory(DataMemory dataMemory)
        {
            lock (MemoryDicLock)
            {
                if (!MemoryDic.ContainsKey(dataMemory.TargetHost))
                {
                    MemoryDic.Add(dataMemory.TargetHost, dataMemory);
                }
            }
        }



        /// <summary>保存数据
        /// 
        /// </summary>
        /// <param name="taskModule">读取任务对象</param>
        /// <param name="data">需要被保存的数据</param>
        private void SaveData<T>(T taskModule, byte[] data) where T : TaskBase
        {
            lock (MemoryDicLock)
            {
                if (MemoryDic.ContainsKey(taskModule.Host))
                {
                    MemoryDic[taskModule.Host].SaveData(DataMemory.ConvertToMemoryAre(taskModule.FunctionCode), taskModule.StartAddress, data);
                }
            }
        }


        /// <summary>取值
        /// 
        /// </summary>
        /// <param name="taskModule">读取任务对象</param>
        /// <param name="quantity">需要读取的byte数</param>
        /// <returns></returns>
        public byte[] GetValues<T>(T taskModule, int quantity)where T : RequestInfo
        {
            //RequestInfo
            return GetValues(taskModule.Host,
                taskModule.MemoryArea,
                taskModule.StartAddress,
                quantity);
        }

        /// <summary>取值
        /// 
        /// </summary>
        /// <param name="host">站号</param>
        /// <param name="func">功能码</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="quantity">需要读取的byte数</param>
        /// <returns></returns>
        public byte[] GetValues(int host, DataMemory.MemoryArea area, int startAddress, int quantity)
        {
            byte[] bytes = null;
            lock (MemoryDicLock)
            {
                if (MemoryDic.ContainsKey(host))
                {
                    bytes = MemoryDic[host].GetValues(area, startAddress, quantity);
                }
            }
            return bytes;
        }

    }
}
