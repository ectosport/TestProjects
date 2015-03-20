namespace Fibonacci
{
    public class FibonacciIterative : ISolveFibonacci
    {
        public int Calculate(int n)
        {
            if (n == 0) return 0;
            else if (n == 1) return 1;

            int nMinus1 = 1;
            int nMinus2 = 0;
            int total = 0;
            for (int i = 1; i < n; ++i)
            {
                total = nMinus2 + nMinus1;
                nMinus2 = nMinus1;
                nMinus1 = total;
            }

            return total;
        }
    }
}