using CalculatorProj.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorProj.Constans
{
    public partial class Constants
    {
        public static readonly Dictionary<string, UnaryOpEnum> UnaryOpDict = new()
        {
            { "+/-", UnaryOpEnum.Minus },
            { "x^2", UnaryOpEnum.Pow2 },
            { "x^3", UnaryOpEnum.Pow3 },
            { "sqrt", UnaryOpEnum.Sqrt },
            { "1/x", UnaryOpEnum.Reverse },
            { "10^x", UnaryOpEnum.TenPow },
            { "e^x", UnaryOpEnum.ePow },
            { "ln", UnaryOpEnum.Ln },
            { "log10", UnaryOpEnum.LogTen },
            { "sin", UnaryOpEnum.Sin },
            { "cos", UnaryOpEnum.Cos },
            { "tan", UnaryOpEnum.Tan },
            { "sinh", UnaryOpEnum.Sinh },
            { "cosh", UnaryOpEnum.Cosh },
            { "tanh", UnaryOpEnum.Tanh }
        };
    }
}
