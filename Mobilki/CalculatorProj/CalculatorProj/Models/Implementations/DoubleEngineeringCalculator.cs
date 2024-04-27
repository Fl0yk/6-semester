using CalculatorProj.Exceptions;
using CalculatorProj.Models.Interfaces;

namespace CalculatorProj.Models.Implementations
{
    public class DoubleEngineeringCalculator : IEngineeringCalculator<double>
    {
        public double Pi => throw new NotImplementedException();

        public double E => throw new NotImplementedException();

        public Task<double> Cos(double digit)
        {
            throw new NotImplementedException();
        }

        public Task<double> Diff(double first, double second)
        {
            throw new NotImplementedException();
        }

        public Task<double> Div(double first, double second)
        {
            throw new NotImplementedException();
        }

        public Task<double> Ln(double digit)
        {
            throw new NotImplementedException();
        }

        public Task<double> Minus(double digit)
        {
            throw new NotImplementedException();
        }

        public Task<double> Mult(double first, double second)
        {
            throw new NotImplementedException();
        }

        public Task<double> PowY(double digit, double degree)
        {
            throw new NotImplementedException();
        }

        public Task<double> PowY(double digit, int degree, int scale)
        {
            throw new NotImplementedException();
        }

        public Task<double> Reverse(double digit)
        {
            throw new NotImplementedException();
        }

        public Task<double> Sin(double digit)
        {
            throw new NotImplementedException();
        }

        public Task<double> Sqrt(double digit)
        {
            throw new NotImplementedException();
        }

        public Task<double> Square(double digit)
        {
            throw new NotImplementedException();
        }

        public Task<double> Sum(double first, double second)
        {
            throw new NotImplementedException();
        }
    }
}
