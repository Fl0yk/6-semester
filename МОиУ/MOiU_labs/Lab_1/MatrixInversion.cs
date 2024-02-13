using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace lab_1
{
    public class MatrixInversion
    {
        public static Matrix<double> FindInversionMatrix(Matrix<double> a_1, Matrix<double> x, int i)
        {
            int size = a_1.RowCount;

            //1
            var l = a_1 * x;

            if (l[--i, 0] == 0)
                throw new Exception("Матрица необратима");
            //2
            var l_i = l[i, 0];
            l[i, 0] = -1;
            //3
            var l_krisha = (-1 / l_i) * l;
            //4
            Matrix<double> q = DenseMatrix.CreateDiagonal(size, size, 1);

            for (int j = 0; j < size; j++)
            {
                q[j, i] = l_krisha[j, 0];
            }

            //5
            Matrix<double> res = DenseMatrix.Create(size, size, 0);

            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    if (j == i)
                    {
                        res[j, k] = q[j, j] * a_1[k, i];
                        continue;
                    }

                    res[j, k] = q[j, j] * a_1[j, k] + q[j, i] * a_1[k, i];
                }
            }

            return res;
        }
    }
}
