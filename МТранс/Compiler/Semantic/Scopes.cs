using Compiler.Syntax;

namespace Compiler.Semantic
{
    public class Scope
    {
        public Scope? Parent { get; set; }
        public Dictionary<string, Variable> Variables { get; set; } = [];
        public Dictionary<string, Function> Functions { get; set; } = [];
        public Dictionary<string, Array> Arrays { get; set; } = [];
        public Dictionary<string, Struct> Structs { get; set; } = [];

        public Scope(Scope? parent)
        {
            Parent = parent;
        }

        public bool ContainsName(string varName)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Variables.ContainsKey(varName) 
                    || cur.Functions.ContainsKey(varName) 
                    || cur.Arrays.ContainsKey(varName)
                    || cur.Structs.ContainsKey(varName))
                {
                    return true;
                }

                cur = cur.Parent;
            }

            return false;
        }

        public bool ContainsName(string varName, bool isCur)
        {
            if (Variables.ContainsKey(varName)
                    || Functions.ContainsKey(varName)
                    || Arrays.ContainsKey(varName)
                    || Structs.ContainsKey(varName))
            {
                return true;
            }

            return false;
        }

        public bool VarContains(string name)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Variables.ContainsKey(name))
                {
                    return true;
                }

                cur = cur.Parent;
            }

            return false;
        }

        public Variable GetVariable(string name)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Variables.ContainsKey(name))
                {
                    return cur.Variables[name];
                }

                cur = cur.Parent;
            }

            throw new InvalidOperationException("Сначала проверь наличие, а затем доставай");
        }

        public bool ArrContains(string name)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Arrays.ContainsKey(name))
                {
                    return true;
                }

                cur = cur.Parent;
            }

            return false;
        }

        public Array GetArr(string name)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Arrays.ContainsKey(name))
                {
                    return cur.Arrays[name];
                }

                cur = cur.Parent;
            }

            throw new InvalidOperationException("Сначала проверь наличие, а затем доставай");
        }


        public bool FuncContains(string name)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Functions.ContainsKey(name))
                {
                    return true;
                }

                cur = cur.Parent;
            }

            return false;
        }

        public Function GetFunction(string name)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Functions.ContainsKey(name))
                {
                    return cur.Functions[name];
                }

                cur = cur.Parent;
            }

            throw new InvalidOperationException("Сначала проверь наличие, а затем доставай");
        }

        public bool StructContains(string name)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Structs.ContainsKey(name))
                {
                    return true;
                }

                cur = cur.Parent;
            }

            return false;
        }

        public Struct GetStruct(string name)
        {
            Scope? cur = this;

            while (cur != null)
            {
                if (cur.Structs.ContainsKey(name))
                {
                    return cur.Structs[name];
                }

                cur = cur.Parent;
            }

            throw new InvalidOperationException("Сначала проверь наличие, а затем доставай");
        }
    }

    public class Variable
    {
        private NonTermNode VarTypes { get; set; }

        public string VarType
        {
            get
            {
                foreach (ValueNode t in VarTypes.Children)
                {
                    if (t.Token.Value != "const")
                        return t.Token.Value;
                }

                throw new Exception("???");
            }
        }

        public string Name { get; set; }

        public bool IsConstant
        {
            get => VarTypes.Children.Select(n => (n as ValueNode)!.Token.Value).Contains("const");
        }

        public Variable(NonTermNode type, ValueNode name)
        {
            VarTypes = type;
            Name = name.Token.Value;
        }
    }

    public class  Function
    {
        public ValueNode FuncType { get; set; }
        public string Name { get; set; }
        public List<Variable> Arguments { get; set; } = [];

        public Function (ValueNode type, ValueNode name)
        {
            FuncType = type;
            Name = name.Token.Value;
        }

        public Function(ValueNode type, ValueNode name, IEnumerable<Variable> args)
        {
            FuncType = type;
            Name = name.Token.Value;
            Arguments = args.ToList();
        }
    }

    public class Array
    {
        public ValueNode ArrType { get; set; }
        public string Name { get; set; }
        public NonTermNode Size { get; set; }

        public Array(ValueNode type, ValueNode name, NonTermNode size)
        {
            ArrType = type;
            Name = name.Token.Value;
            Size = size;
        }
    }

    public class Struct
    {
        public string Name { get; set; }
        public Dictionary<string, Variable> Fields { get; set; } = [];

        public Struct(ValueNode name)
        {
            Name = name.Token.Value;
        }

        public void AddField(Variable field)
        {
            if (Fields.ContainsKey(field.Name))
                throw new SemanticException("Данное поле в структуре уже есть");

            Fields.Add(field.Name, field);
        }
    }

}
