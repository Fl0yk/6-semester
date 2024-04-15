using Compiler.Lexis;
using Compiler.Semantic;
using Compiler.Syntax;

var filePath = "example.c";
var code = File.ReadAllText(filePath); // Console.ReadLine(); //File.ReadAllText(filePath);
Lexer lexer = new(code);

var res = lexer.Tokenize();

//Console.WriteLine("Tokens\n");
//foreach (var token in res)
    //Console.WriteLine(token);

SyntaxAnalyzer syntax = new(res);
Node tree = syntax.GetSyntaxTree();

//Console.WriteLine("\nSyntax Tree\n");

//SyntaxAnalyzer.WriteTree(tree, "", true);


SemanticAnalyzer semantic = new();
semantic.Analyze(tree);

if (semantic.Errors.Count > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Семантические ошибки:");
    foreach (var error in semantic.Errors)
        Console.WriteLine(error);

    Console.ResetColor();
}
else
{
    Console.WriteLine("\nSyntax Tree\n");

    SyntaxAnalyzer.WriteTree(tree, "", true);
}