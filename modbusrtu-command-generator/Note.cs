using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator
{

    /*
     * 2024-10-06 工作项
     * 开发Modbus-rtu协议通讯库：
     * 1、【closed】编写 值转换器：
     *  方便调用者取值时将值转换为目标值
        //byte[] to values 值转换器 valueConvertor

     * 2、【closed】编写 原始数据处理工作器：
     *      根据功能码区别的 存值器 worker（包含 需读取byte数、抓取数据包并返回、比较器【例如写操作时，比较发送、返回报文是否一致】）
     * 
     * 
     * 
     * 
     * 3、还需要考虑读取超时的情况（例如 远程主机反馈的是报错报文，那么读取字节数就可能达不到readCount，也就跳不出循环）
     * 
     * 
     * 2024-10-07
     * 1、【closed】DataMemory扩容方式不合理
     * 2、【closed】添加即时任务方法应当改为 可等待。
     * 3、【closed】ModbusManager类添加“注册原始数据处理器”方法
     * 
     * 
     * 
     * 2024-10-08
     * 1、【closed】可订阅AccessPort关闭事件
     * 2、【closed】需要重写TaskModule的派生类
     * 
     * 
     * 
     * 2025-05-19
     * 1、DataMemory类待修改：线圈、离散输入状态，它们的每个单元存储一个bit位，读取时不满8个位的按照1个byte返回
     * 2、【closed】任务对象可能是整块地读取，但是取值时可能只取有限个byte（比如int32是4个），那么要求取值用的任务对象为TaskForRead是不合理的
     * 3、TaskForRead和TaskForWrite的CreateCommand、GetExpectedByteCount方法需要具体实现
     * 4、需要更多的IRawDataProcessor实现
     * 5、单例创建、缓存器，应用于IRawDataProcessor（原始数据处理器）、报文生成器（）
     */
}
