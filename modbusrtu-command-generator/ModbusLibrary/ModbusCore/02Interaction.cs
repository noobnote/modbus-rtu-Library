using modbusrtu_command_generator.CommandGenerator.WorkBase;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.ModbusLibrary.ModbusCore
{
    public class Interaction
    {
        private SerialPort _serialPort;
        private SerialPort SerialPort { get { return this._serialPort; } }

        private WaitHandle[] WaitHandles { get; set; }

        public Interaction(DeviceInfo deviceInfo)
        {
            this._serialPort = new SerialPort();
            _serialPort.BaudRate = deviceInfo.BaudRate;
            _serialPort.PortName = "COM" + deviceInfo.Port.ToString();
            _serialPort.StopBits = deviceInfo.StopBits;//停止位
            _serialPort.DataBits = deviceInfo.DataBits;//数据位
            _serialPort.Parity = deviceInfo.Parity;//校验位
        }

        public void OpenPort()
        {
            this.SerialPort?.Open();
        }



        public void ClosePort()
        {
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

        private void Initialize()
        {
            if (this.Begun)
            {
                return;
            }

            InitializeConfig();


            Task.Run(() =>
            {

                while (true)
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


                                return;
                            }
                    }
                }
            });
        }

        /// <summary>继续执行周期任务
        /// 
        /// </summary>
        public void Continue()
        {
            Periodic?.Set();
        }
        /// <summary>暂停执行周期任务
        /// 
        /// </summary>
        public void Suspend()
        {
            Periodic?.Reset();
        }
        /// <summary>开始运行任务
        /// 
        /// </summary>
        public void BeginRunTask()
        {
            Initialize();
            this.Begun = true;
        }
        /// <summary>关闭所有任务
        /// 
        /// </summary>
        public void CloseAllTask()
        {
            Suspend();
            ExitEvent?.Set();
            this.Begun = false;
        }
        /// <summary>运行任务且任务只执行一次
        /// 
        /// </summary>
        public void OnlyOnceTask()
        {
            OnlyOnce?.Set();
        }






        /// <summary>单次任务队列
        /// 
        /// </summary>
        Queue<ReadBase> OnlyOnceTasks { get; set; }
        /// <summary>周期任务队列
        /// 
        /// </summary>
        List<ReadBase> PeriodicTasks { get; set; }
        /// <summary>单次任务队列锁
        /// 
        /// </summary>
        object OnlyOnceTasksLock { get; set; }
        /// <summary>周期任务队列锁
        /// 
        /// </summary>
        object PeriodicTasksLock { get; set; }

        /// <summary>添加周期任务
        /// 
        /// </summary>
        /// <param name="readTask"></param>
        public void AddPeriodicTask(ReadBase readTask)
        {
            lock (PeriodicTasksLock)
            {
                PeriodicTasks.Add(readTask);
            }

        }
        /// <summary>移除周期任务
        /// 
        /// </summary>
        /// <param name="readTask"></param>
        public void RemovePeriodicTask(ReadBase readTask)
        {
            lock (PeriodicTasksLock)
            {
                PeriodicTasks.Remove(readTask);
            }
        }

        /// <summary>添加单次任务
        /// 
        /// </summary>
        /// <param name="readTask"></param>
        public void AddImmediateTask(ReadBase readTask)
        {
            lock (OnlyOnceTasksLock)
            {
                OnlyOnceTasks.Enqueue(readTask);
            }
        }

        /// <summary>执行列表中的任务
        /// 
        /// </summary>
        /// <param name="onlyOnceTasks">仅用于区别</param>
        void ExecuteTasks(Queue<ReadBase> onlyOnceTasks)
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
        void ExecuteTasks(List<ReadBase> periodicTasks)
        {
            lock (PeriodicTasksLock)
            {
                foreach (var task in PeriodicTasks)
                {
                    Execute(task);
                }
            }
        }


        public void Execute(ReadBase readBase)
        {
            this.SerialPort.DiscardInBuffer();
            this.SerialPort.DiscardOutBuffer();

            this.SerialPort.Write(readBase.CreateCommand(), 0, readBase.CreateCommand().Length);

            //应当接收的总字节数
            int readCount = readBase.GetExpectedByteCount();
            //其它功能码待定
            byte[] buffer = new byte[readCount];


            Thread.Sleep(10);

            int offset = 0;
            while (readCount > 0)
            {
                int received = this.SerialPort.Read(buffer, offset, buffer.Length);
                offset += received;
                readCount -= received;

                //还需要考虑读取超时的情况（例如 远程主机反馈的是报错报文，那么读取字节数就可能达不到readCount，也就跳不出循环）
            }

            IRawDataProcessor<byte[]> processor = null;
            lock (RawDataProcessorDicLock)
            {
                if (RawDataProcessorDic.ContainsKey(readBase.Host))
                {
                    processor = RawDataProcessorDic[readBase.Host];
                }
            }

            //数据处理
            if (processor != null)
            {
                if (processor.Validate(readBase.CreateCommand(), buffer))
                {
                    //处理read操作
                    if (IsRead(readBase.FunctionCode))
                    {
                        byte[] processedData = processor.CaptureData(buffer).ToArray();

                        SaveData(readBase, processedData);
                    }
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
        Dictionary<int, IRawDataProcessor<byte[]>> RawDataProcessorDic { get; set; } = new Dictionary<int, IRawDataProcessor<byte[]>>();
        /// <summary>注册一个原始数据处理器
        /// 
        /// </summary>
        /// <param name="host">远程主机站号</param>
        /// <param name="rawDataProcessor">数据处理器</param>
        public void RegistRawDataProcessor(int host, IRawDataProcessor<byte[]> rawDataProcessor)
        {
            lock (RawDataProcessorDicLock)
            {
                if (RawDataProcessorDic.ContainsKey(host))
                {
                    RawDataProcessorDic.Add(host, rawDataProcessor);
                }
            }
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
        /// <param name="host">远程主机站号</param>
        /// <param name="dataMemory">数据存储器</param>
        public void RegistDataMemory(int host, DataMemory dataMemory)
        {
            lock (MemoryDicLock)
            {
                if (!MemoryDic.ContainsKey(host))
                {
                    MemoryDic.Add(host, dataMemory);
                }
            }
        }



        /// <summary>保存数据
        /// 
        /// </summary>
        /// <param name="readBase">读取任务对象</param>
        /// <param name="data">需要被保存的数据</param>
        private void SaveData(ReadBase readBase, byte[] data)
        {
            lock (MemoryDicLock)
            {
                if (MemoryDic.ContainsKey(readBase.Host))
                {
                    MemoryDic[readBase.Host].SaveData(DataMemory.JudgeArea(readBase.FunctionCode), readBase.StartAddress, data);
                }
            }
        }


        /// <summary>取值
        /// 
        /// </summary>
        /// <param name="readBase">读取任务对象</param>
        /// <param name="quantity">需要读取的byte数</param>
        /// <returns></returns>
        public byte[] GetValues(ReadBase readBase, int quantity)
        {
            byte[] bytes = null;
            lock (MemoryDicLock)
            {
                if (MemoryDic.ContainsKey(readBase.Host))
                {
                    bytes = MemoryDic[readBase.Host].GetValues(DataMemory.JudgeArea(readBase.FunctionCode), readBase.StartAddress, quantity);
                }
            }
            return bytes;
        }
    }
}
