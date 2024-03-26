

namespace MonitorApp.Services
{
    public interface IPLCService
    {
        bool Connected { get; set; }
        bool[] ReadXCoils(int addr, int count);
        bool[] ReadYCoils(int addr, int count);
        void WriteYCoil(int addr, bool val);
        ushort[] ReadDRegisters(int addr, int count);
        void WriteSingleRegister(int addr, int value);
        bool[] ReadMCoils(int addr, int count);
        void WriteMCoil(int addr, bool val);
        float ReadFloat(int addr);
        void WriteFloat(int addr, float value);
        bool Connect(string ip);
        void Close();
    }
}
