using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.ModbusLibrary.ModbusCore
{
    /// <summary>数据存储器
    /// 
    /// </summary>
    public class DataMemory
    {
        /// <summary>存储区域
        /// 
        /// </summary>
        public enum Area
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


        public int TargetHost { get; private set; }
        /// <summary>线圈状态 读写锁
        /// 
        /// </summary>
        private object _CSLock { get; set; }
        /// <summary>离散输入状态 读写锁
        /// 
        /// </summary>
        private object _DISLock { get; set; }
        /// <summary>保持寄存器 读写锁
        /// 
        /// </summary>
        private object _HRLock { get; set; }
        /// <summary>输入寄存器 读写锁
        /// 
        /// </summary>
        private object _IRLock { get; set; }


        /// <summary>线圈状态：Coil Status（CS）
        /// 
        /// </summary>
        private byte[] CS { get; set; }

        /// <summary>离散输入状态：Discrete Input Status（DIS）
        /// 
        /// </summary>
        private byte[] DIS { get; set; }
        /// <summary>保持寄存器：Holding Register（HR）
        /// 
        /// </summary>
        private byte[] HR { get; set; }
        /// <summary>输入寄存器：Input Register（IR）
        /// 
        /// </summary>
        private byte[] IR { get; set; }

        /// <summary>根据功能码判断存储区域
        /// 
        /// </summary>
        /// <param name="functionCode">功能码</param>
        /// <returns>存储区域</returns>
        public static Area JudgeArea(int functionCode)
        {
            Area area = Area.None;

            if (functionCode == 1)
            {
                area = Area.CS;
            }
            if (functionCode == 2)
            {
                area = Area.DIS;
            }
            if (functionCode == 3)
            {
                area = Area.HR;
            }
            if (functionCode == 4)
            {
                area = Area.IR;
            }
            return area;
        }

        /// <summary>保存数据
        /// 
        /// </summary>
        /// <param name="area">存储区域</param>
        /// <param name="startAdderss">起始地址</param>
        /// <param name="data"></param>
        public void SaveData(Area area, int startAdderss, byte[] data)
        {
            switch (area)
            {
                case Area.CS:
                    {
                        lock (_CSLock)
                        {
                            if (this.CS.Length < startAdderss - 1 + data.Length)
                            {
                                //扩充容量
                                byte[] old = this.CS;
                                byte[] @new = new byte[this.CS.Length * 2];
                                Array.Copy(old, 0, @new, 0, old.Length);
                                this.CS = @new;
                            }
                            Array.Copy(data, 0, this.CS, startAdderss, data.Length);
                        }
                    }
                    break;
                case Area.DIS:
                    {
                        lock (_DISLock)
                        {
                            if (this.DIS.Length < startAdderss - 1 + data.Length)
                            {
                                //扩充容量
                                byte[] old = this.DIS;
                                byte[] @new = new byte[this.DIS.Length * 2];
                                Array.Copy(old, 0, @new, 0, old.Length);
                                this.DIS = @new;
                            }
                            Array.Copy(data, 0, this.DIS, startAdderss, data.Length);
                        }
                    }
                    break;
                case Area.HR:
                    {
                        lock (_HRLock)
                        {
                            startAdderss = startAdderss * 2;//注意：寄存器的单个地址存储2个byte
                            if (this.HR.Length < startAdderss - 1 + data.Length)
                            {
                                //扩充容量
                                byte[] old = this.HR;
                                byte[] @new = new byte[this.HR.Length * 2];
                                Array.Copy(old, 0, @new, 0, old.Length);
                                this.HR = @new;
                            }
                            Array.Copy(data, 0, this.HR, startAdderss, data.Length);
                        }
                    }
                    break;
                case Area.IR:
                    {
                        lock (_IRLock)
                        {
                            startAdderss = startAdderss * 2;//注意：寄存器的单个地址存储2个byte
                            if (this.IR.Length < startAdderss - 1 + data.Length)
                            {
                                //扩充容量
                                byte[] old = this.IR;
                                byte[] @new = new byte[this.IR.Length * 2];
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
        public byte[] GetValues(Area area, int startAdderss, int quantity)
        {
            byte[] bytes = new byte[quantity];
            switch (area)
            {
                case Area.CS:
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
                case Area.DIS:
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
                case Area.HR:
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
                case Area.IR:
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
