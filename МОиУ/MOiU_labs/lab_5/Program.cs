using lab_5;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

List<double> a = [100, 300, 300];
List<double> b = [300, 200, 200];
Matrix<double> c = DenseMatrix.OfArray(new double[,]{
    {8, 4, 1},
    {8, 4, 3},
    {9, 7, 5 }
});

var res = PotencialMethod.FindPlan(a, b, c);

Console.WriteLine(res);