#include <iostream>
#include <fstream>
#include <string>
#include "functions.h"

// На вход программы можем получить два аргкмента
// Первый - флаг, дкуодируем ли мы (-d)
// Второй - файл для вывода

void inputInFile(std::string file, std::string text);

int main(int argc, char* argv[]) {
    bool isEncode = true;
    std::string fileName;

    if (argc == 2)
    {
        if (std::string(argv[1]) == "-d")
        {
            isEncode = false;
        }
        else
        {
            fileName = argv[1];
        }
    }
    else if (argc == 3)
    {
        if (std::string(argv[1]) != "-d")
        {
            std::cout << "Incorrect arguments" << std::endl;
            return 0;
        }

        isEncode = false;
        fileName = argv[2];
    }

    std::string message;
    std::cout << "Enter the message: ";
    std::getline(std::cin, message);
    
    std::string result;

    if (isEncode)
    {
        std::string morseCode = encode(message);
        result = "Encoded message: " + morseCode;
    }
    else
    {
        std::string decodedMessage = decode(message);
        result = "Decoded message: " + decodedMessage;
    }

    if (fileName.empty())
    {
        std::cout << result << std::endl;
    }
    else
    {
        inputInFile(fileName, result);
    }

    return 0;
}

void inputInFile(std::string file, std::string text)
{
    std::fstream out; // поток для записи
    out.open(file, std::ios::out);

    if(!out)
    {
        std::cout << "File not exist and can't create" << std::endl;
    }

    out << text << std::endl;

    out.close();
}