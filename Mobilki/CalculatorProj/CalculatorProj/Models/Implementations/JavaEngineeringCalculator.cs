using Android.Icu.Number;
using CalculatorProj.Models.Interfaces;
using Java.Lang;
using Java.Math;
using Javax.Security.Auth;

namespace CalculatorProj.Models.Implementations
{
    internal class JavaEngineeringCalculator : IEngineeringCalculator<BigDecimal>
    {
        private RoundOptions RoundingMode = Java.Math.RoundOptions.Down;
        private int _precision = 100;

        public BigDecimal Pi => new(System.Math.PI);

        public BigDecimal E => new(System.Math.E);

        public async Task<BigDecimal> Diff(BigDecimal first, BigDecimal second)
        {
            return await Task.Run(() => first.Subtract(second)!);
        }

        public async Task<BigDecimal> Div(BigDecimal first, BigDecimal second)
        {
            return await Task.Run(() => first.Divide(second, new MathContext(_precision))!);
        }

        public async Task<BigDecimal> Ln(BigDecimal digit)
        {
            if (digit.Signum() <= 0)
            {
                throw new IllegalArgumentException("x <= 0");
            }

            // The number of digits to the left of the decimal point.
            int magnitude = digit.ToString().Length - digit.Scale() - 1;

            if (magnitude < 3)
            {
                return await Task.Run(() => lnNewton(digit, 45)!);
            }

            // Compute magnitude*ln(x^(1/magnitude)).
            else
            {
                // x^(1/magnitude)
                BigDecimal root = await Task.Run(() => digit.Pow(magnitude, new MathContext(_precision)));

                // ln(x^(1/magnitude))
                BigDecimal lnRoot = await Task.Run(() => lnNewton(root, _precision));

                // magnitude*ln(x^(1/magnitude))
                return await Task.Run(() => BigDecimal.ValueOf(magnitude).Multiply(lnRoot)
                            .SetScale(45, BigDecimal.RoundHalfEven)!);
            }
        }

        public async Task<BigDecimal> Minus(BigDecimal digit)
        {
            return await Task.Run(() => digit.Multiply(new BigDecimal(-1))!);
        }

        public async Task<BigDecimal> Mult(BigDecimal first, BigDecimal second)
        {
            return await Task.Run(() => first.Multiply(second, new MathContext(_precision))!);
        }

        public async Task<BigDecimal> PowY(BigDecimal digit, int degree, int scale)
        {
            // If the degree is negative, compute 1/(x^-degree).
            if (degree < 0)
            {
                return BigDecimal.One!
                        .Divide(PowY(digit, -degree, scale).Result, scale, BigDecimal.RoundHalfEven)!;
            }

            BigDecimal power = BigDecimal.One!;

            return await Task.Run(() => Calculation());

            BigDecimal Calculation()
            {
                // Loop to compute value^degree.
                while (degree > 0)
                {

                    // Is the rightmost bit a 1?
                    if ((degree & 1) == 1)
                    {
                        power = power.Multiply(digit).SetScale(scale, BigDecimal.RoundHalfEven);
                    }

                    // Square x and shift degree 1 bit to the right.
                    digit = digit.Multiply(digit).SetScale(scale, BigDecimal.RoundHalfEven);
                    degree >>= 1;

                    Java.Lang.Thread.Yield();
                }

                if (power.Precision() - power.Scale() > 500)
                    return power.Multiply(BigDecimal.One, new MathContext(_precision))!;

                return power!;
            }
        }

        public async Task<BigDecimal> Reverse(BigDecimal digit)
        {
            return await Task.Run(() => BigDecimal.One!.Divide(digit)!);
        }

        public async Task<BigDecimal> Sin(BigDecimal digit)
        {
            BigDecimal lastVal = digit.Add(BigDecimal.One);
            BigDecimal currentValue = digit;
            BigDecimal xSquared = await Task.Run(() => digit.Multiply(digit));
            BigDecimal numerator = digit;
            BigDecimal denominator = BigDecimal.One;
            int i = 0;

            return await Task.Run(() => Calculated());

            BigDecimal Calculated()
            {
                while (lastVal.CompareTo(currentValue) != 0)
                {
                    lastVal = currentValue;

                    int z = 2 * i + 3;

                    denominator = denominator.Multiply(BigDecimal.ValueOf(z));
                    denominator = denominator.Multiply(BigDecimal.ValueOf(z - 1));
                    numerator = numerator.Multiply(xSquared);

                    BigDecimal term = numerator.Divide(denominator, 50, RoundingMode);

                    if (i % 2 == 0)
                    {
                        currentValue = currentValue.Subtract(term);
                    }
                    else
                    {
                        currentValue = currentValue.Add(term);
                    }

                    i++;
                }
                return currentValue!;
            }
        }

        public async Task<BigDecimal> Cos(BigDecimal digit)
        {
            BigDecimal currentValue = BigDecimal.One;
            BigDecimal lastVal = currentValue.Add(BigDecimal.One);
            BigDecimal xSquared = await Task.Run(() => digit.Multiply(digit));
            BigDecimal numerator = BigDecimal.One;
            BigDecimal denominator = BigDecimal.One;
            int i = 0;

            return await Task.Run(() => Calculated());

            BigDecimal Calculated()
            {
                while (lastVal.CompareTo(currentValue) != 0)
                {
                    lastVal = currentValue;

                    int z = 2 * i + 2;

                    denominator = denominator.Multiply(BigDecimal.ValueOf(z));
                    denominator = denominator.Multiply(BigDecimal.ValueOf(z - 1));
                    numerator = numerator.Multiply(xSquared);

                    BigDecimal term = numerator.Divide(denominator, 50, RoundingMode);

                    if (i % 2 == 0)
                    {
                        currentValue = currentValue.Subtract(term);
                    }
                    else
                    {
                        currentValue = currentValue.Add(term);
                    }
                    i++;
                }

                return currentValue!;
            }
        }

        public async Task<BigDecimal> Sqrt(BigDecimal digit)
        {
            return await Task.Run(() => (digit.Sqrt(new MathContext(45))));
        }

        public async Task<BigDecimal> Square(BigDecimal digit)
        {
            if (digit.Precision() - digit.Scale() > 500)
                return await Task.Run(() => digit.Pow(2, new MathContext(10))!);

            return await Task.Run(() => digit.Pow(2)!);
        }

        public async Task<BigDecimal> Sum(BigDecimal first, BigDecimal second)
        {
            return await Task.Run(() => first.Add(second, new MathContext(_precision))!);
        }

        private BigDecimal lnNewton(BigDecimal x, int scale)
        {
            int sp1 = scale + 1;
            BigDecimal n = x;
            BigDecimal term;

            // Convergence tolerance = 5*(10^-(scale+1))
            BigDecimal tolerance = BigDecimal.ValueOf(5)
                                                .MovePointLeft(sp1);

            // Loop until the approximations converge
            // (two successive approximations are within the tolerance).
            do
            {

                // e^x
                BigDecimal eToX = Exp(x, sp1);

                // (e^x - n)/e^x
                term = eToX.Subtract(n)
                            .Divide(eToX, sp1, BigDecimal.RoundDown);

                // x - (e^x - n)/e^x
                x = x.Subtract(term);

                Java.Lang.Thread.Yield();
            } while (term.CompareTo(tolerance) > 0);

            return x.SetScale(45, BigDecimal.RoundHalfEven);
        }

        private BigDecimal Exp(BigDecimal x, int scale)
        {
            // e^0 = 1
            if (x.Signum() == 0)
            {
                return BigDecimal.ValueOf(1);
            }

            // If x is negative, return 1/(e^-x).
            else if (x.Signum() == -1)
            {
                return BigDecimal.ValueOf(1)
                            .Divide(Exp(x.Negate(), scale), scale,
                                    BigDecimal.RoundHalfEven);
            }

            // Compute the whole part of x.
            BigDecimal xWhole = x.SetScale(0, BigDecimal.RoundDown);

            // If there isn't a whole part, compute and return await Task.Run( () =>  e^x.
            if (xWhole.Signum() == 0) return expTaylor(x, scale);

            // Compute the fraction part of x.
            BigDecimal xFraction = x.Subtract(xWhole);

            // z = 1 + fraction/whole
            BigDecimal z = BigDecimal.ValueOf(1)
                                .Add(xFraction.Divide(
                                        xWhole, scale,
                                        BigDecimal.RoundHalfEven));

            // t = e^z
            BigDecimal t = expTaylor(z, scale);

            BigDecimal maxLong = BigDecimal.ValueOf(Long.MaxValue);
            BigDecimal result = BigDecimal.ValueOf(1);

            // Compute and return await Task.Run( () =>  t^whole using IntPower().
            // If whole > Long.MaxValue, then first compute products
            // of e^Long.MaxValue.
            while (xWhole.CompareTo(maxLong) >= 0)
            {
                result = result.Multiply(t.Pow(int.MaxValue, new MathContext(scale)))
                            .SetScale(scale, BigDecimal.RoundHalfEven);
                xWhole = xWhole.Subtract(maxLong);

                Java.Lang.Thread.Yield();
            }
            return result.Multiply(t.Pow(xWhole.IntValue(), new MathContext(scale)))
                            .SetScale(scale, BigDecimal.RoundHalfEven);
        }

        private BigDecimal expTaylor(BigDecimal x, int scale)
        {
            BigDecimal factorial = BigDecimal.ValueOf(1);
            BigDecimal xPower = x;
            BigDecimal sumPrev;

            // 1 + x
            BigDecimal sum = x.Add(BigDecimal.ValueOf(1));

            // Loop until the sums converge
            // (two successive sums are equal after rounding).
            int i = 2;
            do
            {
                // x^i
                xPower = xPower.Multiply(x).SetScale(scale, BigDecimal.RoundHalfEven);

                // i!
                factorial = factorial.Multiply(BigDecimal.ValueOf(i));

                // x^i/i!
                BigDecimal term = xPower.Divide(factorial, scale, BigDecimal.RoundHalfEven);

                // sum = sum + x^i/i!
                sumPrev = sum;
                sum = sum.Add(term);

                ++i;
                Java.Lang.Thread.Yield();
            } while (sum.CompareTo(sumPrev) != 0);

            return sum;
        }
    }
}
