namespace CalculatorProj.Models.Interfaces
{
    public interface IBaseCalculator<T>
    {
        public T Sum(T first, T second);

        public T Diff(T first, T second);

        public T Mult(T first, T second);
                
        public T Div(T first, T second);

        public T Minus(T digit);

    }
}
