public enum TokenType
{
    Keyword, Identifier, Constant, StringLiteral,
    Operator, Punctuator, PotencialError, Unknown
}

public class Token
{
    private static int last_id = 0;

    public int Id { get; private set; }
    public TokenType Type { get; set; }
    public string Value { get; set; }
    public Token(TokenType type, string value)
    {
        Id = last_id++;
        Type = type;
        Value = value;
    }
}

public class Lexer
{
    private readonly string _source;
    private readonly List<Token> _tokens = new List<Token>();
    private int _position = 0;

    //Набор зарезервтрованных слов
    private static readonly HashSet<string> Keywords = new HashSet<string>
    {
        "auto", "break", "case", "char", "const", "continue", "default", "do",
        "double", "else", "float", "for", "if",
        "int", "long", "return", "short", "signed", "sizeof",
        "static", "switch", "typedef", "unsigned", "void", "while",
    };

    private static readonly HashSet<string> _types = new HashSet<string>
    {
        "auto", "char", "double", "float", "int", "long", "short", "signed", "void"
    };

    //Набор зарезервтрованных слов
    private static readonly HashSet<string> PotencialError = new HashSet<string>
    {
        "cons", "ele", "elseif", "for", "iff", "whil",
    };

    //Набор возможных операторов
    private static readonly HashSet<string> Operators = new HashSet<string>
    {
        "+", "-", "*", "/", "%", "&", "|", "^", "!", "~", "++", "--", 
        "==", "!=", ">", "<", ">=", "<=", "&&", "||",
        "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=",
    };

    public Lexer(string source)
    {
        _source = source;
    }

    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            char currentChar = Peek();

            //Пропускаем пробельные символы
            if (char.IsWhiteSpace(currentChar))
            {
                _position++;
                continue;
            }

            //Если начинается с цифры, то это должно быть константное число
            if (char.IsDigit(currentChar))
            {
                string number = ReadWhile(c => char.IsDigit(c) || c == '.' || c == ',');
                //Некорректное заадание константного числа
                if(number.Contains(','))
                {
                    _tokens.Add(new Token(TokenType.Unknown, number));
                    continue;
                }

                _tokens.Add(new Token(TokenType.Constant, number));
                continue;
            }

            //Если начинается с буквы, то это либо ключевое слово либо переменная
            //Первый символ обязательно буква, дальше могут идти цифры
            if (char.IsLetter(currentChar))
            {
                string identifier = ReadWhile(char.IsLetterOrDigit);

                if (Keywords.Contains(identifier))
                {
                    _tokens.Add(new Token(TokenType.Keyword, identifier));
                }
                //Необъявленная переменная
                else if (!IsDecaredVarible(identifier))
                {
                    if (PotencialError.Contains(identifier))
                        _tokens.Add(new Token(TokenType.PotencialError, identifier));
                    else
                        _tokens.Add(new Token(TokenType.Unknown, identifier));
                }
                //Все нормально
                else
                {
                    _tokens.Add(new Token(TokenType.Identifier, identifier));
                }

                continue;
            }

            //Если оператор. Может быть опреатор из нескольких символов, так что тоже проверяем дальше
            if (Operators.Contains(currentChar.ToString()))
            {
                //string op = ReadWhile(c => Operators.Contains(c.ToString()));
                string op = ReadWhileOperator();
                if(Operators.Contains(op))
                {
                    _tokens.Add(new Token(TokenType.Operator, op));
                }
                else
                {
                    _tokens.Add(new Token(TokenType.Unknown, op));
                }
      
                continue;
            }

            //Проверяем на константную строку (т.е. если кавычка, то читаем до следующей)
            if(currentChar == '\"')
            {
                _tokens.Add(new Token(TokenType.Punctuator, currentChar.ToString()));
                _position++;
                _tokens.Add(new Token(TokenType.StringLiteral, ReadWhile(c => c != '\"')));
            }

            if (currentChar == '\'')
            {
                _tokens.Add(new Token(TokenType.Punctuator, currentChar.ToString()));
                _position++;
                string symbol = ReadWhile(c => c != '\'');
                //В одинарные кавычки можно только символ вставлять
                //Длинна равна 3, если две одинарных кавычки и сам символ
                if ( symbol.Length != 1)
                {
                    _tokens.Add(new Token(TokenType.Unknown, symbol));
                }
                else
                {
                    _tokens.Add(new Token(TokenType.StringLiteral, symbol));
                }
            }


            //Остраются только символы пунктуации
            _tokens.Add(new Token(TokenType.Punctuator, currentChar.ToString()));
            _position++;
        }

        return _tokens;
    }

    //Читаем символы ,пока выполняется какое-то условие
    private string ReadWhile(Func<char, bool> condition)
    {
        int start = _position;
        while (!IsAtEnd() && condition(Peek()))
        {
            _position++;
        }
        return _source.Substring(start, _position - start);
    }

    //Проверяем, не закончился ли исходный текст
    private bool IsAtEnd()
    {
        return _position >= _source.Length;
    }

    //Берем нынешний символ
    private char Peek()
    {
        return _source[_position];
    }

    //Читаем символы, пока является оператором
    private string ReadWhileOperator()
    {
        int start = _position;
        while (!IsAtEnd() && Operators.Contains(Peek().ToString()))
        {
            _position++;
        }
        return _source.Substring(start, _position - start);
    }

    //Проверяем символы, которые могут быть не первыми в составном операторе
    private bool IsOperator(char c)
    {
        return Operators.Contains(c.ToString()) ||
               (c == '+' || c == '-' || c == '&' || c == '|' || c == '=');
    }

    private bool IsDecaredVarible(string var)
    {
        int last = _tokens.Count - 1;
        if (last < 0)
            return false;

        //Проверяем, была ли эта переменная раньше
        if(_tokens.FirstOrDefault(t => t.Value == var) is Token token 
                && token.Type != TokenType.Unknown)
        {
            return true;
        }

        //Проверяем, является ли это объявлением переменной
        while (last >= 0 && _tokens[last].Value != ";")
        {
            if (_types.Contains(_tokens[last].Value))
                return true;
            last--;
        }

        return false;
    }
}

public class Program
{
    public static void Main()
    {
        var filePath = "example.c";
        var code = File.ReadAllText(filePath);
        Lexer lexer = new Lexer(code);
        List<Token> tokens = lexer.Tokenize();

        foreach (var token in tokens)
        {
            if (token.Type == TokenType.Unknown || token.Type == TokenType.PotencialError)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error! ID: {token.Id}, Type: {token.Type}, Value: '{token.Value}'");
                Console.ResetColor();
                continue;
            }

            Console.WriteLine($"ID: {token.Id}, Type: {token.Type}, Value: '{token.Value}'");
        }
    }
}