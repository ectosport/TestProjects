namespace Fibonacci
{
    public class FibonacciIterative : ISolveFibonacci
    {
        public int Calculate(int n)
        {
            if (n == 0) return 0;
            else if (n == 1) return 1;

            int total = 0;

            if (n > 1)
            {
                int nMinus1 = 1;
                int nMinus2 = 0;

                for (int i = 1; i < n; ++i)
                {
                    total = nMinus2 + nMinus1;
                    nMinus2 = nMinus1;
                    nMinus1 = total;
                }
            }
            else
            {
                int nPlus2 = 1;
                int nPlus1 = 0;

                for (int i = -1; i >= n; --i)
                {
                    total = nPlus2 - nPlus1;
                    nPlus2 = nPlus1;
                    nPlus1 = total;
                }
            }

            return total;
        }
    }
}