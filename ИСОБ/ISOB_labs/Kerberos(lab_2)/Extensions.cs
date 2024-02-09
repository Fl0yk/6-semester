using System;
using System.Collections;
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

        public static byte[] GetDesEncryptBytes(this string str, string key)
        {
            return AlgorithmDES.Encrypt(str.GetBytes(), key.GetBytes());
        }
    }

    public static class ByteArrExtension
    {
        public static string GetJsonString(this byte[] bytes, string key)
        {
            return Encoding.UTF8.GetString(AlgorithmDES.Decrypt(bytes, key.GetBytes()));
        }

        public static string GetJsonString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] Xor(this byte[] a, byte[] b)
        {
            byte[] result = new byte[a.Length];

            for (int i = 0; i < a.Length; i++)
            {
                result[i] = (byte)(a[i] ^ b[i]);
            }

            return result;
        }
    }


    public static class BitArrayExtensions
    {
        public static string GetString(this BitArray array)
        {
            StringBuilder sb = new StringBuilder(array.Length);

            for (int i = 0; i < array.Length; i++) 
            {
                sb.Append(array[i] ? "1" : "0");
            }

            return sb.ToString();
        }

        public static byte[] GetBytes(this BitArray bits)
        {
            byte[] result = new byte[(bits.Length + 7) / 8];
            bits.CopyTo(result, 0);
            return result;
        }
    }

}
