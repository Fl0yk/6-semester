namespace CalculatorProj.Models.Interfaces
{
    public interface IEngineeringCalculator<T> : IBaseCalculator<T>
    {
        public T Pi {  get; }
        public T E {  get; }
        public Task<T> Square(T digit);

        public Task<T> PowY(T digit, int degree, int scale);

        public Task<T> Ln(T digit);

        public Task<T> Sin(T digit);
        
        public Task<T> Cos(T digit);

        public Task<T> Sqrt(T digit);

        public Task<T> Reverse(T digit);
    }
}
