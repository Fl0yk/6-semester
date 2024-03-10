using Lab_2;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Runtime.ConstrainedExecution;

Matrix<double> a = DenseMatrix.OfArray(new double[,]{
    {-1, 1, 1, 0, 0},
    {1, 0, 0, 1, 0},
    {0, 1, 0, 0, 1 }
});

Matrix<double> c = DenseVector.OfArray([1, 1, 0, 0, 0]).ToRowMatrix();

Matrix<double> x = DenseVector.OfArray([0, 0, 1, 3, 2]).ToColumnMatrix();

List<int> b = [3, 4, 5];

var res = SimplexMethod.MainPhase(a, c, x, b);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\nРезультаты!\n");
Console.WriteLine(res);
Console.ResetColor();