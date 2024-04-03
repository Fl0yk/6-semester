using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Compiler
{
    public static partial class Compiler
    {
        public static Node SyntaxAnalysis(List<Token> tokens)
        {
            int i = 0;

            var res = Grammar.ProgramGrammar.Parse(tokens, ref i);

            if (i < tokens.Count || res is null)
                throw new SyntaxException("Invalid syntax");

            return res;
        }

        #region Exception
        public class SyntaxException : Exception
        {
            public SyntaxException()
                : base()
            { }

            public SyntaxException(string message)
                : base(message)
            { }

            public SyntaxException(string message, Exception innerException)
                : base(message, innerException)
            { }
        }
        #endregion

        #region Grammars

        private abstract class Grammar
        {
            // Разраюирает последовательность токенов, начиная с индекса i
            // Возвращает результирующий узел синт. дерева или null
            public abstract Node? Parse(List<Token> tokens, ref int i);

            // Метод для объеденения несколких правил аналогично логическому или
            public Grammar Or(Grammar right)
            {
                if (this is null)
                    throw new ArgumentNullException("left");

                if (right is null)
                    throw new ArgumentNullException("right");

                return new DelegateGrammar(ParseOr);

                Node? ParseOr(List<Token> tokens, ref int i)
                {
                    int initial = i;
                    if (this.Parse(tokens, ref i) is Node leftNode)
                        return leftNode;

                    i = initial;
                    if (right.Parse(tokens, ref i) is Node rightNode)
                        return rightNode;

                    i = initial;
                    return null;
                }
            }

            // Позволяет последовательно применить два правилаа грамматики
            // Count - сколько раз нужно применить второре правило
            public GrammarRow Then(Grammar other, int count = 1)
            {
                ArgumentOutOfRangeException
                    .ThrowIfLessThanOrEqual(count, 0, nameof(count));

                return new GrammarRow() { (this, count: 1, null), (other, count, null) };
            }

            // Класс для представления последовательности правил
            public class GrammarRow
            : List<(Grammar, int count, string? errorMessage)>
            {
                public GrammarRow Then(Grammar grammar, int count = 1)
                {
                    ArgumentOutOfRangeException
                        .ThrowIfLessThanOrEqual(count, 0, nameof(count));

                    Add((grammar, count, null));
                    return this;
                }

                public GrammarRow WithError(string errorMessage)
                {
                    this[^1] = this[^1] with { errorMessage = errorMessage };

                    return this;
                }

                public Grammar AsNode(Func<List<Node>, Node> merge)
                {
                    return new DelegateGrammar(ParseAll);


                    Node? ParseAll(List<Token> tokens, ref int i)
                    {
                        int next = i;

                        List<Node> nodes = [];

                        foreach (var (grammar, count, errorMessage) in this)
                        {
                            for (int j = 0; j < count; j++)
                            {
                                if (grammar.Parse(tokens, ref next) is not Node node)
                                {
                                    if (errorMessage is not null)
                                    {
                                        throw new SyntaxException(errorMessage);
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    nodes.Add(node);
                                }
                            }
                        }

                        i = next;
                        return merge(nodes);
                    }
                }
            }

            // Метод для применения двух правил с возможностью вложенного повторения 2-го правила
            public Grammar ThenNested(Grammar other, Func<Node, Node, Node> merge, bool canEmpty = false)
            {
                return new DelegateGrammar(ParseNested);


                Node? ParseNested(List<Token> tokens, ref int i)
                {
                    int initial = i;
                    LinkedList<Node> nodes = [];

                    if (this.Parse(tokens, ref i) is not Node startNode)
                    {
                        i = initial;
                        return null;
                    }
                    else
                    {
                        nodes.AddLast(startNode);
                    }

                    int next = i;


                    while (true)
                    {
                        if (other.Parse(tokens, ref next) is Node nextNode)
                        {
                            nodes.AddLast(nextNode);
                            i = next;
                        }
                        else
                        {
                            break;
                        }
                    }

                    int minNodesCount = canEmpty ? 1 : 2;

                    if (nodes.Count < minNodesCount)
                    {
                        i = initial;
                        return null;
                    }

                    if (nodes.Count == 1 && canEmpty)
                        return nodes.First!.Value;

                    return nodes.Aggregate(merge);
                }
            }


            #region Language grammar

            public static LazyGrammar ProgramGrammar => new(() =>
            {
                return
                    new ListGrammar(StructDeclarationGrammar
                        .Or(FunctionDeclarationGrammar)
                        .Or(FunctionPrototypeGrammar)
                        .Or(VariableCreatingGrammar
                            .Then(new TokenGrammar(TokenType.Punctuator, ";")).WithError("; missed after variable creating.")
                            .AsNode(nodes => nodes[0])),
                        "Program",
                        separator: null,
                        canEmpty: true);
            });

            public static LazyGrammar StructDeclarationGrammar => new(() =>
            {
                return
                    new TokenGrammar(TokenType.Type, "struct")
                    .Then(IdentifierGrammar)
                    .Then(new BlockGrammar("{", "}", new ListGrammar(VariableDeclarationGrammar
                        .Then(new TokenGrammar(TokenType.Punctuator, ";")).WithError("Invalid struct field declaration: ';' is missed.")
                        .AsNode(nodes => nodes[0]), "Struct fields", separator: null, canEmpty: true), "Struct fields"))
                    .Then(new TokenGrammar(TokenType.Punctuator, ";")).WithError("Invalid structure declaration: ';' is missed.")
                    .AsNode(nodes => new BinaryOperatorNode(nodes[1], ((OperatorNode)nodes[2]).Children.ToArray()[0], "Struct declaration"));
            });

            public static LazyGrammar FunctionDeclarationGrammar => new(() =>
            {
                return
                    TypeGrammar
                    .Then(IdentifierGrammar)
                    .Then(new TokenGrammar(TokenType.Punctuator, "("))
                    .Then(new ListGrammar(VariableDeclarationGrammar, "Function parameters", canEmpty: true))
                    .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid function declaration: ')' is missed.")
                    .Then(ScopeGrammar)
                    .AsNode(nodes => new OperatorNode([nodes[0], nodes[1], nodes[3], nodes[5]], "Function declaration"));
            });

            public static LazyGrammar FunctionPrototypeGrammar => new(() =>
            {
                return
                   TypeGrammar
                   .Then(IdentifierGrammar)
                   .Then(new TokenGrammar(TokenType.Punctuator, "("))
                   .Then(new ListGrammar(VariableDeclarationGrammar, "Function parameters", canEmpty: true))
                   .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid function declaration: ')' is missed.")
                   .Then(new TokenGrammar(TokenType.Punctuator, ";"))
                   .AsNode(nodes => new OperatorNode([nodes[0], nodes[1], nodes[3]], "Function prototype"));
            });

            public static LazyGrammar IfElseIfElseGrammar => new(() =>
            {
                return
                    new TokenGrammar(TokenType.KeyWord, "if")
                        .Then(new TokenGrammar(TokenType.Punctuator, "(")).WithError("Invalid if declaration: '(' is missed.")
                        .Then(ExpressionGrammar).WithError("Invalid if declaration: predicate is missed.")
                        .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid if declaration: ')' is missed.")
                        .Then(ScopeGrammar.Or(LineGrammar)).WithError("Invalid if declaration: body is missed.")
                        .AsNode(nodes => new BinaryOperatorNode(nodes[2], nodes[4], "If statement"))
                    .Then(new ListGrammar(
                        new TokenGrammar(TokenType.KeyWord, "else")
                        .Then(new TokenGrammar(TokenType.KeyWord, "if"))
                        .Then(new TokenGrammar(TokenType.Punctuator, "(")).WithError("Invalid else if declaration: '(' is missed.")
                        .Then(ExpressionGrammar).WithError("Invalid else if declaration: predicate is missed.")
                        .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid if declaration: ')' is missed.")
                        .Then(ScopeGrammar.Or(LineGrammar)).WithError("Invalid else if declaration: body is missed.")
                        .AsNode(nodes => new BinaryOperatorNode(nodes[3], nodes[5], "Else if statement")),
                    "Else if operators",
                    separator: null,
                    canEmpty: true))
                    .AsNode(nodes =>
                    {
                        var ifNode = nodes[0];
                        var elseIfNodes = ((OperatorNode)nodes[1]).Children;

                        List<Node> ifElseIfNodes = [ifNode, .. elseIfNodes];

                        return new OperatorNode(ifElseIfNodes, "If else if statements");
                    })
                    .Then(new TokenGrammar(TokenType.KeyWord, "else")
                        .Then(ScopeGrammar.Or(LineGrammar)).WithError("Invalid else declaration: body is missed.")
                        .AsNode(nodes => new UnaryOperatorNode(nodes[1], "Else statement"))
                       .Or(EmptyGrammar))
                    .AsNode(nodes =>
                    {
                        var ifElseIfNode = nodes[0];
                        var elseNode = nodes[1];

                        if (elseNode is EmptyNode)
                            return ifElseIfNode;

                        var ifElseIfNodes = ((OperatorNode)ifElseIfNode).Children;

                        List<Node> ifElseIfElseNodes = [.. ifElseIfNodes, elseNode];

                        return new OperatorNode(ifElseIfElseNodes, "If else if else statements");
                    });
            });

            public static LazyGrammar SwitchCaseGrammar => new(() =>
            {
                return
                    new TokenGrammar(TokenType.KeyWord, "switch")
                    .Then(new TokenGrammar(TokenType.Punctuator, "("))
                    .Then(ExpressionGrammar)
                    .Then(new TokenGrammar(TokenType.Punctuator, ")"))
                    .Then(new TokenGrammar(TokenType.Punctuator, "{"))
                    .Then(new ListGrammar(CaseGrammar, "Cases", separator: null, canEmpty: true))
                    .Then(new TokenGrammar(TokenType.Punctuator, "}"))
                    .AsNode(nodes =>
                    {
                        var defaultCount = ((OperatorNode)nodes[5]).Children
                            .Count(@case =>
                            {
                                var caseValue = ((OperatorNode)@case).Children.First();

                                if (caseValue is ValueNode { Token.Value: "default" })
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            });

                        if (defaultCount > 1)
                            throw new SyntaxException("Switch case has more than one default.");

                        return new BinaryOperatorNode(nodes[2], nodes[5], "Switch case");
                    });
            });

            public static LazyGrammar CaseGrammar => new(() =>
            {
                return
                    (new TokenGrammar(TokenType.KeyWord, "case")
                        .Then(ExpressionGrammar)
                        .AsNode(nodes => nodes[1])
                       .Or(new TokenGrammar(TokenType.KeyWord, "default")))
                    .Then(new TokenGrammar(TokenType.Punctuator, ":"))
                    .Then(ScopeGrammar.Or(LinesGrammar))
                        .AsNode(nodes => new BinaryOperatorNode(nodes[0], nodes[2], "Case"));
            });

            public static LazyGrammar ForGrammar => new(() =>
            {
                return
                    new TokenGrammar(TokenType.KeyWord, "for")
                    .Then(new TokenGrammar(TokenType.Punctuator, "(")).WithError("Invalid for declaration: '(' is missed.")
                    .Then(InstructionGrammar.Or(EmptyGrammar))
                    .Then(new TokenGrammar(TokenType.Punctuator, ";")).WithError("Invalid for declaration: ';' is missed.")
                    .Then(InstructionGrammar.Or(EmptyGrammar))
                    .Then(new TokenGrammar(TokenType.Punctuator, ";")).WithError("Invalid for declaration: ';' is missed.")
                    .Then(InstructionGrammar.Or(EmptyGrammar))
                    .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid for declaration: ')' is missed.")
                    .Then(ScopeGrammar.Or(LineGrammar)).WithError("Invalid for declaration: body is missed.")
                    .AsNode(nodes => new OperatorNode([nodes[2], nodes[4], nodes[6], nodes[8]], "For loop"));
            });


            public static LazyGrammar DoWhileGrammar => new(() =>
            {
                return
                    new TokenGrammar(TokenType.KeyWord, "do")
                    .Then(ScopeGrammar.Or(LineGrammar))
                    .Then(new TokenGrammar(TokenType.KeyWord, "while")).WithError("Invalid do while declaration: 'while' is missed.")
                    .Then(new TokenGrammar(TokenType.Punctuator, "(")).WithError("Invalid do while declaration: '(' is missed.")
                    .Then(ExpressionGrammar)
                    .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid do while declaration: ')' is missed.")
                    .Then(new TokenGrammar(TokenType.Punctuator, ";")).WithError("Invalid do while declaration: ';' is missed.")
                    .AsNode(nodes => new BinaryOperatorNode(nodes[4], nodes[1], "Do while loop"));
            });

            public static LazyGrammar WhileGrammar => new(() =>
            {
                return
                    new TokenGrammar(TokenType.KeyWord, "while")
                    .Then(new TokenGrammar(TokenType.Punctuator, "(")).WithError("Invalid while declaration: '(' is missed.")
                    .Then(ExpressionGrammar)
                    .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid while declaration: ')' is missed.")
                    .Then(ScopeGrammar.Or(LineGrammar))
                    .AsNode(nodes => new BinaryOperatorNode(nodes[2], nodes[4], "While loop"));
            });

            public static LazyGrammar ScopeGrammar => new(() =>
            {
                return
                    new TokenGrammar(TokenType.Punctuator, "{")
                    .Then(LinesGrammar)
                    .Then(new TokenGrammar(TokenType.Punctuator, "}"))
                    .AsNode(nodes => nodes[1]);
            });

            public static LazyGrammar LinesGrammar => new(() =>
            {
                return
                    new ListGrammar(
                        LineGrammar
                        .Or(LabelGrammar)
                        .Or(WhileGrammar)
                        .Or(DoWhileGrammar)
                        .Or(ForGrammar)
                        .Or(IfElseIfElseGrammar)
                        .Or(SwitchCaseGrammar),
                    "Block of code", separator: null, canEmpty: true);
            });

            public static LazyGrammar LineGrammar => new(() =>
            {
                return
                    new ListGrammar(InstructionGrammar, "Instruction list", canEmpty: true)
                    .Then(new TokenGrammar(TokenType.Punctuator, ";"))
                    .AsNode(nodes =>
                    {
                        if (nodes[0] is OperatorNode instructionsNode)
                        {
                            var instructions = instructionsNode.Children.ToArray();

                            string lineNodeValue = instructions.Length switch
                            {
                                0 => "Empty line ;",
                                1 => "Line ;",
                                _ => "Lines ;"
                            };

                            return new OperatorNode(instructions, lineNodeValue);
                        }
                        else
                        {
                            throw new Exception("???");
                        }
                    });
            });

            public static LazyGrammar InstructionGrammar => new(() =>
            {
                return
                    VariableCreatingGrammar
                    .Or(VariableDeclarationGrammar)
                    .Or(ExpressionGrammar)
                    .Or(KeyWordGrammar);
            });

            public static LazyGrammar LabelGrammar => new(() =>
            {
                return
                    IdentifierGrammar
                    .Then(new TokenGrammar(TokenType.Punctuator, ":"))
                    .AsNode(nodes => new UnaryOperatorNode(nodes[0], "Label"));
            });

            public static LazyGrammar KeyWordGrammar => new(() =>
            {
                return
                    new TokenGrammar(TokenType.KeyWord, "break")
                    .Or(new TokenGrammar(TokenType.KeyWord, "continue"))
                    .Or(new TokenGrammar(TokenType.KeyWord, "return")
                      .Then(ExpressionGrammar.Or(EmptyGrammar))
                      .AsNode(nodes => new UnaryOperatorNode(nodes[1], "Return")))
                    .Or(new TokenGrammar(TokenType.KeyWord, "goto")
                      .Then(IdentifierGrammar).WithError("Goto invalid declaration: label is missed.")
                      .AsNode(nodes => new UnaryOperatorNode(nodes[1], "Go to")));
            });

            public static LazyGrammar VariableCreatingGrammar => new(() =>
            {
                return
                    VariableDeclarationGrammar
                        .Then(new TokenGrammar(TokenType.Punctuator, "=")
                            .Then(ExpressionGrammar)
                            .AsNode(nodes => nodes[1])
                            .Or(EmptyGrammar))
                        .AsNode(nodes =>
                        {
                            if (nodes[1] is EmptyNode)
                                return nodes[0];

                            return new BinaryOperatorNode(nodes[0], nodes[1], "Variable initialization");
                        })
                            .ThenNested(new TokenGrammar(TokenType.Punctuator, ",")
                                .ThenNested(new TokenGrammar(TokenType.Punctuator, "*").Or(new TokenGrammar(TokenType.Type, "const")),
                                merge: (ptrNode1, ptrNode2) =>
                                {
                                    return (ptrNode1, ptrNode2) switch
                                    {
                                        (ValueNode { Token.Value: "," }, ValueNode valueNode) => new TypesNode(valueNode.Token),
                                        (ValueNode valueNode1, ValueNode valueNode2) => new TypesNode(valueNode1.Token, valueNode1.Token),
                                        (TypesNode typesNode, ValueNode valueNode) => new TypesNode() { Types = [.. typesNode.Types, valueNode.Token] },

                                        _ => throw new Exception("???")
                                    };
                                },
                                canEmpty: true)
                                .Then(IdentifierGrammar)
                                .Then(new TokenGrammar(TokenType.Punctuator, "=")
                                    .Then(ExpressionGrammar)
                                    .AsNode(nodes => nodes[1])
                                .Or(Grammar.EmptyGrammar))
                                .AsNode(nodes =>
                                {
                                    Node? typesAddition;

                                    if (nodes[0] is ValueNode { Token.Value: "," })
                                    {
                                        typesAddition = new TypesNode();
                                    }
                                    else
                                    {
                                        typesAddition = nodes[0];
                                    }

                                    var identifier = nodes[1];

                                    return nodes[2] is not EmptyNode
                                        ? new ParentNode([typesAddition, identifier, nodes[2]])
                                        : new ParentNode([typesAddition, identifier]);
                                }),
                                merge: (prevVarsNode, currentVarNode) =>
                                {
                                    var nodes = (prevVarsNode switch
                                    {
                                        BinaryOperatorNode firstVarNode => [firstVarNode],
                                        OperatorNode prevVarsOperatorNode => prevVarsOperatorNode.Children,

                                        _ => throw new Exception("???")
                                    })
                                    .ToArray();

                                    if (currentVarNode is ParentNode parentNode)
                                    {
                                        var children = parentNode.Children.ToArray();

                                        var firstVarTypeOrDeclarationNode = ((OperatorNode)nodes[0]).Children.First();

                                        List<Token> type = firstVarTypeOrDeclarationNode switch
                                        {
                                            ValueNode typeValueNode => [typeValueNode.Token],
                                            TypesNode typeTypesNode => typeTypesNode.Types,

                                            BinaryOperatorNode declarationNode => declarationNode.Children.ToArray()[0] switch
                                            {
                                                ValueNode typeValueNode => [typeValueNode.Token],
                                                TypesNode typeTypesNode => typeTypesNode.Types,

                                                _ => throw new Exception("???")
                                            },

                                            _ => throw new Exception("???")
                                        };

                                        var typesAddition = ((TypesNode)children[0]).Types;

                                        var newType = new TypesNode([.. type, .. typesAddition]);

                                        var varDeclaration = new BinaryOperatorNode(newType, children[1], "Variable declaration");

                                        if (children.Length == 3)
                                        {
                                            varDeclaration = new BinaryOperatorNode(varDeclaration, children[2], "Variable initialization");
                                        }

                                        currentVarNode = varDeclaration;
                                    }
                                    else
                                    {
                                        throw new Exception("???");
                                    }

                                    nodes = [.. nodes, currentVarNode];

                                    return new OperatorNode(nodes, "Variables declaration");
                                },
                                canEmpty: true)
                    .Or(VariableDeclarationGrammar);
            });

            public static LazyGrammar VariableDeclarationGrammar => new(() =>
            {
                return
                    TypeGrammar
                        .Then(IdentifierGrammar)
                        .AsNode(nodes => new BinaryOperatorNode(nodes[0], nodes[1], "Variable declaration"))
                        .ThenNested(new TokenGrammar(TokenType.Punctuator, "[")
                            .Then(ExpressionGrammar.Or(EmptyGrammar))
                            .Then(new TokenGrammar(TokenType.Punctuator, "]"))
                            .AsNode(nodes => nodes[1]),
                            merge: (varDecNode, bracketsNode) =>
                            {
                                var nodes = ((OperatorNode)varDecNode).Children.ToList();
                                nodes.Add(new UnaryOperatorNode(bracketsNode, $"Array declaration [{(bracketsNode is EmptyNode ? "" : "..")}]"));
                                return new OperatorNode(nodes, "Variable declaration");
                            },
                            canEmpty: true);
            });

            public static LazyGrammar ExpressionGrammar => new(() =>
            {
                return
                    new BinaryOperatorGrammar(LValueGrammar, GGrammar, TokenGrammar.Any(TokenType.Punctuator, "=", "+=", "-=", "*=", "/=", "%=", "&=", "^=", "|="))
                    .Or(GGrammar);
            });

            public static LazyGrammar GGrammar => new(() =>
            {
                return
                    FGrammar
                        .Then(new TokenGrammar(TokenType.Punctuator, "?"))
                        .Then(GGrammar)
                        .Then(new TokenGrammar(TokenType.Punctuator, ":")).WithError("Invalid ternary operator: ':' is missed")
                        .Then(GGrammar)
                        .AsNode(nodes => new TernaryOperatorNode(nodes[0], nodes[2], nodes[4], "Ternary operator ?:"))
                    .Or(FGrammar);
            });

            public static LazyGrammar FGrammar => new(() =>
            {
                return
                    new BinaryOperatorGrammar(EGrammar, EGrammar, TokenGrammar.Any(TokenType.Punctuator, "==", "!=", ">", "<", ">=", "<=", "&", "^", "|", "&&", "||"))
                    .Or(EGrammar);
            });

            public static LazyGrammar EGrammar => new(() =>
            {
                return
                    new BinaryOperatorGrammar(DGrammar, DGrammar, TokenGrammar.Any(TokenType.Punctuator, "<<", ">>"))
                    .Or(DGrammar);
            });

            public static LazyGrammar DGrammar => new(() =>
            {
                return
                    new BinaryOperatorGrammar(CGrammar, CGrammar, TokenGrammar.Any(TokenType.Punctuator, "+", "-"))
                    .Or(CGrammar);
            });

            public static LazyGrammar CGrammar => new(() =>
            {
                return
                    new BinaryOperatorGrammar(BGrammar, BGrammar, TokenGrammar.Any(TokenType.Punctuator, "*", "/", "%"))
                    .Or(BGrammar);
            });

            public static LazyGrammar BGrammar => new(() =>
            {
                return
                    new PreUnaryOperatorGrammar(LValueGrammar, "++", "Preincrement ++")
                    .Or(new PreUnaryOperatorGrammar(LValueGrammar, "--", "Predecrement --"))
                    .Or(new PreUnaryOperatorGrammar(BGrammar, "-", "Negation -"))
                    .Or(new PreUnaryOperatorGrammar(BGrammar, "+", "Plus +"))
                    .Or(new PreUnaryOperatorGrammar(BGrammar, "!", "Logical not !"))
                    .Or(new PreUnaryOperatorGrammar(BGrammar, "~", "Bitwise complement ~"))
                    .Or(new PreUnaryOperatorGrammar(BGrammar, "*", "Indirection *"))
                    .Or(new PreUnaryOperatorGrammar(BGrammar, "&", "Address-of &"))
                    .Or(new TokenGrammar(TokenType.KeyWord, "sizeof")
                        .Then(new TokenGrammar(TokenType.Punctuator, "(")).WithError("Invalid sizeof operator: '(' is missed.")
                        .Then(TypeGrammar).WithError("Invalid sizeof operator: type is missed.")
                        .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid sizeof operator: ')' is missed.")
                        .AsNode(nodes => new OperatorNode([nodes[2]], "Sizeof")))
                    .Or(new TokenGrammar(TokenType.Punctuator, "(")
                        .Then(TypeGrammar)
                        .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid type casr operator: ')' is missed.")
                        .Then(RValueGrammar)
                        .AsNode(nodes => new OperatorNode([nodes[1], nodes[3]], "Type cast")))
                    .Or(AGrammar);
            });


            public static LazyGrammar AGrammar => new(() =>
            {
                return
                    new PostUnaryOperatorGrammar(LValueGrammar, "++", "Postincrement ++")
                    .Or(new PostUnaryOperatorGrammar(LValueGrammar, "--", "Postdecrement --"))
                    .Or(RValueGrammar
                        .Then(new TokenGrammar(TokenType.Punctuator, "("))
                        .Then(new ListGrammar(ExpressionGrammar, "Function call args", canEmpty: true))
                        .Then(new TokenGrammar(TokenType.Punctuator, ")")).WithError("Invalid function calling: ')' is missed.")
                        .AsNode(nodes => new OperatorNode((new[] { nodes[0] }).Concat((nodes[2] is OperatorNode args) ? args.Children : Array.Empty<Node>()), "Function calling")))
                    .Or(RValueGrammar);
            });

            public static LazyGrammar RValueGrammar => new(() =>
            {
                return
                    LValueGrammar
                    .Or(new BlockGrammar("(", ")", ExpressionGrammar))
                    .Or(LiteralGrammar);
            });

            public static LazyGrammar LValueGrammar => new(() =>
            {
                return
                    MemberedGrammar
                        .ThenNested(
                            new TokenGrammar(TokenType.Punctuator, "[")
                                .Then(ExpressionGrammar)
                                .Then(new TokenGrammar(TokenType.Punctuator, "]"))
                                .AsNode(nodes => new UnaryOperatorNode(nodes[1], "Indexer [..]"))
                            .Or(new TokenGrammar(TokenType.Punctuator, ".")
                                .Then(IdentifierGrammar)
                                .AsNode(nodes => new UnaryOperatorNode(nodes[1], "Member access .")))
                            .Or(new TokenGrammar(TokenType.Punctuator, "->")
                                .Then(IdentifierGrammar).WithError("Invalid -> operator: identifier is missed.")
                                .AsNode(nodes => new UnaryOperatorNode(nodes[1], "Member access ->"))),
                             merge: (l, r) => new BinaryOperatorNode(l, ((UnaryOperatorNode)r).Child, ((UnaryOperatorNode)r).Operator))
                            .Or(IdentifierGrammar);
            });


            public static LazyGrammar MemberedGrammar => new(() =>
            {
                return IdentifierGrammar.Or(new BlockGrammar("(", ")", ExpressionGrammar));
            });


            public static LazyGrammar TypeGrammar => new(() =>
            {
                return
                    (new TokenGrammar(TokenType.Type).Or(new TokenGrammar(TokenType.Punctuator, "*")))
                        .ThenNested(new TokenGrammar(TokenType.Type).Or(new TokenGrammar(TokenType.Punctuator, "*")),
                        merge: (node1, node2) =>
                        {
                            var token2 = ((ValueNode)node2).Token;

                            return node1 switch
                            {
                                ValueNode valueNode1 => new TypesNode([valueNode1.Token, token2]),
                                TypesNode typesNode1 => new TypesNode([.. typesNode1.Types, token2]),

                                _ => throw new Exception("???")
                            };
                        },
                        canEmpty: true);
            });


            public static LazyGrammar IdentifierGrammar => new(() =>
            {
                return new PredicateGrammar(t => (t.TokenType == TokenType.Identifier));
            });

            public static LazyGrammar LiteralGrammar => new(() =>
            {
                return new PredicateGrammar(t => (t.TokenType is LiteralType));
            });


            public static Grammar EmptyGrammar => new EmptyGrammar();

            #endregion
        }

        #region Others
        private class DelegateGrammar : Grammar
        {
            public delegate Node? ParseDelegate(List<Token> tokens, ref int i);

            private ParseDelegate _parse;


            public DelegateGrammar(ParseDelegate parse)
            {
                _parse = parse;
            }

            public override Node? Parse(List<Token> tokens, ref int i)
            {
                return _parse(tokens, ref i);
            }
        }

        // Класс для создания грамматики по необходимости
        private class LazyGrammar : Grammar
        {
            private readonly Lazy<Grammar> _lazy;


            public LazyGrammar(Func<Grammar> factory)
            {
                _lazy = new(factory);
            }

            public override Node? Parse(List<Token> tokens, ref int i)
            {
                return _lazy.Value.Parse(tokens, ref i);
            }
        }

        // Класс проверяет токен на соответствие условию
        private class PredicateGrammar : Grammar
        {
            private readonly Func<Token, bool> _predicate;


            public PredicateGrammar(Func<Token, bool> predicate)
            {
                _predicate = predicate;
            }

            public override Node? Parse(List<Token> tokens, ref int i)
            {
                if (i >= tokens.Count || !_predicate(tokens[i]))
                    return null;

                return new ValueNode(tokens[i++]);
            }
        }

        // Для определения пустого узла
        private class EmptyGrammar : Grammar
        {
            public override Node? Parse(List<Token> tokens, ref int i)
            {
                return new EmptyNode();
            }
        }

        // Класс описания бинарного оператора
        // Хранит левый операнд, правый и оператор
        private class BinaryOperatorGrammar : Grammar
        {
            private readonly Grammar _grammar;


            public BinaryOperatorGrammar(Grammar leftGrammar, Grammar rightGrammar, Grammar operatorGrammar, string? operatorName = null)
            {
                _grammar =
                    leftGrammar
                        .ThenNested(operatorGrammar
                            .Then(rightGrammar)
                            .AsNode(nodes => new ParentNode(nodes)),
                        merge: (node1, node2) =>
                        {
                            return new BinaryOperatorNode(node1, ((ParentNode)node2).Children.ToArray()[1], ((ValueNode)((ParentNode)node2).Children.ToArray()[0]).Token);
                        });
            }

            public override Node? Parse(List<Token> tokens, ref int i)
            {
                return _grammar.Parse(tokens, ref i);
            }
        }

        // Обрабатывает унарные операторы перед операндом
        private class PreUnaryOperatorGrammar : Grammar
        {
            private readonly Grammar _operandGrammar;

            private readonly string _operator;
            private readonly string? _operatorName;


            public PreUnaryOperatorGrammar(Grammar operandGrammar, string op, string? operatorName = null)
            {
                _operandGrammar = operandGrammar;

                _operator = op;
                _operatorName = operatorName;
            }

            public override Node? Parse(List<Token> tokens, ref int i)
            {
                int next = i;

                if (next >= tokens.Count || tokens[next].Value != _operator)
                    return null;

                next++;

                if (_operandGrammar.Parse(tokens, ref next) is not Node operandNode)
                    return null;

                i = next;
                return new UnaryOperatorNode(operandNode, _operatorName ?? _operator);
            }
        }

        // Обрабатываает унарные операторы после операнда
        private class PostUnaryOperatorGrammar : Grammar
        {
            private readonly Grammar _operandGrammar;

            private readonly string _operator;
            private readonly string? _operatorName;


            public PostUnaryOperatorGrammar(Grammar operandGrammar, string op, string? operatorName = null)
            {
                _operandGrammar = operandGrammar;

                _operator = op;
                _operatorName = operatorName;
            }

            public override Node? Parse(List<Token> tokens, ref int i)
            {
                int next = i;

                if (_operandGrammar.Parse(tokens, ref next) is not Node operandNode)
                    return null;


                if (next >= tokens.Count || tokens[next].Value != _operator)
                    return null;

                i = next + 1;
                return new UnaryOperatorNode(operandNode, _operatorName ?? _operator);
            }
        }

        // Проверяет, соответствует ли следующий токен лпределенному типу токена и значению
        private class TokenGrammar : PredicateGrammar
        {
            public TokenGrammar(TokenType tokenType, string? token = null)
                : base(t => t.TokenType == tokenType && (token is null || t.Value == token))
            { }

            // Для создания грамматики, которая может совпаадать с несколькими вариантами токенов
            public static Grammar Any(TokenType tokenType, params string[] tokens)
            {
                return tokens[1..].Aggregate(new TokenGrammar(tokenType, tokens[0]) as Grammar, (sum, token) => sum.Or(new TokenGrammar(tokenType, token)));
            }
        }

        // Класс для обработки блока кода
        private class BlockGrammar : Grammar
        {
            private readonly string _left;
            private readonly string _right;
            private readonly string? _operatorName;

            private readonly Grammar _innerGrammar;


            public BlockGrammar(string left, string right, Grammar innerGrammar, string? operatorName = null)
            {
                _left = left;
                _right = right;

                _innerGrammar = innerGrammar;
                _operatorName = operatorName;
            }

            public override Node? Parse(List<Token> tokens, ref int i)
            {
                int next = i;

                if (next >= tokens.Count || tokens[next].Value != _left)
                    return null;

                next++;

                if (_innerGrammar.Parse(tokens, ref next) is not Node innerNode)
                    throw new SyntaxException($"Invalid block: '{_right}' is missed.");

                if (next >= tokens.Count || tokens[next].Value != _right)
                    throw new SyntaxException($"Invalid block: '{_right}' is missed.");

                i = next + 1;
                return new UnaryOperatorNode(innerNode, _operatorName ?? $"{_left}..{_right}");
            }
        }

        // Для паарсинга граммаатик с разделителем
        private class ListGrammar : Grammar
        {
            private readonly Grammar _elemGrammar;
            private readonly string _operator;
            private readonly string? _separator;

            private readonly bool _canEmpty;


            public ListGrammar(Grammar elemGrammar, string op, string? separator = ",", bool canEmpty = false)
            {
                _elemGrammar = elemGrammar;
                _operator = op;
                _separator = separator;
                _canEmpty = canEmpty;
            }

            public override Node? Parse(List<Token> tokens, ref int i)
            {
                int initial = i;

                LinkedList<Node> nodes = [];

                while (true)
                {
                    if (_elemGrammar.Parse(tokens, ref i) is not Node elemNode)
                    {
                        if (nodes.Count == 0 && _canEmpty || _separator is null)
                        {
                            break;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    nodes.AddLast(elemNode);

                    if (_separator is not null)
                        if (new TokenGrammar(TokenType.Punctuator, _separator).Parse(tokens, ref i) is null)
                            break;
                }

                return new OperatorNode(nodes, _operator);
            }
        }
        #endregion

        #endregion

        #region Nodes
        public abstract class Node
        { }

        // Узел значения
        public class ValueNode : Node
        {
            public Token Token { get; init; }


            public ValueNode(Token token)
            {
                Token = token;
            }
        }

        // Узел типа
        public class TypesNode : Node
        {
            public List<Token> Types { get; init; }

            public TypesNode(params Token[] types)
            {
                Types = new(types);
            }
        }

        // Пустой узел
        public sealed class EmptyNode : Node
        { }

        
        public class ParentNode : Node
        {
            public virtual IEnumerable<Node> Children { get; init; }


            public ParentNode(IEnumerable<Node> children)
            {
                Children = children;
            }
        }

        // Узел для описания операатора
        public class OperatorNode : ParentNode
        {
            public string Operator { get; init; }


            public OperatorNode(IEnumerable<Node> children, string op)
                : base(children)
            {
                Operator = op;
            }

            public OperatorNode(List<Node> children, Token token)
                : base(children)
            {
                Operator = token.Value;
            }
        }

        // Узел унарного уоператора
        public class UnaryOperatorNode : OperatorNode
        {
            public Node Child => Children.First();

            public UnaryOperatorNode(Node child, Token token)
                : base([child], token)
            { }

            public UnaryOperatorNode(Node child, string op)
                : base([child], op)
            { }
        }

        // Узел бинарного оператора
        public class BinaryOperatorNode : OperatorNode
        {
            public BinaryOperatorNode(Node child1, Node child2, Token token)
                : base([child1, child2], token)
            { }

            public BinaryOperatorNode(Node child1, Node child2, string op)
                : base([child1, child2], op)
            { }
        }

        // Узел тернарного выражения
        public class TernaryOperatorNode : OperatorNode
        {
            public TernaryOperatorNode(Node child1, Node child2, Node child3, string op)
                : base([child1, child2, child3], op)
            { }
        }
        #endregion
    }
}
