using static Compiler.Compiler;

public class Program
{
    public static void Main()
    {
        var filePath = "example.c";
        var code = File.ReadAllText(filePath);
        List<Token> tokens = Compiler.Compiler.LexicalAnalysis(code).ToList();

        var root = Compiler.Compiler.SyntaxAnalysis(tokens);

        WriteTree(root);
    }

    private static void WriteTree(Node? node, int i = 0)
    {
        if (node is EmptyNode)
        {
            return;
        }
        if (node is ValueNode tokenNode)
        {
            Console.WriteLine(new string(' ', i * 2) + tokenNode.Token.Value);
        }
        else if (node is OperatorNode operatorNode)
        {
            Console.WriteLine(new string(' ', i * 2) + operatorNode.Operator);

            foreach (Node child in operatorNode.Children)
                WriteTree(child, i + 1);

        }
        else if (node is TypesNode typesNode)
        {
            Console.WriteLine(new string(' ', i * 2) + string.Join(' ', typesNode.Types.Select(t => t.Value)));
        }
    }
}