namespace CalculatorProj.Models.Interfaces
{
    public interface IEngineeringCalculator<T> : IBaseCalculator<T>
    {
        public T Pi {  get; }
        public T E {  get; }
        public T Square(T digit);

        public T Cube(T digit);

        public T PowY(T digit, T degree);

        public T Ln(T digit);

        public T Log10(T digit);

        public T Sin(T digit);
        
        public T Cos(T digit);

        public T Tan(T digit);

        public T Sinh(T digit);

        public T Cosh(T digit);

        public T Tanh(T digit);

        public T ePow(T degree);

        public T TenPow(T degree);

        public T Sqrt(T digit);

        public T Yqrt(T digit, T y);

        public T Reverse(T digit);


    }
}
