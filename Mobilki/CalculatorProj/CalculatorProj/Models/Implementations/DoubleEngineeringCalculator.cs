using CalculatorProj.Exceptions;
using CalculatorProj.Models.Interfaces;

namespace CalculatorProj.Models.Implementations
{
    public class DoubleEngineeringCalculator : IEngineeringCalculator<double>
    {
        public double Pi => Math.PI;

        public double E => Math.E;

        public double Cos(double digit)
        {   
            double res =  Math.Cos(digit);
            CalculationException.ThrowIfNanOrInfinity(res);
            return res;
        }

        public double Cosh(double digit)
        {
            return Math.Cosh(digit);
        }

        public double Cube(double digit)
        {
            return Math.Pow(digit, 3);
        }

        public double Diff(double first, double second)
        {
            return first - second;
        }

        public double Div(double first, double second)
        {
            if (second == 0)
                throw new CalculationException();

            return first / second;
        }

        public double ePow(double degree)
        {
            return Math.Pow(Math.E, degree);
        }

        public double Ln(double digit)
        {
            return Math.Log(digit);
        }

        public double Log10(double digit)
        {
            return Math.Log10(digit);
        }

        public double Minus(double digit)
        {
            return -digit;
        }

        public double Mult(double first, double second)
        {
            return first * second;
        }

        public double PowY(double digit, double degree)
        {
            return Math.Pow(digit, degree);
        }

        public double Reverse(double digit)
        {
            return 1/digit;
        }

        public double Sin(double digit)
        {
            return Math.Sin(digit);
        }

        public double Sinh(double digit)
        {
            return Math.Sinh(digit);
        }

        public double Sqrt(double digit)
        {
            return Math.Sqrt(digit);
        }

        public double Square(double digit)
        {
            return Math.Pow(digit, 2);
        }

        public double Sum(double first, double second)
        {
            return first + second;
        }

        public double Tan(double digit)
        {
            return Math.Tan(digit);
        }

        public double Tanh(double digit)
        {
            return Math.Tanh(digit);
        }

        public double TenPow(double degree)
        {
            return Math.Pow(10, degree);
        }

        public double Yqrt(double digit, double y)
        {
            if (y == 0)
                throw new CalculationException();

            return Math.Pow(digit, 1 / y);
        }
    }
}
