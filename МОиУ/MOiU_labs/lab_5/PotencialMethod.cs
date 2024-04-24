using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace lab_5
{
    internal static class PotencialMethod
    {
        public static Matrix<double> FindPlan(List<double> a, List<double> b, Matrix<double> c)
        {
            int n = a.Count;
            int m = b.Count;

            Matrix<double> X;
            List<(int i, int j)> B;

            InitialPhase(a, b, out X, out B);
            //Console.WriteLine(string.Join(' ', B));

            while (true)
            {
                // Составляем систему
                Matrix<double> A = DenseMatrix.Create(B.Count + 1, n + m, 0);
                Vector<double> c_col = DenseVector.Create(B.Count + 1, 0);
                int row_id = 0;

                var first = B.First();
                A[row_id, first.i] = 1;
                row_id++;

                foreach ((int i, int j) ind in B)
                {
                    A[row_id, ind.i] = 1;
                    A[row_id, n + ind.j] = 1;
                    c_col[row_id++] = c[ind.i, ind.j];

                }

                Vector<double> systemSol = A.Solve(c_col);
                List<double> u = systemSol.Take(n).ToList();
                List<double> v = systemSol.TakeLast(m).ToList();

                //Console.WriteLine(systemSol);
                //Console.WriteLine(string.Join(' ', u));
                //Console.WriteLine(string.Join(' ', v));

                (int i, int j)? newPos = OptimalityCondition(n, m, u, v, B, c);
                
                if (newPos is null)
                    return X;

                B.Add(newPos.Value);


            }
        }


        private static void InitialPhase(List<double> a, List<double> b, 
                                            out Matrix<double> X, out List<(int i, int j)> B)
        {
            X = DenseMatrix.Create(a.Count, b.Count, 0);
            B = [];

            if (a.Sum() != b.Sum())
            {
                double diff = a.Sum() - b.Sum();

                if (diff > 0)
                {
                     b.Add(diff);
                }
                else
                {
                    a.Add(-diff);
                }
            }

            for (int i = 0, j = 0; i < a.Count && j < b.Count;)
            {
                double min = Math.Min(a[i], b[j]);

                a[i] -= min;
                b[j] -= min;

                X[i, j] = min;
                B.Add((i, j));

                if (a[i] == 0)
                    i++;
                else if (b[j] == 0)
                    j++;
            }

            if (a.Any(x => x != 0) 
                ||  b.Any(x => x != 0) 
                || B.Count != (a.Count + b.Count - 1))
            {
                Console.WriteLine("Что-то пошло не так");
            }
        }

        private static (int i, int j)? OptimalityCondition(int n, int m, 
                                                    List<double> u, List<double> v,
                                                    List<(int i, int j)> B, Matrix<double> c)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (B.Contains((i, j)))
                        continue;

                    if (u[i] + v[j] > c[i, j])
                        return (i, j);
                }
            }

            return null;
        }
    }
}
