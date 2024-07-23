
using System.Text;
using System.Runtime.InteropServices;
using FreeSql.Internal;
using System;

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
        public static void TryToInt(ref int d, string str)
        {
            try
            {
                d = Convert.ToInt32(str);
            }
            catch
            {
            }
        }
        public static void TryToDouble(ref double d, string str)
        {
            try
            {
                d = Convert.ToDouble(str);
            }
            catch
            {
            }
        }
        public static void TryToBool(ref bool d, string str)
        {
            try
            {
                d = Convert.ToBoolean(str);
            }
            catch
            {
            }
        }
    }
}
