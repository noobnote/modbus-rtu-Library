using ModbusLibrary.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    internal class Program
    {
        //static void Main(string[] args){ }


        static void Main(string[] args)
        {
            //示例


            //哪个端口
            //站号
            //起始地址
            //读取个数

            var COM1 = new PortInfo("COM1", 9600, StopBits.One, 8, Parity.None);



            var taskForRead0 = new TaskForRead(0x03, COM1, 0x01, 0x00, 100);
            var taskForRead1 = new TaskForRead(0x03, COM1, 0x01, 0x00, 100);




            //添加周期任务
            AccessPortsManager.Instance.AddPeriodicTaskAsync(taskForRead0).Wait();
            AccessPortsManager.Instance.AddPeriodicTaskAsync(taskForRead1).Wait();



            //端口
            //站号
            //起始地址
            //读取个数

            //应当自带转换器


            //取值



            //端口
            //站号
            //存储区域
            //起始地址

            var request0 = new RequestInfo()
            {
                PortInfo = COM1,
                Host = 1,
                MemoryArea = DataMemory.MemoryArea.HR,
                StartAddress = 0x0000
            };
            var value0 = AccessPortsManager.Instance.GetValue<int>(request0);



            var request1 = new RequestInfo()
            {
                PortInfo = COM1,
                Host = 1,
                MemoryArea = DataMemory.MemoryArea.HR,
                StartAddress = 0xAA00
            };
            var value1 = AccessPortsManager.Instance.GetValue<string>(request1, new StringConvert());




            var request2 = new RequestInfo()
            {
                PortInfo = COM1,
                Host = 1,
                MemoryArea = DataMemory.MemoryArea.HR,
                StartAddress = 0xAA00
            };
            var value2 = AccessPortsManager.Instance.GetValues<int[]>(request2, 10);


            var request3 = new RequestInfo()
            {
                PortInfo = COM1,
                Host = 1,
                MemoryArea = DataMemory.MemoryArea.HR,
                StartAddress = 0xAA00
            };
            var value3 = AccessPortsManager.Instance.GetValues<string[]>(request3, 10, new StringArrayConvert());

            Console.ReadKey();
        }
    }










    //示例
    public class StringConvert : ITypeConvertor<string>
    {
        public int ByteSize { get { return 1000; } }

        public string Convert(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }


    //示例
    public class StringArrayConvert : ITypeConvertor<string[]>
    {
        public int ByteSize { get { return 1000; } }

        public string[] Convert(byte[] bytes)
        {
            return default(string[]);
        }
    }
}
