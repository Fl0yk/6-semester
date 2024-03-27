using lab_1;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lab_2
{
    public class SimplexMethod
    {
        public static (Vector<double> x, List<int> B) InitialPhase(Matrix<double> A, Vector<double> c, List<double> b)
        {
            int n = A.ColumnCount, m = A.RowCount;

            // Шаг 1
            for (int i = 0; i < b.Count; i++)
            {
                // Шаг 1
                if (b[i] < 0)
                {
                    b[i] *= -1;
                    A.SetRow(i, A.Row(i).Map(el => -el));
                }
            }

            // Шаг 2
            Vector<double> c_kr = DenseVector.OfEnumerable(Enumerable.Repeat(0d, n).Concat(Enumerable.Repeat(-1d, m)));
            Matrix<double> A_kr = A.Append(DenseMatrix.CreateIdentity(m));

            // Шаг 3
            Vector<double> x_kr = DenseVector.OfEnumerable(Enumerable.Repeat(0d, n).Concat(b));
            List<int> B = Enumerable.Range(n + 1, m).ToList();

            // Шаг 4
            (x_kr, B) = MainPhase(A_kr, c_kr.ToRowMatrix(), x_kr, B);

            // Шаг 5
            if (x_kr.Skip(A.ColumnCount).Any(el => el != 0))
                throw new Exception("Задача несовместна :(");

            // Шаг 6
            Vector<double> x = DenseVector.OfArray(x_kr.Take(n).ToArray());

            while (true)
            {
                // Шаг 7
                if (B.All(el => el <= n))
                    return (x, B);

                // Шаг 8
                int j_k = B.Max();
                int k = B.Max() - n - 1;

                // Шаг 9
                List<int> j_list = Enumerable.Range(1, n).Except(B).ToList();
                Matrix<double> A_b_inv = DenseMatrix.OfColumnArrays(B.Select(col => A_kr.Column(col - 1).ToArray())).Inverse();

                Vector <double>[] l = j_list.Select(ind => A_b_inv * A_kr.Column(ind - 1)).ToArray();

                // Шаг 10
                bool nashel = false;

                for (int i = 0; i < l.Length; i++)
                {
                    if (l[i][k] != 0)
                    {
                        B[k] = i + 1;
                        nashel = true;
                        break;
                    }
                }

                // Шаг 11
                if (!nashel)
                {
                    int i = j_k - n - 1;

                    A_kr.RemoveRow(i);
                    b.RemoveAt(i);

                    B.RemoveAt(i);
                    A.RemoveRow(i);
                }
            }
        }

        public static (Vector<double> x, List<int> B) MainPhase(Matrix<double> A, Matrix<double> c_T, Vector<double> x, List<int> B)
        {
            int j0 = 0, k = 0;
            bool isFirstIt = true;
            Matrix<double> A_b_inverted = default;

            while (true)
            {
                //Console.WriteLine("================================\nСтарт итерации\n================================\n");
                //1
                //Console.WriteLine("Шаг 1. Строим матрицу A_b и находим ее обратную матрицу");
                Matrix<double> A_b = DenseMatrix.OfColumnArrays(B.Select(col => A.Column(col - 1).ToArray()));
                //Console.WriteLine(A_b);

                if (isFirstIt)
                {
                    A_b_inverted = A_b.Inverse();
                    isFirstIt = false;
                }
                else
                {
                    A_b_inverted = MatrixInversion.FindInversionMatrix(A_b_inverted, A.Column(j0).ToColumnMatrix(), k + 1);
                }
                
                //Console.WriteLine(A_b_inverted);

                //2
                //Console.WriteLine("Шаг 2. строим вектор компонент вектора с");
                Matrix<double> c_b_T = DenseVector.OfArray(B.Select(i => c_T[0, i - 1]).ToArray()).ToRowMatrix();
                //Console.WriteLine(c_b_T);

                //3
                //Console.WriteLine("Шаг 3. Находим вектор потенциалов");
                Matrix<double> u_T = c_b_T * A_b_inverted;
                //Console.WriteLine(u_T);

                //4
                //Console.WriteLine("Шаг 4. Находим вектор оценок");
                Matrix<double> delta_T = u_T * A - c_T;
                //Console.WriteLine(delta_T);

                //5
                //Console.WriteLine("Шаг 5");
                if (delta_T.Row(0).All(val => val >= 0))
                    return (x, B);

                //6
                //Console.WriteLine("Шаг 6. Находим в векторе оценок первую отрицательную компоненту и ее индекс");
                j0 = delta_T.Row(0).Find(v => v < 0).Item1;
                //Console.WriteLine(j0);

                //7
                //Console.WriteLine("Шаг 7. Вычисляем вектор z");
                Vector<double> z = A_b_inverted * A.Column(j0);
                //Console.WriteLine(z);

                //8
                //Console.WriteLine("Шаг 8. Вычисляем вектор тета");
                List<double> theta_T = [];

                for(int i = 0; i  < B.Count; i++)
                {
                    if ( z[i] > 0)
                        theta_T.Add(x[B[i] - 1] / z[i]);
                    else
                        theta_T.Add(double.PositiveInfinity);
                }
                //Console.WriteLine(string.Join(' ', theta_T));

                //9
                //Console.WriteLine("Шаг 9. Находим минимальный эдемент извектора тета");
                double theta_0 = theta_T.Min();
                //Console.WriteLine(theta_0);

                //10
                //Console.WriteLine("Шаг 10");
                if (double.IsInfinity(theta_0))
                    throw new Exception("целевой функционал задачи не ограничен сверху на множестве допустимых планов");

                //11
                //Console.WriteLine("Шаг 11. Находим индекс, на котором достигается минимум в тета");
                k = theta_T.FindIndex(v => v == theta_0);
                int j_k = B[k];
                //Console.WriteLine($"k = {k}, j_k = {j_k}");

                //12
                //Console.WriteLine("Шаг 12. Производим замену в мноэестве В");
                B[k] = j0 + 1;
                //Console.WriteLine(string.Join(' ', B));

                //13
                //Console.WriteLine("Шаг 13. Обновляем компоненты х");
                x[j0] = theta_0;

                for (int i = 0; i < B.Count; i++)
                {
                    if (i != k)
                        x[B[i] - 1] -= theta_0 * z[i];
                }

                x[j_k - 1] = 0;

                //Console.WriteLine(x);
                //Console.WriteLine("================================\nКонец итерации\n=================================\n");
            }
        }
    }
}
