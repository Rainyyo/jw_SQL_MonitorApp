
using System.Text;
using System.Runtime.InteropServices;

namespace DXH.Ini
{
    public class DXHIni
    {
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);
        public static string ContentReader(string area, string key, string def, string file)
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            GetPrivateProfileString(area, key, def, stringBuilder, 1024, file);
            return stringBuilder.ToString();
        }
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(
            string mpAppName,
            string mpKeyName,
            string mpDefault,
            string mpFileName);
    }
}
