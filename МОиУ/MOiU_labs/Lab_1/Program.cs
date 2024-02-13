using lab_1;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

Matrix<double> a = DenseMatrix.OfArray(new double[,]{
    {1, -1, 0},
    {0, 1, 0},
    {0, 0, 1 }
});

Matrix<double> a_1 = DenseMatrix.OfArray(new double[,]{
    {1, 1, 0},
    {0, 1, 0},
    {0, 0, 1 }
});

Matrix<double> x = DenseMatrix.OfArray(new double[,]
{
    {1},
    {0},
    {1}
});

var res = MatrixInversion.FindInversionMatrix(a_1, x, 3);
Console.WriteLine(res);