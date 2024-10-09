using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLibrary.Core
{
    /// <summary>数据存储器
    /// 
    /// </summary>
    public class DataMemory
    {
        /// <summary>存储区域
        /// 
        /// </summary>
        public enum MemoryArea
        {
            /// <summary>无意义
            /// 
            /// </summary>
            None,
            /// <summary>线圈状态：Coil Status（CS）
            /// 
            /// </summary>
            CS,
            /// <summary>离散输入状态：Discrete Input Status（DIS）
            /// 
            /// </summary>
            DIS,
            /// <summary>保持寄存器：Holding Register（HR）
            /// 
            /// </summary>
            HR,
            /// <summary>输入寄存器：Input Register（IR）
            /// 
            /// </summary>
            IR,
        }
        /// <summary>目标主机站号
        /// 
        /// </summary>
        public int TargetHost { get; private set; }
        /// <summary>线圈状态 读写锁
        /// 
        /// </summary>
        private object _CSLock { get; set; } = new object();
        /// <summary>离散输入状态 读写锁
        /// 
        /// </summary>
        private object _DISLock { get; set; } = new object();
        /// <summary>保持寄存器 读写锁
        /// 
        /// </summary>
        private object _HRLock { get; set; } = new object();
        /// <summary>输入寄存器 读写锁
        /// 
        /// </summary>
        private object _IRLock { get; set; } = new object();


        /// <summary>线圈状态：Coil Status（CS）
        /// 
        /// </summary>
        private byte[] CS { get; set; } = new byte[1];

        /// <summary>离散输入状态：Discrete Input Status（DIS）
        /// 
        /// </summary>
        private byte[] DIS { get; set; } = new byte[1];
        /// <summary>保持寄存器：Holding Register（HR）
        /// 
        /// </summary>
        private byte[] HR { get; set; } = new byte[1];
        /// <summary>输入寄存器：Input Register（IR）
        /// 
        /// </summary>
        private byte[] IR { get; set; } = new byte[1];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetHost">目标主机站号</param>
        public DataMemory(int targetHost)
        {
            this.TargetHost = targetHost;
        }

        /// <summary>根据功能码转换为具体存储区域
        /// 
        /// </summary>
        /// <param name="functionCode">功能码</param>
        /// <returns>存储区域</returns>
        public static MemoryArea ConvertToMemoryAre(int functionCode)
        {
            MemoryArea area = MemoryArea.None;

            if (functionCode == 1)
            {
                area = MemoryArea.CS;
            }
            if (functionCode == 2)
            {
                area = MemoryArea.DIS;
            }
            if (functionCode == 3)
            {
                area = MemoryArea.HR;
            }
            if (functionCode == 4)
            {
                area = MemoryArea.IR;
            }
            return area;
        }

        /// <summary>保存数据
        /// 
        /// </summary>
        /// <param name="area">存储区域</param>
        /// <param name="startAdderss">起始地址</param>
        /// <param name="data"></param>
        public void SaveData(MemoryArea area, int startAdderss, byte[] data)
        {
            switch (area)
            {
                case MemoryArea.CS:
                    {
                        lock (_CSLock)
                        {
                            if (this.CS.Length < startAdderss - 1 + data.Length)
                            {
                                int multiple = 1;
                                while (this.CS.Length * multiple < startAdderss - 1 + data.Length)
                                {
                                    multiple++;
                                }

                                //扩充容量
                                byte[] old = this.CS;
                                byte[] @new = new byte[this.CS.Length * multiple];
                                Array.Copy(old, 0, @new, 0, old.Length);
                                this.CS = @new;
                            }
                            Array.Copy(data, 0, this.CS, startAdderss, data.Length);
                        }
                    }
                    break;
                case MemoryArea.DIS:
                    {
                        lock (_DISLock)
                        {
                            if (this.DIS.Length < startAdderss - 1 + data.Length)
                            {
                                int multiple = 1;
                                while (this.DIS.Length * multiple < startAdderss - 1 + data.Length)
                                {
                                    multiple++;
                                }

                                //扩充容量
                                byte[] old = this.DIS;
                                byte[] @new = new byte[this.DIS.Length * multiple];
                                Array.Copy(old, 0, @new, 0, old.Length);
                                this.DIS = @new;
                            }
                            Array.Copy(data, 0, this.DIS, startAdderss, data.Length);
                        }
                    }
                    break;
                case MemoryArea.HR:
                    {
                        lock (_HRLock)
                        {
                            startAdderss = startAdderss * 2;//注意：寄存器的单个地址存储2个byte
                            if (this.HR.Length < startAdderss - 1 + data.Length)
                            {
                                int multiple = 1;
                                while (this.HR.Length * multiple < startAdderss - 1 + data.Length)
                                {
                                    multiple++;
                                }

                                //扩充容量
                                byte[] old = this.HR;
                                byte[] @new = new byte[this.HR.Length * multiple];
                                Array.Copy(old, 0, @new, 0, old.Length);
                                this.HR = @new;
                            }
                            Array.Copy(data, 0, this.HR, startAdderss, data.Length);
                        }
                    }
                    break;
                case MemoryArea.IR:
                    {
                        lock (_IRLock)
                        {
                            startAdderss = startAdderss * 2;//注意：寄存器的单个地址存储2个byte
                            if (this.IR.Length < startAdderss - 1 + data.Length)
                            {
                                int multiple = 1;
                                while (this.IR.Length * multiple < startAdderss - 1 + data.Length)
                                {
                                    multiple++;
                                }

                                //扩充容量
                                byte[] old = this.IR;
                                byte[] @new = new byte[this.IR.Length * multiple];
                                Array.Copy(old, 0, @new, 0, old.Length);
                                this.IR = @new;
                            }
                            Array.Copy(data, 0, this.IR, startAdderss, data.Length);
                        }
                    }
                    break;
            }
        }


        /// <summary>取值。从指定的起始地址开始，连续取值。
        /// 
        /// </summary>
        /// <param name="area">存储区域</param>
        /// <param name="startAdderss">起始地址</param>
        /// <param name="quantity">需要获取的字节数</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public byte[] GetValues(MemoryArea area, int startAdderss, int quantity)
        {
            byte[] bytes = new byte[quantity];
            switch (area)
            {
                case MemoryArea.CS:
                    {
                        lock (_CSLock)
                        {
                            if (this.CS.Length < startAdderss - 1 + quantity)
                            {
                                throw new IndexOutOfRangeException();
                            }
                            Array.Copy(this.CS, startAdderss, bytes, 0, bytes.Length);
                        }
                    }
                    break;
                case MemoryArea.DIS:
                    {
                        lock (_DISLock)
                        {
                            if (this.DIS.Length < startAdderss - 1 + quantity)
                            {
                                throw new IndexOutOfRangeException();
                            }
                            Array.Copy(this.DIS, startAdderss, bytes, 0, bytes.Length);
                        }
                    }
                    break;
                case MemoryArea.HR:
                    {
                        lock (_HRLock)
                        {
                            startAdderss = startAdderss * 2;//注意：寄存器的单个地址存储2个byte
                            if (this.HR.Length < startAdderss - 1 + quantity)
                            {
                                throw new IndexOutOfRangeException();
                            }
                            Array.Copy(this.HR, startAdderss, bytes, 0, bytes.Length);
                        }
                    }
                    break;
                case MemoryArea.IR:
                    {
                        lock (_IRLock)
                        {
                            startAdderss = startAdderss * 2;//注意：寄存器的单个地址存储2个byte
                            if (this.IR.Length < startAdderss - 1 + quantity)
                            {
                                throw new IndexOutOfRangeException();
                            }
                            Array.Copy(this.IR, startAdderss, bytes, 0, bytes.Length);
                        }
                    }
                    break;
            }
            return bytes;
        }
    }




}
