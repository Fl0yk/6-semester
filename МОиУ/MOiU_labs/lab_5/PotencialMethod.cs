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

                int deletedCount;
                List<(int i, int j)> B_copy = new(B);

                do
                {
                    deletedCount = 0;
                    
                    for (int i = 0; i < n; i++)
                    {
                        int count = 0;

                        for (int j = 0; j < m; j++)
                        {
                            if (B_copy.Contains((i, j)))
                                count++;
                        }

                        if (count <= 1)
                        {
                            for (int j = 0; j < m; j++)
                            {
                                if (B_copy.Contains((i, j)))
                                {
                                    deletedCount++;
                                    B_copy.Remove((i, j));
                                }
                            }
                        }
                    }

                    for (int j = 0; j < m; j++)
                    {
                        int count = 0;

                        for (int i = 0; i < n; i++)
                        {
                            if (B_copy.Contains((i, j)))
                                count++;
                        }

                        if (count <= 1)
                        {
                            for (int i = 0; i < n; i++)
                            {
                                if (B_copy.Contains((i, j)))
                                {
                                    deletedCount++;
                                    B_copy.Remove((i, j));
                                }
                            }
                        }
                    }

                } while (deletedCount != 0);


                Dictionary<(int i, int j), int> marked_B = B_copy.ToDictionary(x => x, y => 0);
                marked_B[newPos.Value] = 1;
                IsPlusOrMinus((newPos.Value), ref marked_B);

                // Шаг 5
                double theta = double.PositiveInfinity;
                int min_i = -1, min_j = -1;
                foreach (var pos in B_copy)
                {
                    int i = pos.Item1, j = pos.Item2;
                    if (marked_B.ContainsKey(pos) && marked_B[pos] == -1)
                    {
                        if (theta > X[pos.i, pos.j])
                        {
                            theta = X[pos.i, pos.j];
                            min_i = i;
                            min_j = j;
                        }
                    }
                }

                // Шаг 6
                foreach (var pos in marked_B.Keys.ToList())
                {
                    int i = pos.Item1, j = pos.Item2;
                    if (marked_B[pos] == 1)
                        X[pos.i, pos.j] += theta;
                    else
                        X[pos.i, pos.j] -= theta;

                    if (i == min_i && j == min_j)
                        B.Remove(pos);
                }

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

        
        private static void IsPlusOrMinus((int i, int j) pos, 
                                ref Dictionary<(int i, int j), int> B)
        {
            foreach ((int i, int j) in B.Keys)
            {
                if (pos.i == i)
                {
                    if (B[(i, j)] == 0)
                    {
                        B[(i, j)] = (-1) * (B[pos]);
                        IsPlusOrMinus((i, j), ref B);
                    }
                }
                if (pos.j == j)
                {
                    if (B[(i, j)] == 0)
                    {
                        B[(i, j)] = (-1) * (B[pos]);
                        IsPlusOrMinus((i, j), ref B);
                    }
                }
            }
        }
    }
}
