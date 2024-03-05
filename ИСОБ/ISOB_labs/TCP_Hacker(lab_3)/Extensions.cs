using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Hacker_lab_3_
{
    public static class Extensions
    {
        public static byte[] GetBytes(this string str)
        {
            return Encoding.UTF32.GetBytes(str);
        }

        public static string GetString(this byte[] bytes, int offset)
        {
            return Encoding.UTF32.GetString(bytes, 0, offset);
        }

        public static string GetString(this byte[] bytes)
        {
            return Encoding.UTF32.GetString(bytes);
        }
    }
}
