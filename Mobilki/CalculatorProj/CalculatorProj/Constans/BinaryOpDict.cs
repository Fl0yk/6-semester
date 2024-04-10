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
        public static readonly Dictionary<string, BinaryOpEnum> BinaryOpDict = new()
        {
            { "+", BinaryOpEnum.Sum },
            { "-", BinaryOpEnum.Diff },
            { "*", BinaryOpEnum.Mult },
            { "/", BinaryOpEnum.Div },
            { "y√x", BinaryOpEnum.Yqrt },
            {"x^y", BinaryOpEnum.PowY }
        };
    }
}
