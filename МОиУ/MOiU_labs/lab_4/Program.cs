using lab_4;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

Matrix<double> a = DenseMatrix.OfArray(new double[,]{
    {-2, -1, -4, 1, 0},
    {-2, -2, -2, 0, 1}
});

Vector<double> c = DenseVector.OfArray([-4, -3, -7, 0, 0]);

Matrix<double> b = DenseVector.OfArray([-1, (double)-3 / 2]).ToColumnMatrix();

List<int> B = [4, 5];

try
{
    var res = DualSimplexMethod.FindingPlan(a, c, b, B);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Результаты!\n");
    Console.WriteLine(string.Join(' ', res));
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\nОшибка: {ex.Message}\n");
    Console.ResetColor();
}