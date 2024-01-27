using System.Text;

namespace CaesarСipher_lab_1_
{
    internal static class Ciphers
    {
        private const int _caesarKey = 3;
        private readonly static List<char> _smallLitters = new();
        private readonly static List<char> _largeLitters = new();

        static Ciphers()
        {
            //Инициализируем алфавит маленьких букв(буква ё лежит отдельно от всех)
            for (int i = (int)'а'; i <= (int)'я'; i++)
                _smallLitters.Add((char)i);
            _smallLitters.Insert(6, 'ё');

            //Инициализируем алфавит больших букв
            for (int i = (int)'А'; i <= (int)'Я'; i++)
                _largeLitters.Add((char)i);
            _largeLitters.Insert(6, 'Ё');
        }

        public static string CeasarCipherEncrypt(string input)
        {
            StringBuilder output = new StringBuilder(input.Length);
            
            foreach (char c in input)
            {
                //Проверяем большие буквы и смещаем
                int largInd = _largeLitters.IndexOf(c);
                if(largInd >= 0)
                {
                    output.Append(_largeLitters[(largInd + _caesarKey) % _largeLitters.Count]);
                    continue;
                }

                //Если в больших буквах не нашли, то ищем в маленьких
                int smallInd = _smallLitters.IndexOf(c);
                if(smallInd >= 0)
                {
                    output.Append(_smallLitters[(smallInd + _caesarKey) % _smallLitters.Count]);
                    continue;
                }

                //Если какой-то символ не из русского алфавита, то просто пихаем на свое место
                output.Append(c);
            }

            return output.ToString();
        }


        public static string CeasarCipherDecrypt(string input)
        {
            StringBuilder output = new(input.Length);

            foreach (char c in input)
            {
                //Проверяем большие буквы и смещаем
                int largInd = _largeLitters.IndexOf(c);
                if (largInd >= 0)
                {
                    output.Append(_largeLitters[(largInd - _caesarKey + _largeLitters.Count) % _largeLitters.Count]);
                    continue;
                }

                //Если в больших буквах не нашли, то ищем в маленьких
                int smallInd = _smallLitters.IndexOf(c);
                if (smallInd >= 0)
                {
                    output.Append(_smallLitters[(smallInd - _caesarKey + _smallLitters.Count) % _smallLitters.Count]);
                    continue;
                }

                //Если какой-то символ не из русского алфавита, то просто пихаем на свое место
                output.Append(c);
            }

            return output.ToString();
        }
    }
}
