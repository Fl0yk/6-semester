using lab_1;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Diagnostics.Tracing;

namespace Lab_2
{
    public class SimplexMethod
    {
        public static Matrix<double> MainPhase(Matrix<double> A, Matrix<double> c_T, Matrix<double> x, List<int> B)
        {
            int j0 = 0, k = 0;
            bool isFirstIt = true;
            Matrix<double> A_b_inverted = default;

            while (true)
            {
                Console.WriteLine("================================\nСтарт итерации\n================================\n");
                //1
                Console.WriteLine("Шаг 1. Строим матрицу A_b и находим ее обратную матрицу");
                Matrix<double> A_b = DenseMatrix.OfColumnArrays(B.Select(col => A.Column(col - 1).ToArray()));
                Console.WriteLine(A_b);

                if (isFirstIt)
                {
                    A_b_inverted = A_b.Inverse();
                    isFirstIt = false;
                }
                else
                {
                    A_b_inverted = MatrixInversion.FindInversionMatrix(A_b_inverted, A.Column(j0).ToColumnMatrix(), k + 1);
                }
                
                Console.WriteLine(A_b_inverted);

                //2
                Console.WriteLine("Шаг 2. строим вектор компонент вектора с");
                Matrix<double> c_b_T = DenseVector.OfArray(B.Select(i => c_T[0, i - 1]).ToArray()).ToRowMatrix();
                Console.WriteLine(c_b_T);

                //3
                Console.WriteLine("Шаг 3. Находим вектор потенциалов");
                Matrix<double> u_T = c_b_T * A_b_inverted;
                Console.WriteLine(u_T);

                //4
                Console.WriteLine("Шаг 4. Находим вектор оценок");
                Matrix<double> delta_T = u_T * A - c_T;
                Console.WriteLine(delta_T);

                //5
                //Console.WriteLine("Шаг 5");
                if (delta_T.Row(0).All(val => val >= 0))
                    return x;

                //6
                Console.WriteLine("Шаг 6. Находим в векторе оценок первую отрицательную компоненту и ее индекс");
                j0 = delta_T.Row(0).Find(v => v < 0).Item1;
                Console.WriteLine(j0);

                //7
                Console.WriteLine("Шаг 7. Вычисляем вектор z");
                Vector<double> z = A_b_inverted * A.Column(j0);
                Console.WriteLine(z);

                //8
                Console.WriteLine("Шаг 8. Вычисляем вектор тета");
                List<double> theta_T = [];

                for(int i = 0; i  < B.Count; i++)
                {
                    if (x[B[i] - 1, 0] / z[i] > 0)
                        theta_T.Add(x[B[i] - 1, 0] / z[i]);
                    else
                        theta_T.Add(double.PositiveInfinity);
                }
                Console.WriteLine(string.Join(' ', theta_T));

                //9
                Console.WriteLine("Шаг 9. Находим минимальный эдемент извектора тета");
                double theta_0 = theta_T.Min();
                Console.WriteLine(theta_0);

                //10
                //Console.WriteLine("Шаг 10");
                if (double.IsInfinity(theta_0))
                    throw new Exception("целевой функционал задачи не ограничен сверху на множестве допустимых планов");

                //11
                Console.WriteLine("Шаг 11. Находим индекс, на котором достигается минимум в тета");
                k = theta_T.FindIndex(v => v == theta_0);
                int j_k = B[k];
                Console.WriteLine($"k = {k}, j_k = {j_k}");

                //12
                Console.WriteLine("Шаг 12. Производим замену в мноэестве В");
                B[k] = j0 + 1;
                Console.WriteLine(string.Join(' ', B));

                //13
                Console.WriteLine("Шаг 13. Обновляем компоненты х");
                x[j0, 0] = theta_0;

                for (int i = 0; i < B.Count; i++)
                {
                    if (i != k)
                        x[B[i] - 1, 0] -= theta_0 * z[i];
                }

                x[j_k - 1, 0] = 0;

                Console.WriteLine(x);
                Console.WriteLine("================================\nКонец итерации\n=================================\n");
            }
        }
    }
}
