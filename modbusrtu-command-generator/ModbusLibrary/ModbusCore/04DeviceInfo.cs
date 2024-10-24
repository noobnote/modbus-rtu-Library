﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.ModbusLibrary.ModbusCore
{
    public class DeviceInfo
    {
        /// <summary>COM端口
        /// 
        /// </summary>
        public int Port { get; private set; }
        /// <summary>站号
        /// 
        /// </summary>
        public byte Host { get; private set; }
        /// <summary>设备名
        /// 
        /// </summary>
        public string Name { get; private set; }
        /// <summary>波特率
        /// 
        /// </summary>
        public int BaudRate { get; private set; }
        /// <summary>停止位
        /// 
        /// </summary>
        public StopBits StopBits { get; private set; }
        /// <summary>数据位
        /// 
        /// </summary>
        public int DataBits { get; private set; }
        /// <summary>校验位
        /// 
        /// </summary>
        public Parity Parity { get; private set; }
        public DeviceInfo(int port, byte host, string name, int baudrate, StopBits stopBits, int dataBits, Parity parity)
        {

            this.Port = port;
            this.Host = host;
            this.Name = name;
            this.BaudRate = baudrate;
            this.StopBits = stopBits;
            this.Parity = parity;
            this.DataBits = dataBits;
        }
        public static DeviceInfo Create(int port, byte host, string name, int baudrate, StopBits stopBits, int dataBits, Parity parity)
        {
            return new DeviceInfo(port, host, name, baudrate, stopBits, dataBits, parity);
        }
    }
}
