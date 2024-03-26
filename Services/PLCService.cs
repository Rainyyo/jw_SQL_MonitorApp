

using NModbus;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Sockets;

namespace MonitorApp.Services
{
    public class PLCService : IPLCService
    {
        IModbusMaster master;
        TcpClient client;
        public bool Connected { get; set; }
        public void Close()
        {
            try
            {
                Connected = false;
                master.Dispose();
                client.Close();
                client.Dispose();
            }
            catch { }
        }

        public bool Connect(string ip)
        {
            try
            {
                client = new TcpClient(ip, 502);
                var factory = new ModbusFactory();
                master = factory.CreateMaster(client);
                Connected = true;
                return true;
            }
            catch (Exception ex)
            {
                Connected = false;
                return false;
            }
        }

        public bool[] ReadXCoils(int addr, int count)
        {
            return master.ReadCoils(1, (ushort)(0xF800 + addr), (ushort)count);
        }
        public bool[] ReadYCoils(int addr, int count)
        {
            return master.ReadCoils(1, (ushort)(0xFC00 + addr), (ushort)count);
        }
        public void WriteYCoil(int addr, bool val)
        {
            master.WriteSingleCoil(1, (ushort)(0xFC00 + addr), val);
        }
        public ushort[] ReadDRegisters(int addr, int count)
        {
            return master.ReadHoldingRegisters(1, (ushort)addr, (ushort)count);
        }
        public void WriteSingleRegister(int addr, int value)
        {
            var a1 = Convert.ToUInt16(((short)value).ToString("X4"), 16);
            master.WriteSingleRegister(1, (ushort)addr, a1);
        }
        public bool[] ReadMCoils(int addr, int count)
        {
            return master.ReadCoils(1, (ushort)addr, (ushort)count);
        }
        public void WriteMCoil(int addr, bool val)
        {
            master.WriteSingleCoil(1, (ushort)addr, val);
        }
        public float ReadFloat(int addr)
        {
            var regs = master.ReadHoldingRegisters(1, (ushort)addr, 2);
            var hexstr = string.Join("", from p in regs select p.ToString("X4"));
            var hexstr1 = hexstr.Substring(4, 4) + hexstr.Substring(0, 4);//高低位取反
            byte[] raw = new byte[hexstr1.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                // THEN DEPENDING ON ENDIANNESS
                //raw[i] = Convert.ToByte(hexstr1.Substring(i * 2, 2), 16);
                // OR
                raw[raw.Length - i - 1] = Convert.ToByte(hexstr1.Substring(i * 2, 2), 16);
            }
            return BitConverter.ToSingle(raw, 0);
        }
        public void WriteFloat(int addr, float value)
        {
            byte[] vOut = BitConverter.GetBytes(value);
            ushort[] ushorts= new ushort[vOut.Length / 2];
            for (int i = 0; i < vOut.Length / 2; i++)
            {
                string str = vOut[i * 2 + 1].ToString("X2") + vOut[i * 2].ToString("X2");
                ushorts[i] = Convert.ToUInt16(str, 16);
            }
            master.WriteMultipleRegisters(1, (ushort)addr, ushorts);
        }
    }
}
