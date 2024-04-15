namespace Compiler.Lexis
{
    public enum TokenType
    {
        Keyword, Identifier, Constant, StringLiteral,
        Operator, Punctuator, Unknown
    }

    public class Token
    {
        private static int last_id = 0;

        public int Id { get; private set; }
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public Token(TokenType type, string value)
        {
            Id = last_id++;
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"Type: {Enum.GetName<TokenType>(Type)}; Value: \"{Value}\"";
        }
    }
}
