#include <iostream>
#include <string>
#include "functions.h"

int main() {
    std::string message;
    std::cout << "Enter the message: ";
    std::getline(std::cin, message);

    std::string morseCode = encode(message);
    std::cout << "Encoded message: " << morseCode << std::endl;

    std::string decodedMessage = decode(morseCode);
    std::cout << "Decoded message: " << decodedMessage << std::endl;

    return 0;
}