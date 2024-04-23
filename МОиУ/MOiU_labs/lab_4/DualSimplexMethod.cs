using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace lab_4
{
    internal static class DualSimplexMethod
    {
        public static double[] FindingPlan(Matrix<double> A, Vector<double> c_T, Matrix<double> b, List<int> B)
        {

            while (true)
            {
                Matrix<double> A_b = DenseMatrix.OfColumnArrays(B.Select(col => A.Column(col - 1).ToArray()));
                Matrix<double> A_b_inv = A_b.Inverse();
                Vector<double> c_b_T = DenseVector.OfEnumerable(B.Select(i => c_T[i - 1]));

                Vector<double> y_T = c_b_T * A_b_inv;

                double[] psevdoplan = new double[c_T.Count];
                Matrix<double> psevdoplan_B = A_b_inv * b;
                int j = -1;
                double min = double.MaxValue;

                for (int i = 0; i < B.Count; i++)
                {
                    psevdoplan[B[i] - 1] = psevdoplan_B.Row(i)[0];
                    if (min >= psevdoplan[B[i] - 1])
                    {
                        j = i;
                        min = psevdoplan[B[i] - 1];
                    }
                }

                if (psevdoplan.All(el => el >= 0))
                    return psevdoplan;

                //int j = psevdoplan.ToList().IndexOf(psevdoplan.Min());
                Vector<double> delta_y = A_b_inv.Row(j);

                List<(int index, double val)> mu = [];
                List<(int index, double val)> sigma = [];


                foreach (int ind in Enumerable.Range(1, c_T.Count).Except(B))
                {
                    int i = ind - 1;
                    mu.Add((ind, delta_y * A.Column(i)));

                    if (mu.Last().val < 0)
                    {
                        double sig = (c_T[i] - y_T * A.Column(i)) / mu.Last().val;
                        sigma.Add(new(ind, sig));
                    }
                }

                if (mu.All(el => el.val >= 0))
                {
                    throw new Exception("Прямая задача не совместна:(");
                }

                (int index, double val) sigma_0 = sigma.First(v => v.val == sigma.Min(s => s.val));

                B[j] = sigma_0.index;
            }

        }
    }
}
