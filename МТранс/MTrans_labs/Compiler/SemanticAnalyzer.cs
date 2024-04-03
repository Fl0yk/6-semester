using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Compiler.Compiler;

namespace Compiler
{
    public static class SemanticAnalyzer
    {
        public static void CheckSemantic(Node root)
        {
            CheckLabelsSemantic(root);

            CheckVariableUsageSemantic(root);

        }


        #region Exception
        public class SemanticException : Exception
        {
            public SemanticException()
                : base()
            { }

            public SemanticException(string message)
                : base(message)
            { }

            public SemanticException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }
        #endregion

        private static void CheckLabelsSemantic(Node root)
        {
            List<Token> usedLabels = [];
            List<Token> declaredLabels = [];

            GetLabelsInfo(root, declaredLabels, usedLabels);


            var repetitiveDeclaredLabels = declaredLabels
                .GroupBy(l => l.Id)
                .Where(g => g.Count() > 1)
                .Select(y => y.First());

            if (repetitiveDeclaredLabels.Any())
                throw new SemanticException($"The same label is declared more than once: {repetitiveDeclaredLabels.First().Value}");

            var usedUndeclaredLabels = usedLabels.Except(declaredLabels);

            if (usedUndeclaredLabels.Any())
                throw new SemanticException($"Used undeclared label: {usedUndeclaredLabels.First().Value}");


            static void GetLabelsInfo(Node root, List<Token> declaredLabels, List<Token> usedLabels)
            {
                if (root is OperatorNode operatorNode)
                {
                    if (operatorNode.Operator == "Label")
                    {
                        var declaredLabel = ((ValueNode)operatorNode.Children.First()).Token;

                        declaredLabels.Add(declaredLabel);
                    }
                    else if (operatorNode.Operator == "Go to")
                    {
                        var usedLabel = ((ValueNode)operatorNode.Children.First()).Token;

                        usedLabels.Add(usedLabel);
                    }
                    else
                    {
                        foreach (var childNode in operatorNode.Children)
                        {
                            GetLabelsInfo(childNode, declaredLabels, usedLabels);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }


        private static void CheckVariableUsageSemantic(Node root)
        {
            CheckVariableUsageSemantic(root, new ScopeInfo() { Root = root });


            static void CheckVariableUsageSemantic(Node root, ScopeInfo scope)
            {
                if (root is OperatorNode operatorNode)
                {
                    string @operator = operatorNode.Operator;

                    // Функция уже реализована и функцией заменили переменную
                    if (@operator == "Function declaration")
                    {
                        Node type = operatorNode.Children.ToArray()[0];
                        Token funcName = ((ValueNode)operatorNode.Children.ToArray()[1]).Token;

                        Node @params = operatorNode.Children.ToArray()[2];

                        var allParamsScope = new ScopeInfo() { Structs = scope.AncestorsStructs };

                        IEnumerable<IdentifierInfo> paramsInfo = ((OperatorNode)@params).Children.Select(p =>
                        {
                            var paramsScope = new ScopeInfo() { Structs = scope.AncestorsStructs };

                            CheckVariableUsageSemantic(p, paramsScope);
                            CheckVariableUsageSemantic(p, allParamsScope);

                            return paramsScope.Identifiers.Values.First();
                        });

                        if (scope.Identifiers.TryGetValue(funcName.Id, out IdentifierInfo? info))
                        {
                            if (info is FuncInfo funcInfo)
                            {
                                if (funcInfo.Value is not null)
                                    throw new SemanticException($"This function already defined.");
                            }
                            else
                            {
                                throw new SemanticException($"Function replaced by variable");
                            }
                        }

                        Node body = operatorNode.Children.ToArray()[3];

                        scope.Identifiers[funcName.Id] = new FuncInfo
                        {
                            Token = funcName,
                            Type = new TypeInfo(type, scope.AncestorsStructs),
                            argsInfo = paramsInfo.ToList(),
                            Value = body
                        };

                        var paramsScope = new ScopeInfo() { ParentScope = scope };

                        CheckVariableUsageSemantic(@params, paramsScope);


                        paramsScope.Root = operatorNode;

                        CheckVariableUsageSemantic(body, paramsScope);
                    }
                    // Функция определена и функцией заменили переменную
                    else if (@operator == "Function prototype")
                    {
                        Node type = operatorNode.Children.ToArray()[0];
                        Token funcName = ((ValueNode)operatorNode.Children.ToArray()[1]).Token;

                        Node @params = operatorNode.Children.ToArray()[2];

                        IEnumerable<IdentifierInfo> paramsInfo = ((OperatorNode)@params).Children.Select(p =>
                        {
                            var paramsScope = new ScopeInfo() { Structs = scope.AncestorsStructs };

                            CheckVariableUsageSemantic(p, paramsScope);

                            return paramsScope.Identifiers.Values.First();
                        });

                        if (scope.Identifiers.TryGetValue(funcName.Id, out IdentifierInfo? info))
                        {
                            if (info is FuncInfo funcInfo)
                            {
                                if (funcInfo.Value is not null)
                                    throw new SemanticException($"This function already defined.");
                            }
                            else
                            {
                                throw new SemanticException($"Function replaced by variable");
                            }
                        }

                        scope.Identifiers[funcName.Id] = new FuncInfo
                        {
                            Token = funcName,
                            Type = new TypeInfo(type, scope.AncestorsStructs),
                            argsInfo = paramsInfo.ToList()
                        };
                    }
                    // Парсинг структуры и проверка, не существует ли уже
                    else if (@operator == "Struct declaration")
                    {
                        Token token = ((ValueNode)operatorNode.Children.ToArray()[0]).Token;

                        try
                        {
                            var structFields = ((OperatorNode)operatorNode.Children.ToArray()[1]).Children.Select(d => ((OperatorNode)d).Children);

                            var fieldTypes = structFields
                                .Select(sf =>
                                {
                                    var typeNode = sf.ToArray()[0];
                                    var nameNode = sf.ToArray()[1];

                                    var arrayDeclarations = sf.Count() - 2;

                                    for (int i = 0; i < arrayDeclarations; i++)
                                    {
                                        typeNode = typeNode switch
                                        {
                                            ValueNode valueNode => new TypesNode([valueNode.Token, new Token(-2, TokenType.Punctuator, "*")]),
                                            TypesNode typesNode => new TypesNode([.. typesNode.Types, new Token(-2, TokenType.Punctuator, "*")]),

                                            _ => throw new Exception("???")
                                        };
                                    }

                                    string name = ((ValueNode)nameNode).Token.Value;
                                    TypeInfo typeInfo = new TypeInfo(typeNode, scope.AncestorsStructs);

                                    return KeyValuePair.Create(name, typeInfo);
                                })
                                .ToDictionary();

                            var structInfo = new StructInfo() { Pointed = fieldTypes, Name = token.Value };

                            if (scope.AncestorsStructs.Any(s => s.Name == structInfo.Name))
                                throw new SemanticException("Struct already defined.");

                            scope.Structs.Add(structInfo);
                        }
                        catch (Exception e)
                        {
                            throw new SemanticException("Unknown struct.", e);
                        }
                    }
                    // Инициализация переменной. Проверяем, что первая и пытаемся преобразовать тип
                    else if (@operator == "Variable initialization")
                    {
                        var initValue = operatorNode.Children.ToArray()[1];

                        var initValueType = GetRValueType(initValue, scope);

                        var declarationNode = (OperatorNode)operatorNode.Children.ToArray()[0];

                        Node type = declarationNode.Children.ToArray()[0];
                        Token token = ((ValueNode)declarationNode.Children.ToArray()[1]).Token;

                        var varType = new TypeInfo(type, scope.AncestorsStructs);

                        TypeInfo.CanImplicitCasted(initValueType, varType);

                        if (scope.Identifiers.ContainsKey(token.Id))
                            throw new SemanticException($"Variable {token.Value} already defined.");

                        scope.Identifiers[token.Id] = new IdentifierInfo
                        {
                            Token = token,
                            Type = varType,
                            Value = initValue
                        };
                    }
                    // Объявление переменной. Проверяем, чтоб константы были инициализированы
                    else if (@operator == "Variable declaration")
                    {
                        Node type = operatorNode.Children.ToArray()[0];
                        Token token = ((ValueNode)operatorNode.Children.ToArray()[1]).Token;

                        if (type is ValueNode { Token.Value: "const" or "auto" } typeValueNode)
                        {
                            throw new SemanticException($"Variable of {typeValueNode.Token.Value} not initialized.");
                        }
                        else if (type is TypesNode tokenNode)
                        {
                            var typeInfo = new TypeInfo(type, scope.AncestorsStructs);

                            if (typeInfo.TypesParts.Contains("const") || typeInfo.TypesParts.Contains("*const"))
                                throw new SemanticException($"Const variable not initialized.");
                        }

                        if (scope.Identifiers.ContainsKey(token.Id))
                            throw new SemanticException($"Variable {token.Value} already defined.");

                        scope.Identifiers[token.Id] = new IdentifierInfo
                        {
                            Token = token,
                            Type = new TypeInfo(type, scope.AncestorsStructs)
                        };
                    }
                    else if (@operator is "If else if statements" or "If else if else statements")
                    {
                        foreach (Node childNode in operatorNode.Children)
                        {
                            CheckVariableUsageSemantic(childNode, scope);
                        }
                    }
                    else if (@operator is "If statement" or "Else if statement")
                    {
                        var ifScope = new ScopeInfo() { ParentScope = scope, Root = operatorNode };

                        CheckVariableUsageSemantic(operatorNode.Children.ToArray()[0], ifScope);
                        CheckVariableUsageSemantic(operatorNode.Children.ToArray()[1], ifScope);
                    }
                    else if (@operator == "Else statement")
                    {
                        CheckVariableUsageSemantic(operatorNode.Children.ToArray()[0], new ScopeInfo() { ParentScope = scope, Root = operatorNode });
                    }
                    else if (@operator == "For loop")
                    {
                        Node iteratorInitializationNode = operatorNode.Children.ToArray()[0];
                        Node conditionNode = operatorNode.Children.ToArray()[1];
                        Node iteratorIncrementNode = operatorNode.Children.ToArray()[2];
                        Node blockNode = operatorNode.Children.ToArray()[3];

                        var forScope = new ScopeInfo() { ParentScope = scope, Root = operatorNode };

                        CheckVariableUsageSemantic(iteratorInitializationNode, forScope);
                        CheckVariableUsageSemantic(conditionNode, forScope);
                        CheckVariableUsageSemantic(iteratorIncrementNode, forScope);
                        CheckVariableUsageSemantic(blockNode, forScope);
                    }
                    else if (@operator is "While loop" or "Do while loop")
                    {
                        var whileScipe = new ScopeInfo() { ParentScope = scope, Root = operatorNode };

                        CheckVariableUsageSemantic(operatorNode.Children.ToArray()[0], whileScipe);
                        CheckVariableUsageSemantic(operatorNode.Children.ToArray()[1], whileScipe);
                    }
                    else if (@operator == "Return")
                    {
                        TypeInfo funcType = scope.IfFunc
                            ?? throw new SemanticException("Can't use return in not function object.");

                        if (funcType.TypesParts is ["void"])
                        {
                            if (operatorNode.Children.Any())
                                throw new SemanticException("Return type of this function is void. It can't return value.");
                        }
                        else
                        {
                            if (operatorNode.Children.Count() == 0)
                                throw new SemanticException("Return type of this function is not void. It should return value.");

                            TypeInfo returnType = GetRValueType(operatorNode.Children.ToArray()[0], scope);

                            if (!TypeInfo.IsImplicitCasted(returnType, funcType))
                                throw new SemanticException("Invalid return type of function.");
                        }
                    }
                    else if (@operator == "Label")
                    {
                        // Already checked.
                        return;
                    }
                    else
                    {
                        try
                        {
                            _ = GetRValueType(operatorNode, scope);
                        }
                        catch (SemanticException ex) when (ex.Message == "Invalid semantic")
                        {
                            foreach (var childNode in operatorNode.Children)
                            {
                                CheckVariableUsageSemantic(childNode, scope);
                            }
                        }
                    }
                }
                else if (root is ValueNode valueNode)
                {
                    if (valueNode.Token.TokenType == TokenType.Identifier)
                    {
                        int id = valueNode.Token.Id;

                        if (!scope.AncestorsIdentifiers.ContainsKey(id))
                            throw new SemanticException($"Indentifier {valueNode.Token.Value} used without declaration.");
                    }
                    else if (valueNode.Token.TokenType == TokenType.KeyWord)
                    {
                        if (!scope.IsLoop)
                            throw new SemanticException($"Invalid {valueNode.Token.Value} usage: it can be used only in loops.");
                    }
                }
                else
                {
                    return;
                }
            }

            static TypeInfo GetLValueType(Node root, ScopeInfo scope)
            {
                // Индексация. Проверяем, что можно индексироваться и что индекс - целое число
                if (root is OperatorNode { Operator: "Indexer [..]" } indexNode)
                {
                    var left = indexNode.Children.ToArray()[0];
                    var right = indexNode.Children.ToArray()[1];

                    TypeInfo leftTypeInfo = GetLValueType(left, scope);
                    TypeInfo rightTypeInfo = GetRValueType(right, scope);

                    if (leftTypeInfo.IfIndexed is null)
                        throw new SemanticException($"Not indexed type {string.Join(' ', leftTypeInfo.TypesParts)}.");

                    if (!rightTypeInfo.IsInteger)
                        throw new SemanticException("Not integer indexer argument.");

                    return leftTypeInfo.IfIndexed;
                }
                // Проверяем, можем ли мы пользоваться как указателем и что существует поле
                else if (root is OperatorNode { Operator: "Member access ." } pointOperator)
                {
                    var left = pointOperator.Children.ToArray()[0];
                    var right = pointOperator.Children.ToArray()[1];

                    var pointArg = ((ValueNode)right).Token.Value;

                    TypeInfo leftTypeInfo = GetRValueType(left, scope);

                    if (leftTypeInfo.IfPointed is null)
                        throw new SemanticException($"Not pointable type: {string.Join(' ', leftTypeInfo.TypesParts)}.");

                    if (leftTypeInfo.IfPointed.TryGetValue(pointArg, out var pointed))
                        return pointed;

                    throw new SemanticException($"Invalid point member accessing: {pointArg} is infalid field .");
                }
                // Проверяем, можем ли мы пользоваться как указателем и что существует поле
                else if (root is OperatorNode { Operator: "Member access ->" } arrowOperator)
                {
                    var left = arrowOperator.Children.ToArray()[0];
                    var right = arrowOperator.Children.ToArray()[1];

                    var arrowArg = ((ValueNode)right).Token.Value;

                    TypeInfo leftTypeInfo = GetRValueType(left, scope);

                    if (leftTypeInfo.IfArrowed is null)
                        throw new SemanticException($"Not arrowble type {string.Join(' ', leftTypeInfo.TypesParts)}.");

                    if (leftTypeInfo.IfArrowed.TryGetValue(arrowArg, out var arrowed))
                        return arrowed;

                    throw new SemanticException($"Invalid point member accessing: {arrowArg} is infalid field .");
                }
                // Проверяем, объявлена ли переменная
                else if (root is ValueNode valueNode && valueNode.Token.TokenType == TokenType.Identifier)
                {
                    if (scope.AncestorsIdentifiers.TryGetValue(valueNode.Token.Id, out IdentifierInfo? identifierInfo))
                    {
                        return new TypeInfo(identifierInfo.Type);
                    }
                    else
                    {
                        throw new SemanticException($"Undefined variable {valueNode.Token.Value}");
                    }
                }
                // Преобразуем в указатель
                else if (root is OperatorNode { Operator: "Address-of &" } addressOfOperator)
                {
                    var operand = addressOfOperator.Children.ToArray()[0];

                    var typeInfo = GetLValueType(operand, scope);

                    return new TypeInfo([.. typeInfo.TypesParts, "*"], scope.AncestorsStructs);
                }
                else
                {
                    throw new SemanticException("Invalid semantic");
                }
            }

            static TypeInfo GetRValueType(Node root, ScopeInfo scope)
            {
                // Преобразуем литерал
                if (root is ValueNode valueNode && valueNode.Token.TokenType is LiteralType literal)
                {

                    if (literal.ToString().ToLower().Contains("string"))
                        return new TypeInfo(["*", "char"], scope.AncestorsStructs);

                    List<string> list = ["*", "const", "char", "long", "int", "short", "double", "float", "auto"];

                    return new TypeInfo(literal.ToString().ToLower().Split().Where(l => list.Contains(l)).ToList(), scope.AncestorsStructs);
                }
                // Вызов функции. Проверяем, что функция объявлена и аргументы функции
                else if (root is OperatorNode { Operator: "Function calling" } funcOperator)
                {
                    var funcId = ((ValueNode)funcOperator.Children.ToArray()[0]).Token.Id;

                    if (!scope.AncestorsIdentifiers.TryGetValue(funcId, out IdentifierInfo? info))
                        throw new SemanticException("Func is not declared.");

                    if (info is not FuncInfo funcInfo)
                        throw new SemanticException("Function replaced by variable.");

                    IdentifierInfo[] declaredArgsInfo = funcInfo.argsInfo.ToArray();


                    IEnumerable<Node> funcArgs = funcOperator.Children.ToArray()[1..];

                    TypeInfo[] funcArgsTypes = funcArgs.Select(fa =>
                    {
                        return GetRValueType(fa, new ScopeInfo() { ParentScope = scope });
                    })
                    .ToArray();

                    if (declaredArgsInfo.Length < funcArgsTypes.Length)
                        throw new SemanticException("Given extra function args.");

                    for (int i = 0; i < declaredArgsInfo.Length; i++)
                    {
                        if (declaredArgsInfo[i].Value is not null
                            && (funcArgsTypes.Length >= i
                                || TypeInfo.IsImplicitCasted(funcArgsTypes[i], declaredArgsInfo[i].Type)))
                        {
                            throw new SemanticException($"Invalid function argument: {declaredArgsInfo[i].Token.Value}");
                        }
                    }

                    return funcInfo.Type;
                }
                // Оператор равно. Проверяем, что не изменяли константу
                else if (root is OperatorNode assignmentNode
                        && new[] { "=", "+=", "-=", "*=", "/=", "%=", ">>=", "<<=", "|=", "&=", "^=" }.Contains(assignmentNode.Operator))
                {
                    Node var = assignmentNode.Children.ToArray()[0];
                    Node expr = assignmentNode.Children.ToArray()[1];

                    var leftTypeInfo = GetLValueType(var, scope);

                    if (leftTypeInfo.IsReadOnly)
                        throw new SemanticException($"Lvalue is read only.");

                    var type1 = GetRValueType(var, scope);
                    var type2 = GetRValueType(expr, scope);

                    TypeInfo.CanImplicitCasted(type2, type1);

                    CheckVariableUsageSemantic(var, new ScopeInfo() { ParentScope = scope });
                    CheckVariableUsageSemantic(expr, new ScopeInfo() { ParentScope = scope });

                    return type1;
                }
                else if (root is UnaryOperatorNode { Operator: "Indirection *" } starNode)
                {
                    var operand = starNode.Children.ToArray()[0];

                    var typeInfo = GetRValueType(operand, scope);

                    if (typeInfo.IfIndexed is null)
                        throw new SemanticException($"Invalid type of star operator: {string.Join(' ', typeInfo.TypesParts)}.");

                    if (!typeInfo.TypesParts.Remove("*"))
                        typeInfo.TypesParts.Remove("*const");

                    return typeInfo;
                }
                else if (root is OperatorNode operatorNode && new string[] { "+", "-", "*", "/", "%" }.Contains(operatorNode.Operator))
                {
                    var left = operatorNode.Children.ToArray()[0];
                    var right = operatorNode.Children.ToArray()[1];

                    var leftInfo = GetRValueType(left, scope);
                    var rightInfo = GetRValueType(right, scope);

                    if (leftInfo.IfPointed is not null || rightInfo.IfPointed is not null)
                    {
                        throw new SemanticException($"Struct operand of {operatorNode.Operator}.");
                    }
                    if (leftInfo.IfIndexed is not null && rightInfo.IfIndexed is not null)
                    {
                        if (operatorNode.Operator == "+")
                        {
                            if (TypeInfo.IsImplicitCasted(leftInfo, rightInfo))
                            {
                                return rightInfo;
                            }
                            else if (TypeInfo.IsImplicitCasted(rightInfo, leftInfo))
                            {
                                return leftInfo;
                            }
                        }

                        throw new SemanticException($"Invalid type of {operatorNode.Operator} operand.");
                    }
                    else if (TypeInfo.IsImplicitCasted(leftInfo, rightInfo))
                    {
                        return rightInfo;
                    }
                    else if (TypeInfo.IsImplicitCasted(rightInfo, leftInfo))
                    {
                        return leftInfo;
                    }
                    else if (operatorNode.Operator is "+" or "-"
                            && (leftInfo.IfIndexed is not null && rightInfo.IsInteger
                                || rightInfo.IfIndexed is not null && leftInfo.IsInteger))
                    {
                        return leftInfo.IfIndexed is not null
                            ? leftInfo
                            : rightInfo;
                    }
                    else
                    {
                        throw new SemanticException($"Invalid type of {operatorNode.Operator} operand.");
                    }
                }
                else if (root is OperatorNode oNode && new string[] { "==", "!=", ">", "<", ">=", "<=" }
                    .Contains(oNode.Operator))
                {
                    var left = oNode.Children.ToArray()[0];
                    var right = oNode.Children.ToArray()[1];

                    var leftInfo = GetRValueType(left, scope);
                    var rightInfo = GetRValueType(right, scope);

                    if (leftInfo.IfPointed is not null || rightInfo.IfPointed is not null)
                    {
                        throw new SemanticException($"Struct operand of {oNode.Operator}.");
                    }
                    else if (!TypeInfo.IsImplicitCasted(leftInfo, rightInfo) && !TypeInfo.IsImplicitCasted(rightInfo, leftInfo))
                    {
                        throw new SemanticException($"Invalid operator args types.");
                    }
                    else
                    {
                        return new TypeInfo(["char"], scope.AncestorsStructs);
                    }
                }
                else if (root is OperatorNode binNode && new string[] { "<<", ">>", "|", "||", "&&", "&", "^" }
                    .Contains(binNode.Operator))
                {
                    var left = binNode.Children.ToArray()[0];
                    var right = binNode.Children.ToArray()[1];

                    var leftInfo = GetRValueType(left, scope);
                    var rightInfo = GetRValueType(right, scope);

                    if (leftInfo.IfPointed is not null || rightInfo.IfPointed is not null)
                    {
                        throw new SemanticException($"Struct operand of {binNode.Operator}.");
                    }
                    else if (!leftInfo.IsInteger || !rightInfo.IsInteger)
                    {
                        throw new SemanticException($"Not integer argument of binary operator.");
                    }
                    else if (TypeInfo.IsImplicitCasted(leftInfo, rightInfo))
                    {
                        return rightInfo;
                    }
                    else if (TypeInfo.IsImplicitCasted(rightInfo, leftInfo))
                    {
                        return leftInfo;
                    }
                    else
                    {
                        throw new SemanticException($"Invalid type of {binNode.Operator} operand.");
                    }
                }
                else if (root is OperatorNode unaryNode && new string[] { "Preincrement ++", "Postincrement ++", "Predecrement --", "Postdecrement --" }
                    .Contains(unaryNode.Operator))
                {
                    var operand = unaryNode.Children.ToArray()[0];

                    var typeInfo = GetRValueType(operand, scope);

                    if (typeInfo.TypesParts.Contains("struct"))
                        throw new SemanticException($"Struct with {unaryNode.Operator.ToLower()}.");

                    return typeInfo;
                }
                else if (root is OperatorNode { Operator: "(..)" } bracketsNode)
                {
                    var operand = bracketsNode.Children.ToArray()[0];

                    return GetRValueType(operand, scope);
                }
                else if (root is OperatorNode { Operator: "Type cast" } typeCastNode)
                {
                    var type = typeCastNode.Children.ToArray()[0];
                    var expr = typeCastNode.Children.ToArray()[1];

                    var typeInfo1 = new TypeInfo(type, scope.AncestorsStructs);
                    var typeInfo2 = GetRValueType(expr, scope);

                    if (typeInfo2.TypesParts.Contains("string"))
                        throw new SemanticException("Invalid cast to string.");

                    return typeInfo1;
                }
                else
                {
                    return GetLValueType(root, scope);
                }
            }
        }

        #region Helper classes
        // Хранит информацию об идентификаторе (токен, тип и знаачение)
        public class IdentifierInfo
        {
            public Token Token { get; set; }
            public TypeInfo Type { get; set; }
            public Node? Value { get; set; }
        }

        // Дополнительно хранит аргументы функции
        public class FuncInfo : IdentifierInfo
        {
            public List<IdentifierInfo> argsInfo { get; set; }
        }

        public class TypeInfo
        {
            // Список отдельных составляющих типа
            public List<string> TypesParts;

            // Является ли тип только для чтения (типаа константа)
            public bool IsReadOnly
            {
                get
                {
                    return TypesParts.Contains("const") || TypesParts.Contains("*const");
                }
            }

            // Является ли тип числом
            public bool IsInteger
            {
                get
                {
                    List<string> intTypes = ["int", "short", "char"];

                    for (int i = 0; i < intTypes.Count; i++)
                    {
                        if (TypesParts.Contains(intTypes[i]))
                            return true;
                    }

                    if (TypesParts.Contains("long") && !TypesParts.Contains("double"))
                        return true;

                    return false;
                }
            }



            private Dictionary<string, TypeInfo>? _ifPointed;
            public Dictionary<string, TypeInfo>? IfPointed
            {
                get
                {
                    if (IfIndexed is null)
                        return _ifPointed;

                    return null;
                }
            }

            public Dictionary<string, TypeInfo>? IfArrowed
            {
                get
                {
                    return IfIndexed?.IfPointed;
                }
            }

            // Можно ли индексироваться
            public TypeInfo? IfIndexed
            {
                get
                {
                    if (TypesParts.Contains("*") || TypesParts.Contains("*const"))
                    {
                        List<string> parts = TypesParts.ToList();

                        if (!parts.Remove("*"))
                            parts.Remove("*const");

                        return new TypeInfo(parts, _structs);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            private List<StructInfo> _structs;


            // Возможность неявного преобразования между двумя типами
            public static void CanImplicitCasted(TypeInfo type1, TypeInfo type2)
            {
                int starsCount1 = type1.TypesParts.Count(t => t is "*" or "*const");
                int starsCount2 = type2.TypesParts.Count(t => t is "*" or "*count");

                // Проверяем на равенство количество звездочек
                if (starsCount1 != 0 || starsCount2 != 0)
                {
                    if (starsCount1 != starsCount2)
                        throw new SemanticException("Invalid pointer casting.");
                }
                // Проверяем на структуру и чтоб имена структур совпадали
                else if (type1.TypesParts.Contains("struct"))
                {
                    string struct1Name = type1.TypesParts[type1.TypesParts.IndexOf("struct") + 1];

                    if (!type2.TypesParts.Contains("struct"))
                        throw new SemanticException("Invalid struct casting.");

                    string struct2Name = type2.TypesParts[type2.TypesParts.IndexOf("struct") + 1];

                    if (struct1Name != struct2Name)
                        throw new SemanticException("Invalid struct casting.");

                }
                // Проверка на типы с плавающей  запятой
                else if (type1.TypesParts.Contains("float"))
                {
                    if (!type2.TypesParts.Contains("float") && !type2.TypesParts.Contains("double"))
                        throw new SemanticException("Invalid float casting.");
                }
                // Проверка длинного целого числа и дабл
                else if (type1.TypesParts.Contains("double"))
                {
                    if (type1.TypesParts.Contains("long"))
                    {
                        if (!type2.TypesParts.Contains("double") || !type2.TypesParts.Contains("long"))
                            throw new SemanticException("Invalid double casting.");
                    }
                    else
                    {
                        if (!type2.TypesParts.Contains("double"))
                            throw new SemanticException("Invalid double casting.");
                    }
                }
                // В остальных случаях проверяем по весу значений
                else
                {
                    var weights = new List<TypeInfo>() { type1, type2 }.Select(typeInfo =>
                    {
                        int res = 0;

                        var typeParts = typeInfo.TypesParts;

                        Dictionary<string, int> weightsDict = new()
                        {
                            ["char"] = 1,
                            ["short"] = 2,
                            ["long"] = 8,
                            ["float"] = 2,
                            ["int"] = 4,
                            ["double"] = 4,
                        };

                        foreach (var (type, weight) in weightsDict)
                        {
                            if (typeParts.Contains(type))
                            {
                                res = weight;
                                break;
                            }
                        }

                        if (typeInfo.TypesParts.Contains("unsigned"))
                            res *= 2;

                        return res;
                    })
                    .ToArray();

                    int weight1 = weights[0];
                    int weight2 = weights[1];

                    if (weight1 > weight2)
                        throw new SemanticException("Invalid numeric cast.");
                }
            }

            public static bool IsImplicitCasted(TypeInfo type1, TypeInfo type2)
            {
                try
                {
                    CanImplicitCasted(type1, type2);
                    return true;
                }
                catch (SemanticException)
                {
                    return false;
                }
            }


            public TypeInfo(Node typeNode, List<StructInfo> structs)
            {
                this.TypesParts = [];
                _structs = structs;

                if (typeNode is ValueNode valueNode)
                {
                    AnaliseType([valueNode.Token.Value], structs);
                }
                else if (typeNode is TypesNode typesPartsNode)
                {
                    List<string> typeParts = typesPartsNode.Types.Select(token => token.Value).ToList();

                    for (int i = 0; i + 1 < typeParts.Count; i++)
                    {
                        if (typeParts[i] == "*" && typeParts[i + 1] == "const")
                        {
                            typeParts.RemoveAt(i);
                            typeParts[i] = "*const";
                        }
                    }

                    typeParts.Sort((t1, t2) =>
                    {
                        Func<string, int> getIndex = (str) => str switch
                        {
                            "*" => 3,
                            "*const" => 3,
                            "const" => 2,
                            "signed" => 1,
                            "unsigned" => 1,
                            _ => 0
                        };

                        return Comparer<int>.Default.Compare(getIndex(t1), getIndex(t2));
                    });

                    AnaliseType(typeParts, structs);
                }
                else
                {
                    throw new Exception("???");
                }
            }

            public TypeInfo(List<string> typesParts, List<StructInfo> structs)
            {
                this.TypesParts = [];
                _structs = structs;

                AnaliseType(typesParts, structs);
            }

            public TypeInfo(TypeInfo other)
            {
                this.TypesParts = other.TypesParts.ToList();
                _structs = other._structs;

                if (other.IfPointed is not null)
                    _ifPointed = new(other.IfPointed);
            }


            // Проверяем тип на различные ошибки
            private void AnaliseType(List<string> typeParts, List<StructInfo> structs)
            {
                if (typeParts.Contains("auto"))
                {
                    throw new NotImplementedException("Auto is not implemented");
                }
                else if (typeParts.Contains("*") || typeParts.Contains("*const"))
                {
                    var newTypesParts = new List<string>(typeParts);

                    newTypesParts.RemoveAll(s => s == "*");
                    newTypesParts.RemoveAll(s => s == "*const");

                    AnaliseType(newTypesParts, structs);
                }
                else if (typeParts.Contains("struct"))
                {
                    if (typeParts.Count(t => t == "struct") > 1)
                        throw new SemanticException("Duplicate struct keyword.");

                    int index = typeParts.IndexOf("struct");

                    if (index + 1 >= typeParts.Count)
                        throw new SemanticException("Invalis semantic.");

                    string structName = typeParts[index + 1];

                    foreach (string invalidType in new string[] { "char", "double", "float", "int", "long", "short", "signed", "unsigned", "void" })
                        if (typeParts.Contains(invalidType))
                            throw new SemanticException("Invalid struct type modifier.");

                    ///
                    if (!structs.Select(s => s.Name).Contains(structName))
                        throw new SemanticException("Unknown struct.");

                    _ifPointed = structs.First(s => s.Name == structName).Pointed;
                }
                else if (typeParts.Contains("void"))
                {
                    if (typeParts.Count(t => t == "void") > 1)
                        throw new SemanticException("Duplicated void");

                    foreach (string invalidType in new string[] { "char", "double", "float", "int", "long", "short", "signed", "unsigned", "const" })
                        if (typeParts.Contains(invalidType))
                            throw new SemanticException("Invalid void type modifier.");
                }
                else if (typeParts.Contains("float"))
                {
                    if (typeParts.Count(t => t == "float") > 1)
                        throw new SemanticException("Duplicated float");

                    foreach (string invalidType in new string[] { "char", "double", "int", "long", "short", "signed", "unsigned" })
                        if (typeParts.Contains(invalidType))
                            throw new SemanticException("Invalid float type modifier.");
                }
                else if (typeParts.Contains("double"))
                {
                    if (typeParts.Count(t => t == "double") > 1)
                        throw new SemanticException("Duplicated float");

                    foreach (string invalidType in new string[] { "char", "int", "short", "signed", "unsigned" })
                        if (typeParts.Contains(invalidType))
                            throw new SemanticException("Invalid double type modifier.");

                    if (typeParts.Count(t => t == "long") > 1)
                        throw new SemanticException("Very long double.");
                }
                else if (typeParts.Contains("char"))
                {
                    if (typeParts.Count(t => t == "char") > 1)
                        throw new SemanticException("Duplicated char");

                    foreach (string invalidType in new string[] { "int", "short", "long" })
                        if (typeParts.Contains(invalidType))
                            throw new SemanticException("Invalid char type modifier.");

                    if (typeParts.Contains("signed"))
                    {
                        if (typeParts.Count(t => t == "signed") > 1)
                            throw new SemanticException("Duplicated signed.");

                        if (typeParts.Count(t => t == "unsigned") > 0)
                            throw new SemanticException("Signed or unsigned?");
                    }

                    if (typeParts.Count(t => t == "unsigned") > 1)
                        throw new SemanticException("Duplicated unsigned.");

                    if (typeParts.Count(t => t == "unsigned") == 1)
                        typeParts.Remove("unsigned");
                }
                else if (typeParts.Contains("short"))
                {
                    if (typeParts.Count(t => t == "short") > 1)
                        throw new SemanticException("Duplicated short.");

                    if (typeParts.Contains("signed"))
                    {
                        if (typeParts.Count(t => t == "signed") > 1)
                            throw new SemanticException("Duplicated signed.");

                        if (typeParts.Count(t => t == "unsigned") > 0)
                            throw new SemanticException("Signed or unsigned?");

                        typeParts.Remove("signed");
                    }

                    if (typeParts.Count(t => t == "unsigned") > 1)
                        throw new SemanticException("Duplicated unsigned.");

                    if (typeParts.Count(t => t == "int") > 1)
                        throw new SemanticException("Duplicated int.");
                }
                else if (typeParts.Contains("long"))
                {
                    if (typeParts.Count(t => t == "long") > 2)
                        throw new SemanticException("So long.");

                    if (typeParts.Contains("signed"))
                    {
                        if (typeParts.Count(t => t == "signed") > 1)
                            throw new SemanticException("Duplicated signed.");

                        if (typeParts.Count(t => t == "unsigned") > 0)
                            throw new SemanticException("Signed or unsigned?");

                        typeParts.Remove("signed");
                    }

                    if (typeParts.Count(t => t == "unsigned") > 1)
                        throw new SemanticException("Duplicated unsigned.");

                    if (typeParts.Count(t => t == "int") == 0)
                    {
                        typeParts[typeParts.IndexOf("long")] = "int";
                    }
                    else if (typeParts.Count(t => t == "int") == 1)
                    {
                        if (typeParts.Count(t => t == "long") == 2)
                            typeParts.Remove("long");
                    }
                    else
                    {
                        throw new SemanticException("So many ints.");
                    }
                }
                else if (typeParts.Contains("int"))
                {
                    if (typeParts.Count(t => t == "int") > 1)
                        throw new SemanticException("So many ints.");

                    if (typeParts.Contains("signed"))
                    {
                        if (typeParts.Count(t => t == "signed") > 1)
                            throw new SemanticException("Duplicated signed.");

                        if (typeParts.Count(t => t == "unsigned") > 0)
                            throw new SemanticException("Signed or unsigned?");

                        typeParts.Remove("signed");
                    }

                    if (typeParts.Count(t => t == "unsigned") > 1)
                        throw new SemanticException("Duplicated unsigned.");
                }
                else if (typeParts.Contains("const"))
                {
                    int constCount = typeParts.Count(t => t == "const");

                    for (int i = 0; i < constCount - 1; i++)
                    {
                        typeParts.Remove("const");
                    }

                    bool constOnly = true;

                    foreach (var t in new List<string> { "char", "double", "int", "long", "short", "signed", "unsigned" })
                    {
                        if (typeParts.Contains(t))
                        {
                            constOnly = false;
                            break;
                        }
                    }

                    if (constOnly)
                    {
                        typeParts.Add("int");
                    }
                    else
                    {
                        throw new Exception("???");
                    }
                }
                else
                {
                    throw new SemanticException("Invalid type.");
                }

                TypesParts = typeParts;
            }
        }

        public class StructInfo
        {
            public string Name { get; set; }
            public Dictionary<string, TypeInfo>? Pointed { get; set; }
        }

        // Хранит информацию об области видимости
        public class ScopeInfo
        {
            // Корень данной области
            public Node? Root { get; set; }

            // Родительская область
            public ScopeInfo? ParentScope { get; set; } = null;


            public List<StructInfo> Structs { get; set; } = [];

            // Словарь переменных с информацией о них
            public Dictionary<int, IdentifierInfo> Identifiers { get; set; } = [];


            // Возвращает список всех объявленных структур
            public List<StructInfo> AncestorsStructs
            {
                get
                {
                    List<StructInfo> ans = [];

                    var scope = this;

                    do
                    {
                        ans.AddRange(scope.Structs);

                        scope = scope.ParentScope;
                    }
                    while (scope is not null);

                    return ans;
                }
            }

            // Словарь всех переменных этой области и родительских
            public Dictionary<int, IdentifierInfo> AncestorsIdentifiers
            {
                get
                {
                    Dictionary<int, IdentifierInfo> res = [];

                    var scope = this;

                    do
                    {
                        scope.Identifiers.ToList().ForEach(x =>
                        {
                            try
                            {
                                res.Add(x.Key, x.Value);
                            }
                            catch (ArgumentException)
                            { }
                        });

                        scope = scope.ParentScope;
                    }
                    while (scope is not null);

                    return res;
                }
            }

            // Проверяет, находится ли область внутри цикла
            public bool IsLoop
            {
                get
                {
                    var scope = this;

                    do
                    {
                        if (scope.Root is OperatorNode { Operator: "For loop" or "While loop" or "Do while loop" })
                            return true;

                        scope = scope.ParentScope;
                    }
                    while (scope is not null);

                    return false;
                }
            }

            // Ищет объявление функции
            public TypeInfo? IfFunc
            {
                get
                {
                    var scope = this;

                    do
                    {
                        if (scope.Root is OperatorNode { Operator: "Function declaration" } funcNode)
                        {
                            Token funcToken = ((ValueNode)funcNode.Children.ToArray()[1]).Token;

                            if (!AncestorsIdentifiers.TryGetValue(funcToken.Id, out IdentifierInfo? info))
                                throw new SemanticException("Function is not declared.");

                            if (info is not FuncInfo funcInfo)
                                throw new SemanticException("Function is replaced by variable.");

                            return funcInfo.Type;
                        }

                        scope = scope.ParentScope;
                    }
                    while (scope is not null);

                    return null;
                }
            }
        }

        #endregion
    }
}
