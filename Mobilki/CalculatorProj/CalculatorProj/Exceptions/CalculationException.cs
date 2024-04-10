using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorProj.Exceptions
{
    internal class CalculationException : Exception
    {
        public CalculationException()
            : base()
        { }

        public CalculationException(string message) 
            : base(message) 
        { }

        public CalculationException (string message, Exception innerException)
            : base(message, innerException) 
        { }

        public static void ThrowIfNanOrInfinity(double digit)
        {
            if (double.IsNaN(digit) || double.IsInfinity(digit))
                throw new CalculationException("Not a number or infinity");
        }
    }
}
