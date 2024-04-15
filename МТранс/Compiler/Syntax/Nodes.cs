using Compiler.Lexis;

namespace Compiler.Syntax
{
    public enum TypeNode
    {
        Program,
        LogicalExpression,
        MathExpression,
        StructureDefinition,
        FunctionDefinition,
        FunctionParameters,
        FunctionCalling,
        FunctionArguments,
        BlockOfCode,
        ReturnStatement,
        StringStatement,
        CharStatement,
        IfElseStatement,
        IfStatement,
        ElseIfStatement,
        ElseStatement,
        ConditionStatement,
        VariableDeclaration,
        Variable,
        ArrayDeclaration,
        ArraySize,
        ArrayElement,
        ArrayIndex,
        Assignment,
        Increment,
        DataSet,
        MemberAccess,
        Whiletatement,
        ForStatement,
        DoWhileStatement,
        LoopKeyWord,
        SwitchStatement,
        SwitchCondition,
        CaseStatement,
        CaseCondition,
        CaseCode,
        DefaultStatement,
        DefaultCode,
        ValueNode,
        ValueType
    }

    public abstract class Node 
    {
        public Node? Parent { get; set; }

        public TypeNode Type { get; set; }
    }

    public class NonTermNode : Node
    {
        public List<Node> Children { get; set; } = [];

        public NonTermNode(TypeNode type, Node? parent) 
        { 
            Type = type;
            Parent = parent;

            if (parent is NonTermNode n)
                n.Children.Add(this);
        }

        public NonTermNode(TypeNode type, IEnumerable<Node> children, Node parent)
        {
            Type = type;
            Parent = parent;
            Children = children.ToList();

            if (parent is NonTermNode n)
                n.Children.Add(this);
        }
    }

    public class ValueNode : Node
    {
        public Token Token { get; set; }

        public ValueNode(Token value, Node parent)
        {
            Token = value;
            Parent = parent;
            Type = TypeNode.ValueNode;

            if (parent is NonTermNode n)
                n.Children.Add(this);
        }
    }

    public class OperatorNode : NonTermNode
    {
        public Token Operator { get; set; }

        public OperatorNode(TypeNode type, Token op, Node parent) : base(type, parent)
        {
            Operator = op;
        }

        public OperatorNode(TypeNode type, Token op, IEnumerable<Node> children, Node parent) : base(type, children, parent)
        {
            Operator = op;
        }
    }

}
