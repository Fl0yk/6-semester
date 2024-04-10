using CalculatorProj.Exceptions;
using CalculatorProj.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorProj.Models.Implementations
{
    public class DecimalEngineeringCalculator : IEngineeringCalculator<decimal>
    {
        public decimal Pi => throw new NotImplementedException();

        public decimal E => throw new NotImplementedException();

        public decimal Cos(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Cosh(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Cube(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Diff(decimal first, decimal second)
        {
            throw new NotImplementedException();
        }

        public decimal Div(decimal first, decimal second)
        {
            throw new NotImplementedException();
        }

        public decimal ePow(decimal degree)
        {
            throw new NotImplementedException();
        }

        public decimal Ln(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Log10(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Minus(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Mult(decimal first, decimal second)
        {
            throw new NotImplementedException();
        }

        public decimal PowY(decimal digit, decimal degree)
        {
            throw new NotImplementedException();
        }

        public decimal Reverse(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Sin(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Sinh(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Sqrt(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Square(decimal digit)
        {
            var res = Math.Pow((double)digit, 2);
            CalculationException.ThrowIfNanOrInfinity(res);

            return (decimal)res;
        }

        public decimal Sum(decimal first, decimal second)
        {
            throw new NotImplementedException();
        }

        public decimal Tan(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal Tanh(decimal digit)
        {
            throw new NotImplementedException();
        }

        public decimal TenPow(decimal degree)
        {
            throw new NotImplementedException();
        }

        public decimal Yqrt(decimal digit, decimal y)
        {
            throw new NotImplementedException();
        }
    }
}
