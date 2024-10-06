using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modbusrtu_command_generator.CommandGenerator
{


    public class ModbusCrc16
    {
        //CRC16-Modbus的多项式是0x8005, 但是在工业通信中,一般是低位在前,高位在后的,所以就是0x8005的二进制反置后就变成了0xA001了
        private static readonly ushort polynomial = 0xA001;


        /*
        CRC计算方法：
                    1、预置1个16位的寄存器为十六进制0xFFFF(即全为1);称此寄存器为CRC寄存器;
                    2、把第一个字节数据(既通讯信息帧的第一个字节)与16位的CRC寄存器相异或，再把结果放于CRC寄存器;
                    3、把以下步骤执行8次（因为byte有8位）：
								                    a.把CRC寄存器的内容右移一位(朝低位)用0填补最高位，并检查右移后，被移出的那个bit位;
								                    b.如果移出位为0：不做任何处理，然后continue；
								                    c.如果移出位为1：CRC寄存器与多项式0xA001(1010 0000 0000 0001)进行异或，然后continue；



                    4、把输入的byte数组全都按照步骤3执行一遍
                    5、将该通讯信息帧所有字节按上述步骤计算完成后，得到的16位CRC寄存器的高、低字节进行交换;

         */

        /// <summary>计算Modbus-RTU CRC16
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ushort CalculateCRC16(byte[] bytes)
        {
            ushort register = 0xFFFF;


            for (int index = 0; index < bytes.Length; index++)
            {
                register = (ushort)(bytes[index] ^ register);

                int count = 0;
                while (count < 8)
                {
                    //获取将被移除的bit
                    bool result = CheckBit(register, 0);
                    register = (ushort)(register >> 1);
                    if (result)
                    {
                        register = (ushort)(register ^ polynomial);
                    }
                    //如果被移出bit为false将不作任何处理
                    count++;
                }
            }

            //return register;

            byte[] reverse = new byte[2];
            reverse[0] = (byte)(register / 256);
            reverse[1] = (byte)(register % 256);

            return BitConverter.ToUInt16(reverse, 0);

        }

        static bool CheckBit(ushort data, int index)
        {
            return (~(data | (~(1 << index)))) == 0;
        }
    }












    internal class Generator
    {



















        
        //以0x03读（保持寄存器）为例，输入：
        //站号
        //命令
        //地址
        //读取个数


        //行为 











        //以0x06写单字（单个寄存器）为例，输入：
        //站号
        //命令
        //地址
        //values
    }
}
