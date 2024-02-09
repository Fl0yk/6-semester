using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kerberos_lab_2_
{
    internal class AlgorithmDES
    {
        #region Constans
        private static readonly byte[] _IP = [ 58, 50, 42, 34, 26, 18, 10, 2,
                                        60, 52, 44, 36, 28, 20, 12, 4,
                                        62, 54, 46, 38, 30, 22, 14, 6,
                                        64, 56, 48, 40, 32, 24, 16, 8,
                                        57, 49, 41, 33, 25, 17, 9, 1,
                                        59, 51, 43, 35, 27, 19, 11, 3,
                                        61, 53, 45, 37, 29, 21, 13, 5,
                                        63, 55, 47, 39, 31, 23, 15, 7 ];

        private static readonly byte[] _IP_1 = [  40, 8, 48, 16, 56, 24, 64, 32,
                                        39, 7, 47, 15, 55, 23, 63, 31,
                                        38, 6, 46, 14, 54, 22, 62, 30,
                                        37, 5, 45, 13, 53, 21, 61, 29,
                                        36, 4, 44, 12, 52, 20, 60, 28,
                                        35, 3, 43, 11, 51, 19, 59, 27,
                                        34, 2, 42, 10, 50, 18, 58, 26,
                                        33, 1, 41, 9, 49, 17, 57, 25 ];


        #endregion

        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            List<BitArray> keys = GetKeys(key);

            List<byte> bigData = new(data);
         
            //Добавляем пробелы до нормального размера
            while (bigData.Count % 8 != 0)
            {
                bigData.Add((byte)' ');
            }
            List<BitArray> blocks = GetBlocks(bigData.ToArray());

            IEnumerable<byte> resEnc = new List<byte>(data.Length);

            for (int i = 0; i < blocks.Count; i++)
            {
                //Делаем первоначальную перестановку
                BitArray initialPermutation = Permutate(blocks[i], _IP);
                //Console.WriteLine("Block: " + blocks[i].GetString());

                BitArray left = new(32);
                BitArray right = new(32);

                for (int j = 0; j < 32; j++)
                {
                    left[j] = initialPermutation[j];
                    right[j] = initialPermutation[32 + j];
                }


                for (int j = 0; j < 16; j++)
                {
                    //Console.WriteLine("After xor:");
                    //Console.WriteLine("right: " + right.GetString());
                    //Console.WriteLine("left: " + left.GetString());
                    BitArray newRight = left.Xor(keys[j % keys.Count]);
                    left = right;
                    right = newRight;
                    //Console.WriteLine("After xor:");
                    //Console.WriteLine("right: " + right.GetString());
                    //Console.WriteLine("left: " + left.GetString());
                }

                BitArray combined = new(64);
                for (int j = 0; j < 32; j++)
                {
                    combined[j] = left[j];
                    combined[32 + j] = right[j];
                }
                //Console.WriteLine("Res block: " + combined.GetString());

                resEnc = resEnc.Concat(combined.GetBytes());
                //Console.WriteLine("Res: " + string.Join(" ", resEnc));
                //Console.WriteLine("========================================");
            }

            return resEnc.ToArray();
        }

        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            List<BitArray> keys = GetKeys(key);
            BitArray dataBits = new(data);

            List<BitArray> blocks = GetBlocks(data.ToArray());

            IEnumerable<byte> resEnc = new List<byte>(data.Length);

            for (int i = 0; i < blocks.Count; i++)
            {
                //Делаем первоначальную перестановку
                BitArray initialPermutation = Permutate(blocks[i], _IP_1);

                BitArray left = new(32);
                BitArray right = new(32);

                for (int j = 0; j < 32; j++)
                {
                    left[j] = initialPermutation[j];
                    right[j] = initialPermutation[32 + j];
                }


                for (int j = 0; j < 16; j++)
                {
                    BitArray newRight = left.Xor(keys[j % keys.Count]);
                    left = right;
                    right = newRight;
                }

                BitArray combined = new(64);
                for (int j = 0; j < 32; j++)
                {
                    combined[j] = left[j];
                    combined[32 + j] = right[j];
                }
                
                resEnc = resEnc.Concat(combined.GetBytes());
            }
      
            return resEnc.ToArray();
        }

        /// <summary>
        /// Преобразует входной набор байтов в зависимости от таблицы
        /// </summary>
        /// <param name="input">Входной набор битов</param>
        /// <param name="table">Таблица для преобразования</param>
        /// <returns>Измененнную набор битов</returns>
        private static BitArray Permutate(BitArray input, byte[] table)
        {
            BitArray output = new(table.Length);

            for (int i = 0; i < table.Length; i++)
            {
                output[i] = input[table[i] - 1];
            }

            return output;
        }

        private static List<BitArray> GetKeys(byte[] key)
        {
            if (key.Length != 16)
                throw new ArgumentException("Ключ должен быть равен 16 байт");

            List<BitArray> keys = new(16);

            for(int i = 0; i < 16; i += 4)
            {
                keys.Add(new BitArray(key.Skip(i).Take(4).ToArray()));
            }

            return keys;
        }

        private static List<BitArray> GetBlocks(byte[] data)
        {
            List<BitArray> blocks = new(data.Length / 8);

            for(int i = 0; i < data.Length; i += 8)
            {
                blocks.Add(new BitArray(data.Skip(i).Take(8).ToArray()));
            }

            return blocks;
        }
    }
}
