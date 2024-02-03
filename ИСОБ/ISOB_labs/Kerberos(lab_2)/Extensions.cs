using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kerberos_lab_2_
{
    public static class GetByteStringExtension
    {
        public static byte[] GetBytes(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
    }

    public static class GetJsonStringByteArrExtension
    {
        public static string GetJsonString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }

}
