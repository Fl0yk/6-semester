using System.Reflection;
using System.Text;

namespace CaesarСipher_lab_1_
{
    internal static class Ciphers
    {
        private const int _caesarKey = 3;
        private readonly static List<char> _smallLitters = new();
        private readonly static List<char> _largeLitters = new();
        private readonly static int _alphabetSize;

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

            _alphabetSize = _smallLitters.Count;
        }

        public static string CeasarCipherEncrypt(string input)
        {
            return CeasarCipher(input, true);
        }

        public static string CeasarCipherDecrypt(string input)
        {
            return CeasarCipher(input, false);
        }

        private static string CeasarCipher(string input, bool isEncrypt)
        {
            StringBuilder output = new(input.Length);

            foreach (char c in input)
            {
                output.Append(Shift(c, _caesarKey, isEncrypt));
            }

            return output.ToString();
        }

        public static string VigenereCipherEncrypt(string input, string keyWord)
        {
            return VigenereCipher(input, keyWord, true);
        }

        public static string VigenereCipherDecrypt(string input, string keyWord)
        {
            return VigenereCipher(input, keyWord, false);
        }

        private static string VigenereCipher(string input, string keyWord, bool isEncrypt)
        {
            StringBuilder output = new(input.Length);

            for(int i = 0, j = 0; i < input.Length; i++)
            {
                //Нам нужен индекс буквы из ключевого слова
                //Так что сначала пытаемся найти среди больших
                int tryLargeInd = _largeLitters.IndexOf(keyWord[j % keyWord.Length]);
                //А затем проверяем, то ли мы нашли. Если нет, то ищем маленькую
                int keyId = tryLargeInd < 0 
                    ? _smallLitters.IndexOf(keyWord[j % keyWord.Length]) 
                    : tryLargeInd;

                if (input[i].IsRusLetter())
                {
                    //Казалось бы, зачем. А это для того, чтоб кодовое слово не уезжало,
                    //когда в основной строке пробел, запятая и т.д.
                    //Мне было бы пофиг, но в калькуляторах не уезжает
                    j++;
                }

                output.Append(Shift(input[i], keyId, isEncrypt));
            }

            return output.ToString();
        }

        private static char Shift(char letter, int key, bool isEncrypt)
        {
            //Проверяем большие буквы и возвращаем смещенную букву
            int largInd = _largeLitters.IndexOf(letter);
            if (largInd >= 0)
            {
                return _largeLitters[(_alphabetSize + largInd + key * (isEncrypt ? 1 : -1)) % _alphabetSize];
            }

            //Если в больших буквах не нашли, то ищем в маленьких
            int smallInd = _smallLitters.IndexOf(letter);
            if (smallInd >= 0)
            {
                return _smallLitters[(_alphabetSize + smallInd + key * (isEncrypt ? 1 : -1)) % _alphabetSize];
            }
            
            //Если какой-то символ не из русского алфавита, то просто пихаем на свое место
            return letter;
        }
    }

    public static class CharExtension
    {
        /// <summary>
        /// Проверяет, является ли символ буквой русского алфавита
        /// </summary>
        /// <param name="letter">Потенциальная русская буква</param>
        /// <returns></returns>
        public static bool IsRusLetter(this char letter)
        {
            return (char.IsBetween(letter, 'а', 'я')
                    || char.IsBetween(letter, 'А', 'Я')
                    || letter == 'ё' || letter == 'Ё');
        }
    }
}
