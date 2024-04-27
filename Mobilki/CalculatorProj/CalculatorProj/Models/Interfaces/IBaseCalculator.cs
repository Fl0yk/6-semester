namespace CalculatorProj.Models.Interfaces
{
    public interface IBaseCalculator<T>
    {
        public Task<T> Sum(T first, T second);

        public Task<T> Diff(T first, T second);

        public Task<T> Mult(T first, T second);
                
        public Task<T> Div(T first, T second);

        public Task<T> Minus(T digit);

    }
}
