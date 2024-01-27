using CaesarСipher_lab_1_;

Console.WriteLine((int)'а');    //1072
Console.WriteLine((int)'я');    //1103
Console.WriteLine((int)'ё');    //1105
Console.WriteLine((int)'А');    //1040
Console.WriteLine((int)'Я');    //1071
Console.WriteLine((int)'Ё');    //1025
Console.WriteLine((char)1004);  //?
Console.WriteLine("================");


string input = "Съешь же ещё этих мягких французских булок, да выпей чаю.";
string res = Ciphers.CeasarCipherEncrypt(input);

Console.WriteLine(Ciphers.CeasarCipherEncrypt(input));
Console.WriteLine(Ciphers.CeasarCipherDecrypt(res));