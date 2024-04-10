using Compiler;
using static Compiler.Compiler;

public class Program
{
    public static void Main()
    {
        var filePath = "example.c";
        var code = File.ReadAllText(filePath);
        List<Token> tokens = Compiler.Compiler.LexicalAnalysis(code).ToList();

        try
        {
            var root = Compiler.Compiler.SyntaxAnalysis(tokens);
            SemanticAnalyzer.CheckSemantic(root);
            WriteTree(root, "", true);
        }
        catch (SyntaxException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Syntax error");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        catch (SemanticAnalyzer.SemanticException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Semantic error");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }

    private static void WriteTree(Node node, String indent, bool last)
    {
        if (node is EmptyNode)
        {
            return;
        }
        if (node is ValueNode tokenNode)
        {
            Console.WriteLine(indent + "+- " + tokenNode.Token.Value);
            indent += last ? "   " : "|  ";
        }
        else if (node is OperatorNode operatorNode)
        {
            Console.WriteLine(indent + "+- " + operatorNode.Operator);
            indent += last ? "   " : "|  ";

            for (int i = 0; i < operatorNode.Children.Count(); i++)
            {
                WriteTree(operatorNode.Children.ToArray()[i], indent, i == operatorNode.Children.Count() - 1);
            }

        }
        else if (node is TypesNode typesNode)
        {
            Console.WriteLine(indent + "+- " + string.Join(' ', typesNode.Types.Select(t => t.Value)));
            indent += last ? "   " : "|  ";
        }
    }
}