using Compiler.Lexis;
using Compiler.Syntax;

namespace Compiler.Semantic
{
    public class SemanticAnalyzer
    {
        public List<string> Errors { get; init; } = [];
        // Объявляем начальную область видимости. Она является глобальной
        private Scope _scope = new(null);

        public SemanticAnalyzer()
        {
            //_scope.Functions.Add("printf", new Function(new ValueNode(new Token(TokenType.Keyword, "void"), null),
                //new ValueNode(new Token(TokenType.Identifier, "printf"), null)));
        }

        public void Analyze(Node node)
        {
            if (node is ValueNode)
                return;

            NonTermNode nonTerm = (node as NonTermNode)!;

            if (nonTerm.Type == TypeNode.BlockOfCode 
                && nonTerm.Parent!.Type != TypeNode.FunctionDefinition
                && nonTerm.Parent!.Type != TypeNode.StructureDefinition
                && nonTerm.Parent!.Type != TypeNode.ForStatement)
            {
                _scope = new Scope(_scope);

                foreach (var ch in nonTerm.Children)
                    Analyze(ch);

                _scope = _scope.Parent!;

                return;
            }
            else if (nonTerm.Type == TypeNode.BlockOfCode
                && (nonTerm.Parent!.Type == TypeNode.FunctionDefinition
                    || nonTerm.Parent!.Type == TypeNode.ForStatement))
            {
                foreach (var ch in nonTerm.Children)
                    Analyze(ch);

                _scope = _scope.Parent!;

                return;
            }
            else if (nonTerm.Type == TypeNode.VariableDeclaration)
            {
                NonTermNode type = (nonTerm.Children[0] as NonTermNode)!;
                if (type.Children[0] is ValueNode s && s.Token.Value == "struct")
                    return;

                ValueNode name = (nonTerm.Children[1] as ValueNode)!;

                if (_scope.ContainsName(name.Token.Value, true))
                {
                    Errors.Add($"В данной области имя {name.Token.Value} уже занято");
                }
                else
                {
                    Variable var = new(type, name);
                    _scope.Variables.Add(var.Name, var);
                }
            }
            else if (nonTerm.Type == TypeNode.ArrayDeclaration)
            {
                ValueNode type = (nonTerm.Children[0] as ValueNode)!;
                ValueNode name = (nonTerm.Children[1] as ValueNode)!;
                NonTermNode size = (nonTerm.Children[2] as NonTermNode)!;

                if (_scope.ContainsName(name.Token.Value, true))
                {
                    Errors.Add($"В данной области имя {name.Token.Value} уже существует");
                }
                else
                {
                    Array var = new(type, name, size);
                    _scope.Arrays.Add(var.Name, var);
                }
            }
            else if (nonTerm.Type == TypeNode.FunctionDefinition)
            {
                ValueNode type = (nonTerm.Children[0] as ValueNode)!;
                ValueNode name = (nonTerm.Children[1] as ValueNode)!;
                NonTermNode param = (nonTerm.Children[2] as NonTermNode)!;

                if (_scope.ContainsName(name.Token.Value, true))
                {
                    Errors.Add($"В данной области имя {name.Token.Value} уже существует");
                }
                else
                {
                    List<Variable> pars = new();

                    foreach (NonTermNode p in param.Children)
                    {
                        NonTermNode t = (p.Children[0] as NonTermNode)!;
                        ValueNode n = (p.Children[1] as ValueNode)!;

                        pars.Add(new Variable(t, n));
                    }

                    Function var = new(type, name, pars);
                    _scope.Functions.Add(var.Name, var);
                    _scope = new Scope(_scope);
                    //if (nonTerm.Children.Count > 3)
                    //{
                    //    _scope = new Scope(_scope);
                    //    foreach (var p in pars)
                    //    {
                    //        _scope.Variables.Add(p.Name, p);
                    //    }

                    //    return;
                    //}
                }
            }
            else if (nonTerm.Type == TypeNode.StructureDefinition)
            {
                ValueNode name = (nonTerm.Children[0] as ValueNode)!;
                NonTermNode block = (nonTerm.Children[1] as NonTermNode)!;

                Struct st = new(name);

                foreach (NonTermNode p in block.Children)
                {
                    var v = ParseVar(p);
                    st.AddField(v);
                }
            }
            else if (nonTerm.Type == TypeNode.Variable)
            {
                string name = (nonTerm.Children[0] as ValueNode).Token.Value;
                if (!_scope.VarContains(name))
                {
                    Errors.Add($"Переменная {name} не существует");
                }
            }
            else if (nonTerm.Type == TypeNode.Assignment)
            {
                NonTermNode left = (nonTerm.Children[0] as NonTermNode)!;
                Node right = nonTerm.Children[1];

                if (left.Type == TypeNode.ArrayElement)
                {
                    string arrName = (left.Children[0] as ValueNode)!.Token.Value;
                    
                    if (_scope.ArrContains(arrName))
                    {
                        Array arr = _scope.GetArr(arrName);

                        if (right is ValueNode r)
                        {
                            if (!TryParse(GetType(arr.ArrType.Token.Value), r))
                            {
                                Errors.Add($"Не удалось преобразовать {r.Token.Value}");
                            }
                        }
                        else
                        {
                            TypeExpression(right as NonTermNode, GetType(arr.ArrType.Token.Value));
                        }

                    }
                    else
                    {
                        Errors.Add($"Массив {arrName} не существует");
                    }

                }
                else if (left.Type == TypeNode.Variable)
                {
                    string varName = (left.Children[0] as ValueNode)!.Token.Value;

                    if (_scope.VarContains(varName))
                    {
                        Variable lVar = _scope.GetVariable(varName);
                        if (lVar.IsConstant)
                        {
                            Errors.Add($"Попытка изменить константу {lVar.Name}");
                        }
                        if (right is ValueNode r)
                        {
                            if (!TryParse(GetType(lVar.VarType), r))
                            {
                                Errors.Add($"Не удалось преобразовать {r.Token.Value}");
                            }
                        }
                        else
                        {
                            TypeExpression(right as NonTermNode, GetType(lVar.VarType));
                        }
                    }
                }
            }
            else if (nonTerm.Type == TypeNode.FunctionCalling)
            {
                ValueNode name = (nonTerm.Children[0] as ValueNode)!;
                if (name.Token.Value == "printf")
                    return;

                if (_scope.FuncContains(name.Token.Value))
                {
                    NonTermNode args = (nonTerm.Children[1] as NonTermNode)!;
                    Function f = _scope.GetFunction(name.Token.Value);

                    if (f.Arguments.Count != args.Children.Count)
                    {
                        Errors.Add($"В функцию {name.Token.Value} передано некорректное кол-во аргументов");
                    }
                }
                else
                {
                    Errors.Add($"Функции {name.Token.Value} не существует");
                }

            }
            else if (nonTerm.Type == TypeNode.ForStatement)
            {
                _scope = new Scope(_scope);
            }


            foreach (var ch in nonTerm.Children)
                Analyze(ch);
        }

        private static Variable ParseVar(NonTermNode vNode)
        {
            NonTermNode type = (vNode.Children[0] as NonTermNode)!;
            ValueNode name = (vNode.Children[1] as ValueNode)!;

            return new Variable(type, name);
        }

        private void TypeExpression(NonTermNode exp, Type lType)
        {
            foreach (ValueNode op in exp.Children)
            {
                if (op.Token.Type == TokenType.StringLiteral)
                {
                    Errors.Add($"Мат выражение содержит строку {op.Token.Value}");
                }
                else if (op.Token.Type == TokenType.Constant)
                {
                    if (!TryParse(lType, op))
                    {
                        Errors.Add($"Не удалось преобразовать {op.Token.Value}");
                    }
                }
                else if (op.Token.Type == TokenType.Identifier)
                {
                    if (_scope.VarContains(op.Token.Value))
                    {
                        Variable v = _scope.GetVariable(op.Token.Value);
                        if (lType < GetType(v.VarType))
                        {
                            Errors.Add($"Не удалось преобразовать переменную {op.Token.Value}");
                        }
                    }
                    else
                    {
                        Errors.Add($"Переменная {op.Token.Value} не существует");
                    }
                }
            }
        }


        private enum Type
        {
            Char,
            Int,
            Float,
            Long,
            Double
        }

        private static bool TryParse(Type type, ValueNode node) => type switch
        {
            Type.Int => int.TryParse(node.Token.Value, out int res),
            Type.Long => long.TryParse(node.Token.Value, out long res),
            Type.Float => float.TryParse(node.Token.Value, out float res),
            Type.Double => double.TryParse(node.Token.Value, out double res),
            _ => false
        };

        private static string GetStrType(Type type) => type switch
        {
            Type.Int => "int",
            Type.Long => "long",
            Type.Float => "float",
            Type.Double => "double",
            _ => "int"
        };

        private static Type GetType(string type) => type switch
        {
            "int" => Type.Int,
            "long" => Type.Long,
            "float" => Type.Float,
            "double" => Type.Double,
            _ => Type.Int
        };
    }

    public class SemanticException : Exception
    {
        public SemanticException() : base() { }

        public SemanticException(string message) : base(message) { }
    }
}
