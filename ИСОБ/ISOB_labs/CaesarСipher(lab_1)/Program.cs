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





//Console.WriteLine((int)'а');    //1072
//Console.WriteLine((int)'я');    //1103
//Console.WriteLine((int)'ё');    //1105
//Console.WriteLine((int)'А');    //1040
//Console.WriteLine((int)'Я');    //1071
//Console.WriteLine((int)'Ё');    //1025
//Console.WriteLine((char)1004);  //?
//Console.WriteLine("================");


//string input = "Съешь же ещё этих мягких французских булок, да выпей чаю.";
//string key = "Вова";
//string res = Ciphers.CeasarCipherEncrypt(input);

//Console.WriteLine(Ciphers.CeasarCipherEncrypt(input));
//Console.WriteLine(Ciphers.CeasarCipherDecrypt(res));

//Console.WriteLine("=================");
//res = Ciphers.VigenereCipherEncrypt(input, key);

//Console.WriteLine(res);
//Console.WriteLine(Ciphers.VigenereCipherDecrypt(res, key));