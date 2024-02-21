#include "functions.h"

std::map<std::string, char> morseCodeToChar = {
    {".-", 'A'}, {"-...", 'B'}, {"-.-.", 'C'}, {"-..", 'D'}, {".", 'E'},
    {"..-.", 'F'}, {"--.", 'G'}, {"....", 'H'}, {"..", 'I'}, {".---", 'J'},
    {"-.-", 'K'}, {".-..", 'L'}, {"--", 'M'}, {"-.", 'N'}, {"---", 'O'},
    {".--.", 'P'}, {"--.-", 'Q'}, {".-.", 'R'}, {"...", 'S'}, {"-", 'T'},
    {"..-", 'U'}, {"...-", 'V'}, {".--", 'W'}, {"-..-", 'X'}, {"-.--", 'Y'},
    {"--..", 'Z'},

    {"-----", '0'}, {".----", '1'}, {"..---", '2'}, {"...--", '3'}, {"....-", '4'},
    {".....", '5'}, {"-....", '6'}, {"--...", '7'}, {"---..", '8'}, {"----.", '9'}
};

std::string decode(std::string code)
{
    std::string result;
    std::string curSymbol;

    for (char c : code) {
        if (c != ' ' && c != '.' && c != '-' && c != '/')
            return "Bad code";

        if ((c == ' ' || c == '/') && !curSymbol.empty()) {
            if(morseCodeToChar.find(curSymbol) == morseCodeToChar.end())
                return "Bad code";

            result += morseCodeToChar[curSymbol];
            if (c == '/')
                result += ' ';

            curSymbol.clear();
        }
        else if (c != ' ') {
            curSymbol += c;
        }
    }

    if (!curSymbol.empty())
    {
        if (morseCodeToChar.find(curSymbol) == morseCodeToChar.end())
            return "Bad code";

        result += morseCodeToChar[curSymbol];
    }

    return result;
}