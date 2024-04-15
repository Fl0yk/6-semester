using Compiler.Lexis;
using System.Net.Http.Headers;

namespace Compiler.Syntax
{
    /// <summary>
    /// Класс для парсинга математических выражений.
    /// Если выражение состоит из одного операнда без операторов,
    /// то выражение преобразуется ValueNode с данным токеном
    /// </summary>
    public static class MathExpression
    {
        public static bool IsMatch(List<Token> tokens, int i, bool isLog = false)
        {
            try
            {
                int start = i;
                var res = GetExpression(tokens, ref start, isLog);

                if (res.Count == 0)
                    return false;

                return true;
            }
            catch 
            { 
                return false; 
            }
        }

        public static Node AsNode(List<Token> tokens, ref int i, Node parent, bool isLog = false)
        {
            int start = i;
            List<Token> result = GetExpression(tokens, ref start, isLog);
            i = start;
            if (result.Count == 1)
            {
                return new ValueNode(result[0], parent);
            }

            NonTermNode exp = new(TypeNode.MathExpression, parent);

            foreach (var token in result)
            {
                _ = new ValueNode(token, exp);
            }

            return exp;
        }

        static public List<Token> GetExpression(List<Token> tokens, ref int i, bool isLog = false)
        {
            List<Token> output = [];
            Stack<Token> operStack = new();

            for (; i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.Constant || tokens[i].Type == TokenType.Identifier
                        || tokens[i].Type == TokenType.StringLiteral || "\n\t".Contains(tokens[i].Value))
                {
                    output.Add(tokens[i]);
                }
                else if (IsOperator(tokens[i]))
                {
                    if (tokens[i].Value == "(")
                        operStack.Push(tokens[i]);
                    else if (tokens[i].Value == ")")
                    {
                        if (operStack.Count == 0)
                            return output;

                        Token s = operStack.Pop();

                        while (s.Value != "(")
                        {
                            output.Add(s);
                            if (operStack.Count == 0)
                                return output;

                            s = operStack.Pop();
                        }
                    }
                    else
                    {
                        if (operStack.Count > 0)
                            if (GetPriority(tokens[i]) <= GetPriority(operStack.Peek()))
                                output.Add(operStack.Pop());

                        operStack.Push(tokens[i]);

                    }
                }
                else
                {
                    while (operStack.Count > 0)
                        output.Add(operStack.Pop());

                    return output;
                }
            }

            // Метод выйдет из цикла только в том случае, если
            // программа заканчивается выражением.
            // А так точно быть не должно
            throw new Exception("???");
        }

        private static int GetPriority(Token token) => token.Value switch
        {
            "(" => 0,
            ")" => 1,
            "+" => 2,
            "-" => 2,
            "*" => 3,
            "/" => 3,
            "%" => 3,
            _ => 5
        };

        private static bool IsOperator(Token token)
        {
            return "()+-*/%".Contains(token.Value[0]);
        }
    }


    /// <summary>
    /// Класс для обработки логических выражений.
    /// Тоже преобразует логические выражения в ОПЗ следующим образом:
    /// операнды - математические выражения (либо обычная константа/идентификатор).
    /// Между ними по приоритету < > <= >= && || (т.е. сначала больше/меньше, а затем логические)
    /// </summary>
    public static class LogicalExpression
    {
        public static bool IsMatch(List<Token> tokens, int i)
        {
            try
            {
                int start = i;
                var res = GetExpression(tokens, ref start);

                if (res.Count == 0)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Node AsNode(List<Token> tokens, ref int i, Node parent)
        {
            int start = i;
            List<Node> result = GetExpression(tokens, ref start);

            NonTermNode exp = new(TypeNode.LogicalExpression, parent);

            foreach (var node in result)
            {
                exp.Children.Add(node);
                node.Parent = exp;
            }

            i = start;

            return exp;
        }

        static private List<Node> GetExpression(List<Token> tokens, ref int i)
        {
            List<Node> output = [];
            Stack<Token> operStack = new();
            int start = i;

            for (; i < tokens.Count;)
            {
                if (MathExpression.IsMatch(tokens, i, true))
                {
                    output.Add(MathExpression.AsNode(tokens, ref i, null, true));
                }
                else if (IsOperator(tokens[i]))
                {
                    if (tokens[i].Value == "(")
                        operStack.Push(tokens[i]);
                    else if (tokens[i].Value == ")")
                    {
                        if (operStack.Count == 0)
                            return output;

                        Token s = operStack.Pop();

                        while (s.Value != "(")
                        {
                            output.Add(new ValueNode(s, null));
                            // Мы можем наткнуться на закрывающую скобку в ифе
                            if (operStack.Count == 0)
                                return output;
                            s = operStack.Pop();
                        }
                    }
                    else
                    {
                        if (operStack.Count > 0)
                            if (GetPriority(tokens[i]) <= GetPriority(operStack.Peek()))
                                output.Add(new ValueNode(operStack.Pop(), null));

                        operStack.Push(tokens[i]);

                    }

                    i++;
                }
                else
                {
                    while (operStack.Count > 0)
                        output.Add(new ValueNode(operStack.Pop(), null));

                    return output;
                }
            }

            // Метод выйдет из цикла только в том случае, если
            // программа заканчивается выражением.
            // А так точно быть не должно
            throw new Exception("???");
        }

        private static int GetPriority(Token token) => token.Value switch
        {
            "(" => 0,
            ")" => 1,
            "&&" => 2,
            "||" => 2,
            "==" => 3,
            "!=" => 3,
            "<" => 3,
            "<=" => 3,
            ">" => 3,
            ">=" => 3,
            _ => 5
        };

        private static bool IsOperator(Token token)
        {
            return new List<string>(["(", ")", "==", "!=", "<", "<=", ">", ">=", "&&", "||"]).Contains(token.Value);
        }
    }
}
