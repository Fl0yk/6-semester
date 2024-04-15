using Compiler.Lexis;

namespace Compiler.Syntax
{
    public class SyntaxAnalyzer
    {
        private int _curInd = 0;
        private List<Token> _tokens;
        private HashSet<string> _types = ["char", "double", "float", "int", "long", "short", "signed", "void"];


        public SyntaxAnalyzer(IEnumerable<Token> tokens)
        { 
            _tokens = tokens.ToList();
        }

        public Node GetSyntaxTree()
        {
            NonTermNode root = new(TypeNode.Program, null);
            Node curNode = root;

            for (int i = 0; i < _tokens.Count; i++)
            {
                // Объявдение структуры или переменной типаа структуры
                if (_tokens[i].Type == TokenType.Keyword && _tokens[i].Value == "struct")
                {
                    if (i + 2 >= _tokens.Count)
                        throw new SyntaxException("Некорректное объявление структуры");

                    // Объявление структуры
                    if (_tokens[i + 2].Type == TokenType.Punctuator)
                    {
                        curNode = new NonTermNode(TypeNode.StructureDefinition, curNode);
                        _ = new ValueNode(_tokens[i + 1], curNode);
                        continue;
                    }
                    // Объяявление переменной типа структуры
                    else if (_tokens[i + 2].Type == TokenType.Identifier)
                    {
                        curNode = new NonTermNode(TypeNode.VariableDeclaration, curNode);
                        Node tmp = new NonTermNode(TypeNode.ValueType, curNode);
                        _ = new ValueNode(_tokens[i], tmp);
                        _ = new ValueNode(_tokens[i + 1], tmp);
                        continue;
                    }
                }

                if (_tokens[i].Value == "}" && curNode.Parent!.Type == TypeNode.StructureDefinition)
                {
                    curNode = curNode.Parent;
                    continue;
                }

                if (_tokens[i].Value == ";" && curNode.Type == TypeNode.StructureDefinition)
                {
                    curNode = curNode.Parent ?? throw new SyntaxException("Ошибка при объявлении структуры");
                    continue;
                }

                // Объявление функци
                if (i + 2 < _tokens.Count 
                        && _types.Contains(_tokens[i].Value) 
                        && _tokens[i + 1].Type == TokenType.Identifier 
                        && _tokens[i + 2].Value == "(")
                {
                    curNode = new NonTermNode(TypeNode.FunctionDefinition, curNode);
                    _ = new ValueNode(_tokens[i], curNode);
                    _ = new ValueNode(_tokens[++i], curNode);
                    continue;
                }

                if (_tokens[i].Value == "(" && curNode.Type == TypeNode.FunctionDefinition)
                {
                    curNode = new NonTermNode(TypeNode.FunctionParameters, curNode);
                    continue;
                }

                if (_tokens[i].Value == ")" && curNode.Type == TypeNode.FunctionParameters)
                {
                    curNode = curNode.Parent!;
                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Type == TypeNode.FunctionDefinition)
                {
                    curNode = curNode.Parent ?? throw new SyntaxException("Ошибка при объявлении функции");
                    continue;
                }

                // Объявление массива
                if (i + 2 < _tokens.Count 
                        && _types.Contains(_tokens[i].Value) 
                        && _tokens[i + 1].Type == TokenType.Identifier 
                        && _tokens[i + 2].Value == "[")
                {
                    curNode = new NonTermNode(TypeNode.ArrayDeclaration, curNode);
                    _ = new ValueNode(_tokens[i], curNode);
                    _ = new ValueNode(_tokens[++i], curNode);
                    continue;
                }

                if (_tokens[i].Value == "[" && curNode.Type == TypeNode.ArrayDeclaration)
                {
                    curNode = new NonTermNode(TypeNode.ArraySize, curNode);

                    if (MathExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        MathExpression.AsNode(_tokens, ref i, curNode);
                        i--;
                    }

                    continue;
                }

                if (_tokens[i].Value == "]" && curNode.Type == TypeNode.ArraySize)
                {
                    curNode = curNode.Parent ?? throw new SyntaxException("Ошибка при объявлении массива");
                    continue;
                }

                if (_tokens[i].Value == ";" && curNode.Type == TypeNode.ArrayDeclaration)
                {
                    curNode = curNode.Parent ?? throw new SyntaxException("Ошибка при объявлении массива");
                    continue;
                }

                // Объявление переменной
                // Если мы не начали объявлять переменную и встретили тип
                if (i + 1 < _tokens.Count 
                        && (_types.Contains(_tokens[i].Value) || _tokens[i].Value == "const")
                        && curNode.Type != TypeNode.VariableDeclaration && curNode.Type != TypeNode.ValueType)
                {
                    curNode = new NonTermNode(TypeNode.VariableDeclaration, curNode);
                    var tmp = new NonTermNode(TypeNode.ValueType, curNode);
                    _ = new ValueNode(_tokens[i], tmp);
                    curNode = tmp;
                    continue;
                }

                // Если мы начали объявлять переменную и встретили тип
                if (i + 1 < _tokens.Count
                        && (_types.Contains(_tokens[i].Value) || _tokens[i].Value == "const")
                        && curNode.Type == TypeNode.ValueType)
                {
                    _ = new ValueNode(_tokens[i], curNode);
                    continue;
                }

                if (_tokens[i].Type == TokenType.Identifier && curNode.Type == TypeNode.ValueType)
                {
                    curNode = curNode.Parent ?? throw new SyntaxException("Некорректное объявление переменной");
                    _ = new ValueNode(_tokens[i], curNode);
                    continue;
                }

                if (_tokens[i].Value == ";" && curNode.Type == TypeNode.VariableDeclaration)
                {
                    curNode = curNode.Parent ?? throw new SyntaxException("Некорректное объявление переменной");
                    if (curNode is OperatorNode)
                    {
                        curNode = curNode.Parent ?? throw new SyntaxException("Некорректное объявление переменной");
                    }
                    continue;
                }

                if (_tokens[i].Value == "," 
                    && curNode.Type == TypeNode.VariableDeclaration 
                    && curNode.Parent!.Type == TypeNode.FunctionParameters)
                {
                    curNode = curNode.Parent ?? throw new SyntaxException("Некорректное объявление переменной");
                }

                if (_tokens[i].Value == ")"
                    && curNode.Type == TypeNode.VariableDeclaration
                    && curNode.Parent!.Type == TypeNode.FunctionParameters)
                {
                    curNode = curNode.Parent?.Parent ?? throw new SyntaxException("Некорректное объявление переменной");
                }


                #region Присваивание
                if (_tokens[i].Value == "=" 
                    && (curNode.Type == TypeNode.ArrayDeclaration || curNode.Type == TypeNode.VariableDeclaration))
                {
                    Node parent = curNode.Parent!;

                    if (parent is NonTermNode p)
                    {
                        p.Children.Remove(curNode);
                    }

                    var oper = new OperatorNode(TypeNode.Assignment, _tokens[i], parent);
                    oper.Children.Add(curNode);
                    curNode.Parent = oper;
                    curNode = oper;

                    if (i + 1 >= _tokens.Count)
                    {
                        throw new SyntaxException("После равно нет правой части");
                    }

                    if (_tokens[i + 1].Value == "{")
                        continue;

                    if (MathExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        MathExpression.AsNode(_tokens, ref i, oper);
                        i--;
                    }

                    continue;
                }

                if (_tokens[i].Value == "=" && curNode is NonTermNode n2 
                    && (n2.Children.Last().Type == TypeNode.Variable || n2.Children.Last().Type == TypeNode.ArrayElement))
                {
                    var l = n2.Children.Last();
                    n2.Children.Remove(l);
                    var oper = new OperatorNode(TypeNode.Assignment, _tokens[i], curNode);
                    oper.Children.Add(l);
                    curNode = oper;

                    if (_tokens[i + 1].Value == "{")
                        continue;

                    if (MathExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        MathExpression.AsNode(_tokens, ref i, oper);
                        i--;
                    }

                    continue;
                }

                if (_tokens[i].Value == ";" && curNode.Type == TypeNode.Assignment)
                {
                    curNode = curNode.Parent!;
                    continue;
                }

                #endregion

                // Набор данных
                if (_tokens[i].Value == "{" && curNode.Type == TypeNode.Assignment)
                {
                    curNode = new NonTermNode(TypeNode.DataSet, curNode);
                    if (MathExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        MathExpression.AsNode(_tokens, ref i, curNode);
                        i--;
                    }

                    continue;
                }

                if (_tokens[i].Value == "," && curNode.Type == TypeNode.DataSet)
                {
                    if (MathExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        MathExpression.AsNode(_tokens, ref i, curNode);
                        i--;
                    }
                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Type == TypeNode.DataSet)
                {
                    curNode = curNode.Parent!;
                    continue;
                }

                # region if | if else | else состояния
                if (_tokens[i].Value == "if")
                {
                    NonTermNode ifElseRoot = new NonTermNode(TypeNode.IfElseStatement, curNode);
                    NonTermNode ifRoot = new NonTermNode(TypeNode.IfStatement, ifElseRoot);
                    NonTermNode condition = new NonTermNode(TypeNode.ConditionStatement, ifRoot);

                    if (_tokens[i + 1].Value != "(")
                        throw new SyntaxException("If 1 (нужна скобка)");
                    i++;

                    if (LogicalExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        LogicalExpression.AsNode(_tokens, ref i, condition);
                    }
                    else
                    {
                        throw new SyntaxException("If 2 (нужно логическое выражение)");
                    }

                    if (_tokens[i].Value != ")")
                        throw new SyntaxException("If 3 (нужна закрывающая скобка)");

                    curNode = ifRoot;

                    continue;
                }

                if (i + 1 < _tokens.Count 
                    && _tokens[i].Value == "else"
                    && _tokens[i + 1].Value == "if"
                    && curNode.Type == TypeNode.IfElseStatement)
                {
                    i++;
                    NonTermNode ifRoot = new NonTermNode(TypeNode.ElseIfStatement, curNode);
                    NonTermNode condition = new NonTermNode(TypeNode.ConditionStatement, ifRoot);

                    if (_tokens[i + 1].Value != "(")
                        throw new SyntaxException("If 1 (нужна скобка)");
                    i++;

                    if (LogicalExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        LogicalExpression.AsNode(_tokens, ref i, condition);
                    }
                    else
                    {
                        throw new SyntaxException("If 2 (нужно логическое выражение)");
                    }

                    if (_tokens[i].Value != ")")
                        throw new SyntaxException("If 3 (нужна закрывающая скобка)");

                    curNode = ifRoot;

                    continue;
                }

                if (_tokens[i].Value == "else")
                {
                    curNode = new NonTermNode(TypeNode.ElseStatement, curNode);

                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Parent.Type == TypeNode.ElseStatement)
                {
                    curNode = curNode.Parent!.Parent!.Parent!;
                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Parent.Parent.Type == TypeNode.IfElseStatement)
                {
                    // Если дальше есть else, то возвращаемся до IfElseStatement
                    if (_tokens[i + 1].Value == "else")
                    {
                        curNode = curNode.Parent!.Parent!;
                    }
                    // Если нету, то польностью выходим из IfElseStatement
                    else
                    {
                        curNode = curNode.Parent!.Parent!.Parent!;
                    }

                    continue;
                }

                #endregion


                #region Вызов функции
                if (i + 1 < _tokens.Count 
                    && _tokens[i].Type == TokenType.Identifier 
                    && _tokens[i + 1].Value == "("
                    && new TypeNode[] {TypeNode.BlockOfCode, TypeNode.CaseCode, TypeNode.DefaultCode, TypeNode.Assignment }.Contains(curNode.Type))
                {
                    curNode = new NonTermNode(TypeNode.FunctionCalling, curNode);
                    _ = new ValueNode(_tokens[i], curNode);
                    continue;
                }

                if (_tokens[i].Value == "(" && curNode.Type == TypeNode.FunctionCalling)
                {
                    var arg = new NonTermNode(TypeNode.FunctionArguments, curNode);

                    i++;
                    while (MathExpression.IsMatch(_tokens, i) || _tokens[i].Value == "\"")
                    {
                        if (_tokens[i].Value == "\"")
                        {
                            var str = new NonTermNode(TypeNode.StringStatement, arg);
                            while (_tokens[++i].Value != "\"")
                            {
                                _ = new ValueNode(_tokens[i], str);
                            }
                            i++;
                        }
                        else
                        {
                            //i++;
                            var node = MathExpression.AsNode(_tokens, ref i, arg);

                            if (_tokens[i].Value == ".")
                            {
                                arg.Children.Remove(node);
                                var tmp = new NonTermNode(TypeNode.MemberAccess, arg);
                                tmp.Children.Add(node);
                                node.Parent = tmp;

                                if (_tokens[++i].Type != TokenType.Identifier)
                                    throw new SyntaxException("Поле должно быть идентификатором");

                                _ = new ValueNode(_tokens[i], tmp);
                                i++;
                            }

                            if (_tokens[i].Value == "[")
                            {
                                arg.Children.Remove(node);
                                var tmp = new NonTermNode(TypeNode.ArrayElement, arg);
                                tmp.Children.Add(node);
                                node.Parent = tmp;

                                if (MathExpression.IsMatch(_tokens, i + 1))
                                {
                                    var indNode = new NonTermNode(TypeNode.ArrayIndex, tmp);
                                    i++;
                                    MathExpression.AsNode(_tokens, ref i, indNode);
                                }

                                if (_tokens[i].Value == "]")
                                {
                                    i++;
                                }
                                else
                                {
                                    throw new SyntaxException("???");
                                }
                            }

                            if (_tokens[i].Value != "," && _tokens[i].Value != ")")
                                throw new SyntaxException("Некорректный вызов функции 1");
                            //i++;
                        }

                        if (_tokens[i].Value == ",")
                            i++;
                    }
                    //i--;

                    if (_tokens[i].Value == ")")
                        curNode = curNode.Parent ?? throw new SyntaxException("Некорректный вызов функции 2");
                    else
                        throw new SyntaxException("Некорректный вызов функции 3");
                    continue;
                }

                #endregion

                //Состояние строки
                if (_tokens[i].Value == "\"")
                {
                    var strN = new NonTermNode(TypeNode.StringStatement, curNode);

                    while (_tokens[++i].Value != "\"")
                    {
                        _ = new ValueNode(_tokens[i], strN);
                    }
                    continue;
                }

                //Состояние символа
                if (i + 2 < _tokens.Count 
                        && _tokens[i].Value == "'" 
                        && _tokens[i + 2].Value == "'")
                {
                    var chN = new NonTermNode(TypeNode.CharStatement, curNode);
                    _ = new ValueNode(_tokens[i + 1], chN);
                    i += 2;
                    continue;
                }

                //Доступ к полям
                if (i + 1 < _tokens.Count 
                        && _tokens[i].Type == TokenType.Identifier 
                        && _tokens[i + 1].Value == ".")
                {
                    curNode = new NonTermNode(TypeNode.MemberAccess, curNode);
                    _ = new ValueNode(_tokens[i], curNode);
                    continue;
                }

                if (i - 1 >= 0 
                        && _tokens[i].Type == TokenType.Identifier 
                        && _tokens[i - 1].Value == ".")
                {
                    _ = new ValueNode(_tokens[i], curNode);
                    curNode = curNode.Parent!;
                    continue;
                }

                //Состояние return
                if (_tokens[i].Value == "return")
                {
                    curNode = new NonTermNode(TypeNode.ReturnStatement, curNode);

                    if (MathExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        MathExpression.AsNode(_tokens, ref i, curNode);
                        i--;
                    }

                    continue;
                }

                // Инкремент
                if (_tokens[i].Value == "++"
                    && curNode is NonTermNode n3
                    && (n3.Children.Last().Type == TypeNode.Variable || n3.Children.Last().Type == TypeNode.ArrayElement))
                {
                    var operand = n3.Children.Last();
                    n3.Children.Remove(operand);

                    NonTermNode inc = new NonTermNode(TypeNode.Increment, curNode);
                    operand.Parent = inc;
                    inc.Children.Add(operand);
                }

                if (_tokens[i].Value == ";" && curNode.Type == TypeNode.ReturnStatement)
                {
                    curNode = curNode.Parent!;
                    continue;
                }

                #region Loops
                // for
                if (_tokens[i].Value == "for")
                {
                    NonTermNode forRoot = new NonTermNode(TypeNode.ForStatement, curNode);

                    if (_tokens[++i].Value != "(")
                        throw new SyntaxException("Некорректный for 1");

                    // Объявление переменной для цикла
                    if (_types.Contains(_tokens[i + 1].Value))
                    {
                        // Создаем узлы и запихиваем тип
                        OperatorNode ass = new OperatorNode(TypeNode.Assignment, new Token(TokenType.Operator, "="), forRoot);
                        NonTermNode varDecl = new NonTermNode(TypeNode.VariableDeclaration, ass);
                        NonTermNode varT = new NonTermNode(TypeNode.ValueType, varDecl);
                        _ = new ValueNode(_tokens[++i], varT);

                        if (_tokens[i + 1].Type != TokenType.Identifier)
                            throw new SyntaxException("Некорректный цикл 2");

                        _ = new ValueNode(_tokens[++i], varDecl);

                        if (_tokens[i + 1].Value != "=")
                            throw new SyntaxException("Некорректный цикл 3");
                        i++;
                        
                        if (MathExpression.IsMatch(_tokens, i + 1))
                        {
                            i++;
                            MathExpression.AsNode(_tokens, ref i, ass);
                        }
                    }

                    if (_tokens[i].Value != ";")
                        throw new SyntaxException("Некорректный цикл 4");
                    i++;

                    // Логическое условие цикла
                    NonTermNode condition = new NonTermNode(TypeNode.ConditionStatement, forRoot);

                    if (LogicalExpression.IsMatch(_tokens, i))
                    {
                        LogicalExpression.AsNode(_tokens, ref i, condition);
                    }

                    if (_tokens[i].Value != ";")
                        throw new SyntaxException("Некорректный цикл 5");

                    //Инкремент
                    if (_tokens[i + 1].Type != TokenType.Identifier)
                        throw new SyntaxException("Некорректный цикл 6");
                    i++;
                    Token varToken = _tokens[i];

                    if (_tokens[i + 1].Value != "++")
                        throw new SyntaxException("Некорректный цикл 7");
                    i++;

                    NonTermNode inc = new NonTermNode(TypeNode.Increment, forRoot);
                    NonTermNode var = new NonTermNode(TypeNode.Variable, inc);
                    _ = new ValueNode(varToken, var);

                    if (_tokens[i + 1].Value != ")")
                        throw new SyntaxException("Некорректный цикл 8");
                    curNode = forRoot;

                    continue;
                }

                //while
                if (_tokens[i].Value == "while" && curNode.Type != TypeNode.DoWhileStatement)
                {
                    curNode = new NonTermNode(TypeNode.Whiletatement, curNode);
                    if (_tokens[i + 1].Value != "(")
                        throw new SyntaxException("Цикл while 1");
                    i++;

                    NonTermNode condition = new NonTermNode(TypeNode.ConditionStatement, curNode);

                    if (LogicalExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        LogicalExpression.AsNode(_tokens, ref i, condition);
                    }

                    if (_tokens[i].Value != ")")
                        throw new SyntaxException("Цикл while 2");

                    if (_tokens[i + 1].Value == ";")
                        throw new SyntaxException("Цикл while 3 (мб do-while)");

                    continue;
                }

                //do-while
                if (_tokens[i].Value == "do")
                {
                    curNode = new NonTermNode(TypeNode.DoWhileStatement, curNode);

                    continue;
                }

                if (_tokens[i].Value == "while")
                {
                    if (_tokens[i + 1].Value != "(")
                        throw new SyntaxException("Цикл do-while 1");
                    i++;

                    NonTermNode condition = new NonTermNode(TypeNode.ConditionStatement, curNode);

                    if (LogicalExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        LogicalExpression.AsNode(_tokens, ref i, condition);
                    }

                    if (_tokens[i].Value != ")")
                        throw new SyntaxException("Цикл do-while 2");

                    if (_tokens[i + 1].Value != ";")
                        throw new SyntaxException("Цикл do-while 3 (мб while)");

                    curNode = curNode.Parent!;

                    continue;
                }

                if (_tokens[i].Value == "}" &&
                        (curNode.Parent.Type == TypeNode.ForStatement
                            || curNode.Parent.Type == TypeNode.Whiletatement))
                {
                    curNode = curNode.Parent.Parent ?? throw new SyntaxException("Некорректный цикл");
                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Parent.Type == TypeNode.DoWhileStatement)
                {
                    curNode = curNode.Parent ?? throw new SyntaxException("Некорректный цикл");
                    continue;
                }
                #endregion


                #region Switch case
                if (_tokens[i].Value == "switch")
                {
                    NonTermNode switchRoot = new NonTermNode(TypeNode.SwitchStatement, curNode);
                    NonTermNode condition = new NonTermNode(TypeNode.SwitchCondition, switchRoot);

                    if (_tokens[i + 1].Value != "(")
                        throw new SyntaxException("switch 1");
                    i++;
                    
                    if (MathExpression.IsMatch(_tokens, i + 1))
                    {
                        i++;
                        MathExpression.AsNode(_tokens, ref i, condition);
                    }
                    else
                    {
                        throw new SyntaxException("switch 2 (надо мат выражение)");
                    }

                    if (_tokens[i].Value != ")")
                        throw new SyntaxException("switch 3 (скобка не закрыта)");

                    curNode = switchRoot;

                    continue;
                }

                if (_tokens[i].Value == "case" && curNode.Type == TypeNode.CaseCode)
                {
                    curNode = curNode.Parent!.Parent!;
                }

                if (_tokens[i].Value == "case" && curNode.Parent.Type == TypeNode.SwitchStatement)
                {
                    NonTermNode caseRoot = new NonTermNode(TypeNode.CaseStatement, curNode);
                    NonTermNode condition = new NonTermNode(TypeNode.CaseCondition, caseRoot);

                    if (_tokens[i + 1].Value == "'")
                    {
                        i++;
                        var chN = new NonTermNode(TypeNode.CharStatement, condition);
                        _ = new ValueNode(_tokens[i + 1], chN);
                        i += 2;

                        if (_tokens[i].Value != "'")
                            throw new SyntaxException("case 1 (Некорректный символ)");
                        i++;
                    }
                    else if (_tokens[i + 1].Type == TokenType.Constant)
                    {
                        _ = new ValueNode(_tokens[++i], condition);
                        i++;
                    }
                    else
                    {
                        throw new SyntaxException("case 2 (нужна константа или char литерал)");
                    }

                    if (_tokens[i].Value != ":")
                        throw new SyntaxException("case 2 (после case надо :)");

                    curNode = new NonTermNode(TypeNode.CaseCode, caseRoot);

                    continue;
                }

                if (_tokens[i].Value == "break" && curNode.Type == TypeNode.CaseCode)
                {
                    _ = new ValueNode(_tokens[i], curNode);
                    curNode = curNode.Parent!.Parent!;

                    continue;
                }


                if (_tokens[i].Value == "default" && curNode.Parent.Type == TypeNode.SwitchStatement)
                {
                    NonTermNode defaultRoot = new NonTermNode(TypeNode.DefaultStatement, curNode);

                    if (_tokens[i + 1].Value != ":")
                        throw new SyntaxException("Switch default 1 (после default нужнао :)");

                    curNode = new NonTermNode(TypeNode.DefaultCode, defaultRoot);

                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Type == TypeNode.DefaultCode)
                {
                    curNode = curNode.Parent!.Parent!.Parent!.Parent!;

                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Type == TypeNode.CaseCode)
                {
                    curNode = curNode.Parent!.Parent!.Parent!.Parent!;

                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Parent.Type == TypeNode.SwitchStatement)
                {
                    curNode = curNode.Parent!.Parent!;
                    continue;
                }
                #endregion

                //Элементы массива
                if (i - 1 >= 0 && _tokens[i].Value == "[" && _tokens[i-1].Type == TokenType.Identifier)
                {
                    Node tmp;
                    if (curNode is NonTermNode n)
                    {
                        tmp = n.Children[^ 1];
                        n.Children.Remove(tmp);
                        tmp = (tmp as NonTermNode)?.Children[0] ?? throw new SyntaxException("Некорректное использование индексов 1");
                    }
                    else
                    {
                        throw new SyntaxException("Некорректное использование индексов 2");
                    }

                    var node = new NonTermNode(TypeNode.ArrayElement, curNode);
                    tmp.Parent = node;
                    node.Children.Add(tmp);
                    curNode = node;

                    continue;
                }

                if ((_tokens[i].Type == TokenType.Identifier || _tokens[i].Type == TokenType.Constant) 
                    && curNode.Type == TypeNode.ArrayElement)
                {
                    var node = new NonTermNode(TypeNode.ArrayIndex, curNode);
                    _ = new ValueNode(_tokens[i], node);
                    curNode = node;
                    continue;
                }

                if (_tokens[i].Value == "]" && curNode.Type == TypeNode.ArrayIndex)
                {
                    curNode = curNode.Parent?.Parent ?? throw new SyntaxException("Некорректная индексация");
                    continue;
                }

                if (_tokens[i].Value == "{")
                {
                    curNode = new NonTermNode(TypeNode.BlockOfCode, curNode);
                    continue;
                }

                if (_tokens[i].Value == "}" && curNode.Parent.Type == TypeNode.FunctionDefinition)
                {
                    curNode = curNode.Parent?.Parent ?? throw new SyntaxException("Некорректная индексация");
                    continue;
                }

                if (_tokens[i].Type == TokenType.Identifier &&  
                    !(new TypeNode[] {TypeNode.VariableDeclaration, TypeNode.StructureDefinition, 
                        TypeNode.ArrayDeclaration, TypeNode.FunctionDefinition}.Contains(curNode.Type)))
                {
                    var node = new NonTermNode(TypeNode.Variable, curNode);
                    _ = new ValueNode(_tokens[i], node);
                    continue;
                }

                if ((_tokens[i].Value == "break" || _tokens[i].Value == "continue") 
                        && (curNode.Parent.Type == TypeNode.DoWhileStatement 
                            || curNode.Parent.Type == TypeNode.Whiletatement
                            || curNode.Parent.Type == TypeNode.ForStatement))
                {
                    var node = new NonTermNode(TypeNode.LoopKeyWord, curNode);
                    _ = new ValueNode(_tokens[i], node);
                    continue;
                }
            }

            return root;
        }

        public static void WriteTree(Node node, String indent, bool last)
        {
            if (node is ValueNode tokenNode)
            {
                Console.WriteLine(indent + "+- " + tokenNode.Token.ToString());
            }
            else if (node is OperatorNode operatorNode)
            {
                Console.WriteLine(indent + "+- " + operatorNode.Operator.ToString());
                indent += last ? "   " : "|  ";

                for (int i = 0; i < operatorNode.Children.Count; i++)
                {
                    WriteTree(operatorNode.Children[i], indent, i == operatorNode.Children.Count - 1);
                }
            }
            else if (node is NonTermNode parentNode)
            {
                Console.WriteLine(indent + "+- " + Enum.GetName<TypeNode>(parentNode.Type));
                indent += last ? "   " : "|  ";

                for (int i = 0; i < parentNode.Children.Count; i++)
                {
                    WriteTree(parentNode.Children[i], indent, i == parentNode.Children.Count - 1);
                }

            }
        }
    }

    public class SyntaxException : Exception
    {
        public SyntaxException() : base() { }

        public SyntaxException(string message) : base(message) { }
    }
}
