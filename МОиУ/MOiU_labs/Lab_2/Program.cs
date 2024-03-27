using Lab_2;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

Matrix<double> a = DenseMatrix.OfArray(new double[,]{
    {1, 1, 1},
    {2, 2, 2}
});

Vector<double> c = DenseVector.OfArray([1, 0, 0]);

List<double> b = [0, 0];

try
{
    var res = SimplexMethod.InitialPhase(a, c, b);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\nРезультаты!\n");
    Console.WriteLine(res.x);
    Console.WriteLine(string.Join(' ', res.B));
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\nОшибка: {ex.Message}\n");
    Console.ResetColor();
}