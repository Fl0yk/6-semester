using System.Net.Http.Headers;

namespace Compiler.Lexis
{
    public class Lexer
    {
        private readonly string _source;
        private readonly List<Token> _tokens = [];
        private int _position = 0;

        #region Special words ans symbols
        //Набор зарезервтрованных слов
        private static readonly HashSet<string> Keywords =
        [
            "auto", "break", "case", "char", "const", "continue", "default", "do",
            "double", "else", "float", "for", "if", "struct",
            "int", "long", "return", "short", "signed", "sizeof",
            "static", "switch", "typedef", "unsigned", "void", "while"
        ];

        //Набор возможных операторов
        private static readonly HashSet<string> Operators =
        [
            "+", "-", "*", "/", "%", "&", "|", "^", "!", "~", "++", "--",
            "==", "!=", ">", "<", ">=", "<=", "&&", "||", ".",
            "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "'", "\""
        ];

        private static readonly HashSet<string> Punctuators =
        [
            "[", "]", "{", "}", "(", ")", ",", ":", ";", "\n", "\t"
        ];

        #endregion

        public Lexer(string source)
        {
            _source = source;
        }

        public List<Token> Tokenize()
        {
            while (!IsAtEnd())
            {
                //Console.WriteLine(_position + "  " + Peek());
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
                    string number = ReadWhile(c => char.IsDigit(c) || c == '.');

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
                    // Нужно считать строковый литерал
                    if (currentChar == '\"' || currentChar =='\'')
                    {
                        string startOp = currentChar.ToString();
                        _tokens.Add(new Token(TokenType.Operator, startOp));
                        _position++;

                        string literal = ReadWhile(c => c != startOp[0]);
                        string tmp = literal;

                        if (IsAtEnd())
                            throw new Exception("Не закрыли кавычку");

                        List<string> parts = [];
                        int ind_n = 0, ind_t = 0;

                        while (ind_n >= 0 || ind_t >= 0)
                        {
                            ind_n = literal.IndexOf("\\n");
                            ind_t = literal.IndexOf("\\t");

                            if (ind_n >= 0 && ind_t >= 0 && ind_n < ind_t)
                            {
                                parts.Add(literal.Substring(0, ind_n));
                                parts.Add("\\n");
                                literal = literal.Substring(ind_n + 2);
                            }
                            else if (ind_n >= 0 && ind_t >= 0 && ind_t < ind_n)
                            {
                                parts.Add(literal.Substring(0, ind_t));
                                parts.Add("\\t");
                                literal = literal.Substring(ind_t + 2);
                            }
                            else if (ind_n >= 0)
                            {
                                parts.Add(literal.Substring(0, ind_n));
                                parts.Add("\\n");
                                literal = literal.Substring(ind_n + 2);
                            }
                            else if (ind_t >= 0)
                            {
                                parts.Add(literal.Substring(0, ind_t));
                                parts.Add("\\t");
                                literal = literal.Substring(ind_t + 2);
                            }

                            if (parts.Count > 1 && parts[parts.Count - 2] == string.Empty)
                                parts.Remove(parts[parts.Count - 2]);
                        }

                        parts.Add(literal);

                        literal = tmp;
                        for (int i = 0, j = 0; i < literal.Length;)
                        {
                            if (literal[i] == '\n' || literal[i] == '\t')
                            {
                                _tokens.Add(new Token(TokenType.Punctuator, literal[i].ToString().Replace("\n", "\\n").Replace("\t", "\\t")));
                                i++;
                            }
                            else
                            {
                                _tokens.Add(new Token(TokenType.StringLiteral, parts[j]));
                                i += parts[j++].Length;
                            }
                        }

                        _tokens.Add(new Token(TokenType.Operator, Peek().ToString()));
                        _position++;
                    }
                    else
                    {
                        string op = ReadWhileOperator();
                        if (Operators.Contains(op))
                        {
                            _tokens.Add(new Token(TokenType.Operator, op));
                        }
                        else
                        {
                            _tokens.Add(new Token(TokenType.Unknown, op));
                        }
                    }

                    continue;
                }

                // Ну тут либо пунктуатор, либо ошибка
                if (Punctuators.Contains(currentChar.ToString()))
                {
                    _tokens.Add(new Token(TokenType.Punctuator, currentChar.ToString()));
                    _position++;
                    continue;
                }

                //Остраются только ошибки
                _tokens.Add(new Token(TokenType.Unknown, currentChar.ToString()));
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
    }
}
