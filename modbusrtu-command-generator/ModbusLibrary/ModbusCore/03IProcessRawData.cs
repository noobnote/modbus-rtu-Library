﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.ModbusLibrary.ModbusCore
{
    /// <summary>原始数据处理器
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRawDataProcessor<T> where T : IEnumerable<byte>
    {
        //根据功能码区别的 存值器 worker（包含 需读取byte数、抓取数据包并返回、比较器【例如写操作时，比较发送、返回报文是否一致】）

        /// <summary>目标功能码
        /// 
        /// </summary>
        byte TargetFunc { get; set; }
        /// <summary>抓取数据
        /// 
        /// </summary>
        /// <param name="rawData">原始数据</param>
        /// <returns></returns>
        T CaptureData(T rawData);
        /// <summary>验证收发报文是否成功
        /// 
        /// </summary>
        /// <param name="sentMessage">发送的报文</param>
        /// <param name="receivedMessage">接收的报文</param>
        /// <returns></returns>
        bool Validate(T sentMessage, T receivedMessage);
        /// <summary>验证收发报文是否成功
        /// 
        /// </summary>
        /// <param name="sentMessage">发送的报文</param>
        /// <param name="receivedMessage">接收的报文</param>
        /// <param name="handleError">处理错误</param>
        /// <returns></returns>
        bool Validate(T sentMessage, T receivedMessage, Action<Exception> handleError);

    }
}