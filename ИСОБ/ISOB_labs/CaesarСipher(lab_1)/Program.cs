using CaesarСipher_lab_1_;

string fileName = "test.txt";

using (StreamReader sr = new StreamReader(fileName))
{
    string text = sr.ReadLine();
    string keyWord = sr.ReadLine();

    Console.WriteLine($"Считанный текст из файла: {text}");
    Console.WriteLine($"Считанное кодовое слово: {keyWord}");
    Console.WriteLine("\n========================================\n");

    string ceaserEncode = Ciphers.CeasarCipherEncrypt(text);
    Console.WriteLine($"Зашифрованный текст с помощью шифра Цезаря: \n{ceaserEncode}\n");
    Console.WriteLine($"Расшифрованный текст с помощью шифра Цезаря: \n{Ciphers.CeasarCipherDecrypt(ceaserEncode)}");
    Console.WriteLine("\n========================================\n");

    string vigenereEncode = Ciphers.VigenereCipherEncrypt(text, keyWord);
    Console.WriteLine($"Зашифрованный текст с помощью шифра Виженера: \n{vigenereEncode}\n");
    Console.WriteLine($"Расшифрованный текст с помощью шифра Виженера: \n{Ciphers.VigenereCipherDecrypt(vigenereEncode, keyWord)}");
    Console.WriteLine("\n========================================\n");
}
